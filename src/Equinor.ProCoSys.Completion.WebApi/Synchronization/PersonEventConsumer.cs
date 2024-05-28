using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.PcsServiceBus.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class PersonEventConsumer(
    ILogger<PersonEventConsumer> logger,
    IPersonRepository personRepository,
    IUnitOfWork unitOfWork)
    : IConsumer<PersonEvent>
{
    public async Task Consume(ConsumeContext<PersonEvent> context)
    {
        var personEvent = context.Message;

        if (personEvent.Guid == Guid.Empty)
        {
            throw new Exception("Message is missing user Azure oid");
        }

        if (personEvent.Behavior == "delete")
        {
            logger.LogInformation("Delete behavior for person is currently not implemented.");
            return;
        }

        if (!await personRepository.ExistsAsync(personEvent.Guid, context.CancellationToken))
        {
            logger.LogInformation("Person does not exists. Message Skipped: {MessageId} \n {UserName} \n {Guid}",
                context.MessageId, personEvent.UserName, personEvent.Guid);
            return;
        }

        var person = await personRepository.GetAsync(personEvent.Guid, context.CancellationToken);

        if (person.ProCoSys4LastUpdated == personEvent.LastUpdated)
        {
            logger.LogInformation("Person Message Ignored because LastUpdated is the same as in db\n" +
                                  "MessageId: {MessageId} \n ProCoSysGuid {ProCoSysGuid} \n " +
                                  "EventLastUpdated: {LastUpdated} \n" +
                                  "SyncedToCompletion: {CreatedAtUtc} \n",
                context.MessageId, personEvent.Guid, personEvent.LastUpdated, person.SyncTimestamp );
            return;
        }

        if (person.ProCoSys4LastUpdated > personEvent.LastUpdated)
        {
            logger.LogWarning("Person Message Ignored because a newer LastUpdated already exits in db\n" +
                              "MessageId: {MessageId} \n ProCoSysGuid {ProCoSysGuid} \n " +
                              "EventLastUpdated: {LastUpdated} \n" +
                              "LastUpdatedFromDb: {ProjectLastUpdated}",
                context.MessageId, personEvent.Guid, personEvent.LastUpdated, person.ProCoSys4LastUpdated);
            return;
        }
        
        MapFromEventToPerson(personEvent, person);
        person.SyncTimestamp = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("Person Message Consumed: {MessageId} \n Guid {Guid} \n {UserName}",
            context.MessageId, personEvent.Guid, personEvent.UserName);
    }

    private static void MapFromEventToPerson(IPersonEventV1 personEvent, Person person)
    {
        person.FirstName = personEvent.FirstName;
        person.LastName = personEvent.LastName;
        person.Email = personEvent.Email;
        person.UserName = personEvent.UserName;
        person.Superuser = personEvent.SuperUser;
        person.ProCoSys4LastUpdated = personEvent.LastUpdated;
    }
}

public record PersonEvent(string EventType, 
        Guid Guid, 
        string FirstName,
        string LastName,
        string UserName,
        string Email,
        bool SuperUser,
        DateTime LastUpdated,
        string? Behavior)
    : IPersonEventV1;
    
