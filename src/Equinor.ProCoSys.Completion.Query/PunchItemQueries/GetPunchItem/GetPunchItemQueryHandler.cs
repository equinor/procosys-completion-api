using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItem;

public class GetPunchItemQueryHandler : IRequestHandler<GetPunchItemQuery, Result<PunchItemDetailsDto>>
{
    private readonly IReadOnlyContext _context;

    public GetPunchItemQueryHandler(IReadOnlyContext context) => _context = context;

    public async Task<Result<PunchItemDetailsDto>> Handle(GetPunchItemQuery request, CancellationToken cancellationToken)
    {
        var punchItem =
            await (from pi in _context.QuerySet<PunchItem>()
                        .Include(p => p.CreatedBy)
                        .Include(p => p.ModifiedBy)
                        .Include(p => p.ClearedBy)
                        .Include(p => p.RejectedBy)
                        .Include(p => p.VerifiedBy)
                        .Include(p => p.Project)
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
                   where pi.Guid == request.PunchItemGuid
                   select pi)
                .TagWith($"{nameof(GetPunchItemQueryHandler)}.{nameof(Handle)}")
                .SingleOrDefaultAsync(cancellationToken);

        if (punchItem is null)
        {
            throw new Exception($"PunchItem with Guid {request.PunchItemGuid} not found");
        }

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

        var punchItemDetailsDto = new PunchItemDetailsDto(
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
                       punchItem.RowVersion.ConvertToString());
        return new SuccessResult<PunchItemDetailsDto>(punchItemDetailsDto);
    }

    private SWCRDto? MapToSWCRDto(SWCR? swcr)
        => swcr is null
            ? null
            : new SWCRDto(swcr.Guid, swcr.No);

    private DocumentDto? MapToDocumentDto(Document? document)
        => document is null
            ? null
            : new DocumentDto(document.Guid, document.No);

    private WorkOrderDto? MapToWorkOrderDto(WorkOrder? workOrder)
        => workOrder is null
            ? null
            : new WorkOrderDto(workOrder.Guid, workOrder.No);

    private LibraryItemDto? MapToLibraryItemDto(LibraryItem? libraryItem)
        => libraryItem is null
            ? null
            : new LibraryItemDto(libraryItem.Guid, libraryItem.Code, libraryItem.Description);

    private static PersonDto? MapToPersonDto(Person? person)
        => person is null
            ? null
            : new PersonDto(person.Guid,
                person.FirstName,
                person.LastName,
                person.UserName,
                person.Email);
}
