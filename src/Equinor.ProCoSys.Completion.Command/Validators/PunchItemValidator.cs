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

    public PunchItemValidator(IReadOnlyContext context, ICheckListValidator checkListValidator)
    {
        _context = context;
        _checkListValidator = checkListValidator;
    }

    public async Task<bool> ExistsAsync(Guid punchItemGuid, CancellationToken cancellationToken)
    {
        var punchItem = await GetPunchItemAsync(punchItemGuid, cancellationToken);

        return punchItem is not null;
    }

    public async Task<bool> TagOwningPunchItemIsVoidedAsync(Guid punchItemGuid, CancellationToken cancellationToken)
    {
        var punchItem = await GetPunchItemAsync(punchItemGuid, cancellationToken);

        if (punchItem is null)
        {
            return false;
        }

        return await _checkListValidator.TagOwningCheckListIsVoidedAsync(punchItem.CheckListGuid);
    }

    public async Task<bool> ProjectOwningPunchItemIsClosedAsync(Guid punchItemGuid, CancellationToken cancellationToken)
    {
        var project = await (from pi in _context.QuerySet<PunchItem>()
                             join proj in _context.QuerySet<Project>() on pi.ProjectId equals proj.Id
                             where pi.Guid == punchItemGuid
                             select proj).SingleOrDefaultAsync(cancellationToken);

        return project is not null && project.IsClosed;
    }

    public async Task<bool> IsClearedAsync(Guid punchItemGuid, CancellationToken cancellationToken)
    {
        var punchItem = await GetPunchItemAsync(punchItemGuid, cancellationToken);

        return punchItem?.ClearedAtUtc is not null;
    }

    public async Task<bool> IsVerifiedAsync(Guid punchItemGuid, CancellationToken cancellationToken)
    {
        var punchItem = await GetPunchItemAsync(punchItemGuid, cancellationToken);

        return punchItem?.VerifiedAtUtc is not null;
    }

    public async Task<bool> HasCategoryAsync(Guid punchItemGuid, Category category, CancellationToken cancellationToken)
    {
        var punchItem = await GetPunchItemAsync(punchItemGuid, cancellationToken);

        return punchItem is not null && punchItem.Category == category;
    }

    private async Task<PunchItem?> GetPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken)
        => await (from pi in _context.QuerySet<PunchItem>()
            where pi.Guid == punchItemGuid
            select pi).SingleOrDefaultAsync(cancellationToken);
}
