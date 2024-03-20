using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.MessageContracts;

namespace Equinor.ProCoSys.Completion.Command.ModifiedEvents;

public readonly record struct ModifiedEvent(DateTime ModifiedAtUtc, User User);

public sealed class ModifiedEventService(IPersonRepository personRepository) : IModifiedEventService
{
    public async Task<ModifiedEvent> GetModifiedEventAsync(CancellationToken cancellationToken)
    {
        var person = await personRepository.GetCurrentPersonAsync(cancellationToken);
        return new ModifiedEvent(TimeService.UtcNow, new User(person.Guid, person.GetFullName()));
    }
}
