using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Query.PunchItemServices;

public class PunchItemService(IReadOnlyContext context) : IPunchItemService
{
    public async Task<PunchItemDetailsDto> GetByGuid(Guid guid, CancellationToken cancellationToken)
    {
        var whereClause = new Func<IQueryable<PunchItem>, IQueryable<PunchItem>>(query =>
            query.Where(pi => pi.Guid == guid)
        );
        var punchItem = (await GetPunchItems(whereClause, cancellationToken)).FirstOrDefault() 
                        ?? throw new EntityNotFoundException<PunchItem>(guid);
        var attCount = await context.QuerySet<Attachment>()
           .Where(x => x.ParentGuid == punchItem.Guid).CountAsync(cancellationToken);

        return MapPunchToDto(punchItem, attCount);
    }

    public async Task<IReadOnlyCollection<PunchItemDetailsDto>> GetByCheckListGuid(Guid guid, CancellationToken cancellationToken)
    {
        var whereClause = new Func<IQueryable<PunchItem>, IQueryable<PunchItem>>(query =>
            query.Where(pi => pi.CheckListGuid == guid)
        );
        var punchItems = await GetPunchItems(whereClause, cancellationToken);
        var punchItemGuids = punchItems.Select(y => y.Guid);

        var attachmentCounts = await context.QuerySet<Attachment>()
            .Where(a => punchItemGuids.Contains(a.ParentGuid))
            .GroupBy(a => a.ParentGuid)
            .ToDictionaryAsync(k => k.Key, v => v.Count()
                , cancellationToken: cancellationToken);

        return punchItems.Select(p => MapPunchToDto(p, 
            attachmentCounts.GetValueOrDefault(p.Guid, 0))).ToImmutableList();
    }

    private async Task<List<PunchItem>> GetPunchItems(Func<IQueryable<PunchItem>, IQueryable<PunchItem>> whereClause, CancellationToken cancellationToken)
    {
        var query = context.QuerySet<PunchItem>()
            .Include(p => p.Project)
            .Include(p => p.CreatedBy)
            .Include(p => p.ModifiedBy)
            .Include(p => p.ClearedBy)
            .Include(p => p.RejectedBy)
            .Include(p => p.VerifiedBy)
            .Include(p => p.RaisedByOrg)
            .Include(p => p.ClearingByOrg)
            .Include(p => p.Priority)
            .Include(p => p.Sorting)
            .Include(p => p.Type)
            .Include(p => p.ActionBy)
            .Include(p => p.WorkOrder)
            .Include(p => p.OriginalWorkOrder)
            .Include(p => p.SWCR)
            .Include(p => p.Document)
            .AsQueryable();
        query = whereClause(query);

        return await (query)
                .TagWith($"{nameof(PunchItemService)}.{nameof(GetPunchItems)}").ToListAsync(cancellationToken);
    }

    private static PunchItemDetailsDto MapPunchToDto(PunchItem punchItem, int attachmentCount)
    {
        var createdBy = MapToPersonDto(punchItem.CreatedBy)!;
        var modifiedBy = MapToPersonDto(punchItem.ModifiedBy);
        var clearedBy = MapToPersonDto(punchItem.ClearedBy);
        var rejectedBy = MapToPersonDto(punchItem.RejectedBy);
        var verifiedBy = MapToPersonDto(punchItem.VerifiedBy);
        var raisedByOrg = MapToLibraryItemDto(punchItem.RaisedByOrg)!;
        var clearingByOrg = MapToLibraryItemDto(punchItem.ClearingByOrg)!;
        var sorting = MapToLibraryItemDto(punchItem.Sorting);
        var priority = MapToLibraryItemDto(punchItem.Priority);
        var type = MapToLibraryItemDto(punchItem.Type);
        var actionBy = MapToPersonDto(punchItem.ActionBy);
        var workOrderDto = MapToWorkOrderDto(punchItem.WorkOrder);
        var originalWorkOrderDto = MapToWorkOrderDto(punchItem.OriginalWorkOrder);
        var documentDto = MapToDocumentDto(punchItem.Document);
        var swcrDto = MapToSWCRDto(punchItem.SWCR);

        return new PunchItemDetailsDto(
            punchItem.Guid,
            punchItem.Project.Name,
            punchItem.ItemNo,
            punchItem.Category.ToString(),
            punchItem.Description,
            createdBy,
            punchItem.CreatedAtUtc,
            modifiedBy,
            punchItem.ModifiedAtUtc,
            punchItem.IsReadyToBeCleared,
            punchItem.IsReadyToBeUncleared,
            clearedBy,
            punchItem.ClearedAtUtc,
            punchItem.IsReadyToBeRejected,
            rejectedBy,
            punchItem.RejectedAtUtc,
            punchItem.IsReadyToBeVerified,
            punchItem.IsReadyToBeUnverified,
            verifiedBy,
            punchItem.VerifiedAtUtc,
            raisedByOrg,
            clearingByOrg,
            priority,
            sorting,
            type,
            actionBy,
            punchItem.DueTimeUtc,
            punchItem.Estimate,
            punchItem.ExternalItemNo,
            punchItem.MaterialRequired,
            punchItem.MaterialETAUtc,
            punchItem.MaterialExternalNo, 
            workOrderDto,
            originalWorkOrderDto,
            documentDto,
            swcrDto,
            attachmentCount,
            punchItem.RowVersion.ConvertToString(),
            punchItem.CheckListGuid);
    }

    private static SWCRDto? MapToSWCRDto(SWCR? swcr)
        => swcr is null
            ? null
            : new SWCRDto(swcr.Guid, swcr.No);

    private static DocumentDto? MapToDocumentDto(Document? document)
        => document is null
            ? null
            : new DocumentDto(document.Guid, document.No);

    private static WorkOrderDto? MapToWorkOrderDto(WorkOrder? workOrder)
        => workOrder is null
            ? null
            : new WorkOrderDto(workOrder.Guid, workOrder.No);

    private static LibraryItemDto? MapToLibraryItemDto(LibraryItem? libraryItem)
        => libraryItem is null
            ? null
            : new LibraryItemDto(libraryItem.Guid, libraryItem.Code, libraryItem.Description, libraryItem.Type.ToString());

    private static PersonDto? MapToPersonDto(Person? person)
        => person is null
            ? null
            : new PersonDto(person.Guid,
                person.FirstName,
                person.LastName,
                person.UserName,
                person.Email);
}
