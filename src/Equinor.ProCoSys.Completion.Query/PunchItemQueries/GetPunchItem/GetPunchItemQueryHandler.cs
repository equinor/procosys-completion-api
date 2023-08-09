using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
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
        var dto =
            await (from punchItem in _context.QuerySet<PunchItem>()
                   join project in _context.QuerySet<Project>()
                       on punchItem.ProjectId equals project.Id
                   join createdByUser in _context.QuerySet<Person>()
                       on punchItem.CreatedById equals createdByUser.Id
                   from modifiedByUser in _context.QuerySet<Person>()
                       .Where(p => p.Id == punchItem.ModifiedById).DefaultIfEmpty() //left join!
                   from clearedByUser in _context.QuerySet<Person>()
                       .Where(p => p.Id == punchItem.ClearedById).DefaultIfEmpty() //left join!                   
                   from rejectedByUser in _context.QuerySet<Person>()
                       .Where(p => p.Id == punchItem.RejectedById).DefaultIfEmpty() //left join!                   
                   from verifiedByUser in _context.QuerySet<Person>()
                       .Where(p => p.Id == punchItem.VerifiedById).DefaultIfEmpty() //left join!                   
                   join raisedByOrgItem in _context.QuerySet<LibraryItem>()
                       on punchItem.RaisedByOrgId equals raisedByOrgItem.Id
                   join clearingByOrgItem in _context.QuerySet<LibraryItem>()
                       on punchItem.ClearingByOrgId equals clearingByOrgItem.Id
                   from priorityItem in _context.QuerySet<LibraryItem>()
                       .Where(l => l.Id == punchItem.PriorityId).DefaultIfEmpty() //left join!
                   from sortingItem in _context.QuerySet<LibraryItem>()
                       .Where(l => l.Id == punchItem.SortingId).DefaultIfEmpty() //left join!
                   from typeItem in _context.QuerySet<LibraryItem>()
                       .Where(l => l.Id == punchItem.TypeId).DefaultIfEmpty() //left join!
                   where punchItem.Guid == request.PunchItemGuid
                   select new {
                       PunchItem = punchItem,
                       Project = project,
                       CreatedBy = createdByUser, 
                       ModifiedBy = modifiedByUser,
                       ClearedBy = clearedByUser,
                       RejectedBy = rejectedByUser,
                       VerifiedBy = verifiedByUser,
                       RaisedByOrg = raisedByOrgItem,
                       ClearingByOrg = clearingByOrgItem,
                       Priority = priorityItem,
                       Sorting = sortingItem,
                       Type = typeItem
                   })
                .TagWith($"{nameof(GetPunchItemQueryHandler)}.{nameof(Handle)}")
                .SingleOrDefaultAsync(cancellationToken);

        if (dto is null)
        {
            return new NotFoundResult<PunchItemDetailsDto>(Strings.EntityNotFound(nameof(PunchItem), request.PunchItemGuid));
        }

        var createdBy = MapToPersonDto(dto.CreatedBy)!;
        var modifiedBy = MapToPersonDto(dto.ModifiedBy);
        var clearedBy = MapToPersonDto(dto.ClearedBy);
        var rejectedBy = MapToPersonDto(dto.RejectedBy);
        var verifiedBy = MapToPersonDto(dto.VerifiedBy);
        var raisedByOrg = MapToLibraryItemDto(dto.RaisedByOrg)!;
        var clearingByOrg = MapToLibraryItemDto(dto.ClearingByOrg)!;
        var sorting = MapToLibraryItemDto(dto.Sorting);
        var priority = MapToLibraryItemDto(dto.Priority);
        var type = MapToLibraryItemDto(dto.Type);

        var punchItemDetailsDto = new PunchItemDetailsDto(
                       dto.PunchItem.Guid,
                       dto.Project.Name,
                       dto.PunchItem.ItemNo,
                       dto.PunchItem.Description,
                       createdBy,
                       dto.PunchItem.CreatedAtUtc,
                       modifiedBy,
                       dto.PunchItem.ModifiedAtUtc,
                       dto.PunchItem.IsReadyToBeCleared,
                       dto.PunchItem.IsReadyToBeUncleared,
                       clearedBy,
                       dto.PunchItem.ClearedAtUtc,
                       dto.PunchItem.IsReadyToBeRejected,
                       rejectedBy,
                       dto.PunchItem.RejectedAtUtc,
                       dto.PunchItem.IsReadyToBeVerified,
                       dto.PunchItem.IsReadyToBeUnverified,
                       verifiedBy,
                       dto.PunchItem.VerifiedAtUtc,
                       raisedByOrg,
                       clearingByOrg,
                       priority,
                       sorting,
                       type,
                       dto.PunchItem.RowVersion.ConvertToString());
        return new SuccessResult<PunchItemDetailsDto>(punchItemDetailsDto);
    }

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
