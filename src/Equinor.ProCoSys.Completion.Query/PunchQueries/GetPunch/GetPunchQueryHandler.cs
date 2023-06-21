using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunch;

public class GetPunchQueryHandler : IRequestHandler<GetPunchQuery, Result<PunchDetailsDto>>
{
    private readonly IReadOnlyContext _context;

    public GetPunchQueryHandler(IReadOnlyContext context) => _context = context;

    public async Task<Result<PunchDetailsDto>> Handle(GetPunchQuery request, CancellationToken cancellationToken)
    {
        var dto =
            await (from punch in _context.QuerySet<Punch>()
                   join project in _context.QuerySet<Project>()
                       on punch.ProjectId equals project.Id
                   join createdByUser in _context.QuerySet<Person>()
                       on punch.CreatedById equals createdByUser.Id
                   from modifiedByUser in _context.QuerySet<Person>()
                       .Where(p => p.Id == punch.ModifiedById).DefaultIfEmpty() //left join!
                   where punch.Guid == request.PunchGuid
                   select new {
                       Punch = punch,
                       Project = project,
                       CreatedByUser = createdByUser, 
                       ModifiedByUser = modifiedByUser
                })
                .TagWith($"{nameof(GetPunchQueryHandler)}.{nameof(Handle)}")
                .SingleOrDefaultAsync(cancellationToken);

        if (dto == null)
        {
            return new NotFoundResult<PunchDetailsDto>(Strings.EntityNotFound(nameof(Punch), request.PunchGuid));
        }

        var createdBy = new PersonDto(
            dto.CreatedByUser.Guid,
            dto.CreatedByUser.FirstName,
            dto.CreatedByUser.LastName,
            dto.CreatedByUser.UserName,
            dto.CreatedByUser.Email);
        
        PersonDto? modifiedBy = null;
        if (dto.ModifiedByUser != null)
        {
            modifiedBy = new PersonDto(
                dto.ModifiedByUser.Guid,
                dto.ModifiedByUser.FirstName,
                dto.ModifiedByUser.LastName,
                dto.ModifiedByUser.UserName,
                dto.ModifiedByUser.Email);
        }

        var punchDetailsDto = new PunchDetailsDto(
                       dto.Punch.Guid,
                       dto.Project.Name,
                       dto.Punch.ItemNo,
                       dto.Punch.Description,
                       createdBy,
                       dto.Punch.CreatedAtUtc,
                       modifiedBy,
                       dto.Punch.ModifiedAtUtc,
                       dto.Punch.IsVoided,
                       dto.Punch.RowVersion.ConvertToString());
        return new SuccessResult<PunchDetailsDto>(punchDetailsDto);
    }
}
