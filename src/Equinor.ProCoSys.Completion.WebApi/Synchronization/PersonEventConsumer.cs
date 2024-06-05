using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.PcsServiceBus.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class PersonEventConsumer(
    ILogger<PersonEventConsumer> logger,
    IPersonRepository personRepository,
    IUnitOfWork unitOfWork)
    : IConsumer<PersonEvent>
{
    private const string CreateFromPunchEventType = "CreateForPunch";

    public async Task Consume(ConsumeContext<PersonEvent> context)
    {
        var personEvent = context.Message;

        if (personEvent.AzureOid is null || personEvent.AzureOid == Guid.Empty)
        {
            logger.LogInformation("We currently ignore messages without Azure_Oid.");
            return;
        }

        if (personEvent.Behavior == "delete")
        {
            logger.LogInformation("Delete behavior for person is currently not implemented.");
            return;
        }

        if (!await personRepository.ExistsAsync(personEvent.AzureOid!.Value, context.CancellationToken))
        {
            if (CreateFromPunchEventType.Equals(personEvent.EventType, StringComparison.InvariantCultureIgnoreCase))
            {
                var person = new Person(personEvent.AzureOid.Value, 
                    personEvent.FirstName, 
                    personEvent.LastName,
                    personEvent.UserName, 
                    personEvent.Email, 
                    personEvent.SuperUser)
                {
                    ProCoSys4LastUpdated = personEvent.LastUpdated,
                    SyncTimestamp = DateTime.UtcNow
                };
                personRepository.Add(person);
                logger.LogInformation("Person does not exists. Add user: {MessageId} \n {UserName} \n {Guid}",
                    context.MessageId, personEvent.UserName, personEvent.AzureOid);
            }
            else
            {
                logger.LogInformation("Person does not exists. Message Skipped: {MessageId} \n {UserName} \n {Guid}",
                    context.MessageId, personEvent.UserName, personEvent.AzureOid);

                return;
            }
        }
        else
        {
            var person = await personRepository.GetAsync(personEvent.AzureOid.Value, context.CancellationToken);

            if (person.ProCoSys4LastUpdated == personEvent.LastUpdated)
            {
                logger.LogInformation("Person Message Ignored because LastUpdated is the same as in db\n" +
                                      "MessageId: {MessageId} \n ProCoSysGuid: {ProCoSysGuid} \n " +
                                      "EventLastUpdated: {LastUpdated} \n" +
                                      "SyncedToCompletion: {CreatedAtUtc} \n",
                    context.MessageId, personEvent.AzureOid, personEvent.LastUpdated, person.SyncTimestamp);
                return;
            }

            if (person.ProCoSys4LastUpdated > personEvent.LastUpdated)
            {
                logger.LogWarning("Person Message Ignored because a newer LastUpdated already exits in db\n" +
                                  "MessageId: {MessageId} \n ProCoSysGuid: {ProCoSysGuid} \n " +
                                  "EventLastUpdated: {LastUpdated} \n" +
                                  "LastUpdatedFromDb: {ProjectLastUpdated}",
                    context.MessageId, personEvent.AzureOid, personEvent.LastUpdated, person.ProCoSys4LastUpdated);
                return;
            }
            MapFromEventToPerson(personEvent, person);
            person.SyncTimestamp = DateTime.UtcNow;
        }

        await unitOfWork.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("Person Message Consumed: {MessageId} \n Guid {Guid} \n {UserName}",
            context.MessageId, personEvent.AzureOid, personEvent.UserName);
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
        Guid ProCoSysGuid,
        Guid? AzureOid, 
        string FirstName,
        string LastName,
        string UserName,
        string Email,
        bool SuperUser,
        DateTime LastUpdated,
        string? Behavior)
    : IPersonEventV1;
    
