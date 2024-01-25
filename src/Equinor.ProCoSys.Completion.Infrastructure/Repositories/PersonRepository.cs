using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class PersonRepository : EntityWithGuidRepository<Person>, IPersonRepository
{
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IPersonCache _personCache;

    public PersonRepository(
        CompletionContext context,
        ICurrentUserProvider currentUserProvider,
        IPersonCache personCache)
        : base(context, context.Persons)
    {
        _currentUserProvider = currentUserProvider;
        _personCache = personCache;
    }

    public async Task<Person> GetCurrentPersonAsync(CancellationToken cancellationToken)
    {
        var currentUserOid = _currentUserProvider.GetCurrentUserOid();
        
        return await GetAsync(currentUserOid, cancellationToken);
    }

    public async Task<List<Person>> GetOrCreateManyAsync(IEnumerable<Guid> oids, CancellationToken cancellationToken)
    {
        var persons = new List<Person>();
        // This CAN be optimized since this implementation use _personCache.GetAsync to get persons, one by one
        // This will cause requesting ProCoSys Main API for persons multiple times, one by one
        // An optimized solution can be to implement a _personCache.GetManyAsync for persons not exists locally
        foreach (var oid in oids)
        {
            var person = await GetOrCreateAsync(oid, cancellationToken);
            persons.Add(person);
        }

        return persons;
    }

    public async Task<Person> GetOrCreateAsync(Guid oid, CancellationToken cancellationToken)
    {
        var personExists = await ExistsAsync(oid, cancellationToken);
        if (personExists)
        {
            return await GetAsync(oid, cancellationToken);
        }

        var pcsPerson = await _personCache.GetAsync(oid);
        var person = new Person(
            oid,
            pcsPerson.FirstName,
            pcsPerson.LastName,
            pcsPerson.UserName,
            pcsPerson.Email,
            pcsPerson.Super);
        Add(person);

        return person;
    }
}
