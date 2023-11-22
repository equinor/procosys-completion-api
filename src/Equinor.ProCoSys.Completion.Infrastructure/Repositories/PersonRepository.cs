using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class PersonRepository : EntityWithGuidRepository<Person>, IPersonRepository
{
    private readonly ICurrentUserProvider _currentUserProvider;

    public PersonRepository(CompletionContext context, ICurrentUserProvider currentUserProvider)
        : base(context, context.Persons) =>
        _currentUserProvider = currentUserProvider;

    public async Task<Person> GetCurrentPersonAsync(CancellationToken cancellationToken)
    {
        var currentUserOid = _currentUserProvider.GetCurrentUserOid();
        
        return await GetAsync(currentUserOid, cancellationToken);
    }
}
