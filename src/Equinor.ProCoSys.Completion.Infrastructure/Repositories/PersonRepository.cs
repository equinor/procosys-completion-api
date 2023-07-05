using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class PersonRepository : EntityWithGuidRepository<Person>, IPersonRepository
{
    public PersonRepository(CompletionContext context)
        : base(context, context.Persons) { }
}
