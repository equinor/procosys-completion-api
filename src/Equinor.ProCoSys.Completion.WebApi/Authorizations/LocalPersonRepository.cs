using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Auth.Person;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

// Used in authorization of each request.
public class LocalPersonRepository : ILocalPersonRepository
{
    private readonly IReadOnlyContext _context;

    public LocalPersonRepository(IReadOnlyContext context) => _context = context;

    // Should be safe to use AnyAsync since we query for an userOid and there is unique constraint on this Guid
    public async Task<bool> ExistsAsync(Guid userOid, CancellationToken cancellationToken)
        => await (from person in _context.QuerySet<Person>()
            where person.Guid == userOid
            select 1).AnyAsync(cancellationToken: cancellationToken);

    // Should be safe to use AnyAsync since we query for an userOid and there is unique constraint on this Guid
    public async Task<ProCoSysPerson?> GetAsync(Guid userOid, CancellationToken cancellationToken)
        => await (from person in _context.QuerySet<Person>()
            where person.Guid == userOid
            select new ProCoSysPerson
            {
                AzureOid = person.Guid.ToString(),
                Email = person.Email,
                FirstName = person.FirstName,
                LastName = person.LastName,
                Super = person.Superuser
            }).SingleOrDefaultAsync(cancellationToken: cancellationToken);
}
