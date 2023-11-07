using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Microsoft.EntityFrameworkCore;
using Equinor.ProCoSys.Common;
using System;
using Equinor.ProCoSys.Completion.Domain.Validators;

namespace Equinor.ProCoSys.Completion.Command.Validators;

public class PunchItemValidator : IPunchItemValidator
{
    private readonly IReadOnlyContext _context;
    private readonly ICheckListValidator _checkListValidator;

    // Trick to write LINQ queries to let EF create effective SQL queries is
    // 1) use Any
    // 2) select a projection with as few columns as needed
    public PunchItemValidator(IReadOnlyContext context, ICheckListValidator checkListValidator)
    {
        _context = context;
        _checkListValidator = checkListValidator;
    }

    public async Task<bool> ExistsAsync(Guid punchItemGuid, CancellationToken cancellationToken) =>
        await (from pi in _context.QuerySet<PunchItem>()
            where pi.Guid == punchItemGuid
            select 1).AnyAsync(cancellationToken);

    public async Task<bool> TagOwningPunchItemIsVoidedAsync(Guid punchItemGuid, CancellationToken cancellationToken)
    {
        var dto = await (from pi in _context.QuerySet<PunchItem>()
            where pi.Guid == punchItemGuid
            select new
            {
                pi.CheckListGuid
            }).SingleOrDefaultAsync(cancellationToken);

        if (dto is null)
        {
            return false;
        }

        return await _checkListValidator.TagOwningCheckListIsVoidedAsync(dto.CheckListGuid);
    }

    public async Task<bool> ProjectOwningPunchItemIsClosedAsync(Guid punchItemGuid, CancellationToken cancellationToken)
        => await (from pi in _context.QuerySet<PunchItem>()
            join proj in _context.QuerySet<Project>() on pi.ProjectId equals proj.Id
            where pi.Guid == punchItemGuid && proj.IsClosed == true
            select proj).AnyAsync(cancellationToken);

    public async Task<bool> IsClearedAsync(Guid punchItemGuid, CancellationToken cancellationToken)
        => await (from pi in _context.QuerySet<PunchItem>()
            where pi.Guid == punchItemGuid && pi.ClearedAtUtc != null
            select pi).AnyAsync(cancellationToken);

    public async Task<bool> IsVerifiedAsync(Guid punchItemGuid, CancellationToken cancellationToken)
        => await (from pi in _context.QuerySet<PunchItem>()
            where pi.Guid == punchItemGuid && pi.VerifiedAtUtc != null
            select pi).AnyAsync(cancellationToken);

    public async Task<bool> HasCategoryAsync(Guid punchItemGuid, Category category, CancellationToken cancellationToken)
        => await (from pi in _context.QuerySet<PunchItem>()
            where pi.Guid == punchItemGuid && pi.Category == category
            select pi).AnyAsync(cancellationToken);
}
