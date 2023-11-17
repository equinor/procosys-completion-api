using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Command.Validators;

public class PersonValidator : IPersonValidator
{
    private readonly IReadOnlyContext _context;
    private readonly IPersonCache _personCache;

    public PersonValidator(IReadOnlyContext context, IPersonCache personCache)
    {
        _context = context;
        _personCache = personCache;
    }

    public async Task<bool> ExistsLocalOrInProCoSysAsync(Guid oid, CancellationToken cancellationToken)
    {
        var exists = await (from p in _context.QuerySet<Person>()
            where p.Guid == oid
            select 1).AnyAsync(cancellationToken);
        if (exists)
        {
            return true;
        }

        var pcsPerson = await _personCache.GetAsync(oid);
        return pcsPerson is not null && !pcsPerson.ServicePrincipal;
    }
}
