using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

public class LocalPersonRepository : ILocalPersonRepository
{
    private readonly IReadOnlyContext _context;

    public LocalPersonRepository(IReadOnlyContext context) => _context = context;

    // Used in authorization of each request.
    // Should be safe to use AnyAsync since we query for an userOid and 108105 will add unique constraint on this Guid
    public async Task<bool> ExistsAsync(Guid userOid)
        => await (from person in _context.QuerySet<Person>()
            where person.Guid == userOid
            select 1).AnyAsync();
}
