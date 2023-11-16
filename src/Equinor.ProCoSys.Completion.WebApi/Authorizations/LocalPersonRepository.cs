using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

// Used in Equinor.ProCoSys.Auth.Authorization
public class LocalPersonRepository : ILocalPersonRepository
{
    private readonly IReadOnlyContext _context;

    // Trick to write LINQ queries to let EF create effective SQL queries is
    // 1) use Any
    // 2) select a projection with as few columns as needed
    public LocalPersonRepository(IReadOnlyContext context) => _context = context;

    public async Task<bool> ExistsAsync(Guid userOid)
        => await (from person in _context.QuerySet<Person>()
            where person.Guid == userOid
            select 1).AnyAsync();
}
