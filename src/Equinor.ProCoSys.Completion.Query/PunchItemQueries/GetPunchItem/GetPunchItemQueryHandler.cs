using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
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
                   where punchItem.Guid == request.PunchItemGuid
                   select new {
                       PunchItem = punchItem,
                       Project = project,
                       CreatedByUser = createdByUser, 
                       ModifiedByUser = modifiedByUser
                })
                .TagWith($"{nameof(GetPunchItemQueryHandler)}.{nameof(Handle)}")
                .SingleOrDefaultAsync(cancellationToken);

        if (dto is null)
        {
            return new NotFoundResult<PunchItemDetailsDto>(Strings.EntityNotFound(nameof(PunchItem), request.PunchItemGuid));
        }

        var createdBy = new PersonDto(
            dto.CreatedByUser.Guid,
            dto.CreatedByUser.FirstName,
            dto.CreatedByUser.LastName,
            dto.CreatedByUser.UserName,
            dto.CreatedByUser.Email);
        
        PersonDto? modifiedBy = null;
        if (dto.ModifiedByUser is not null)
        {
            modifiedBy = new PersonDto(
                dto.ModifiedByUser.Guid,
                dto.ModifiedByUser.FirstName,
                dto.ModifiedByUser.LastName,
                dto.ModifiedByUser.UserName,
                dto.ModifiedByUser.Email);
        }

        var punchItemDetailsDto = new PunchItemDetailsDto(
                       dto.PunchItem.Guid,
                       dto.Project.Name,
                       dto.PunchItem.ItemNo,
                       dto.PunchItem.Description,
                       createdBy,
                       dto.PunchItem.CreatedAtUtc,
                       modifiedBy,
                       dto.PunchItem.ModifiedAtUtc,
                       dto.PunchItem.RowVersion.ConvertToString());
        return new SuccessResult<PunchItemDetailsDto>(punchItemDetailsDto);
    }
}
