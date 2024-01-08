using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.WebApi.Authentication;
using Equinor.ProCoSys.PcsServiceBus.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class PersonEventConsumer : IConsumer<PersonEvent>
{
    private readonly ILogger<PersonEventConsumer> _logger; 
    private readonly IPersonRepository _personRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserSetter _currentUserSetter;
    private readonly IOptionsMonitor<CompletionAuthenticatorOptions> _options;
    
    public PersonEventConsumer(ILogger<PersonEventConsumer> logger, 
        IPersonRepository personRepository, 
        IUnitOfWork unitOfWork, 
        ICurrentUserSetter currentUserSetter, 
        IOptionsMonitor<CompletionAuthenticatorOptions> options)
    {
        _logger = logger;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _currentUserSetter = currentUserSetter;
        _options = options;
    }

    public async Task Consume(ConsumeContext<PersonEvent> context)
    {
        var personEvent = context.Message;

        if (personEvent.Guid == Guid.Empty)
        {
            throw new Exception("Message is missing user Azure oid");
        }

        if (personEvent.Behavior == "delete")
        {
            _logger.LogInformation("Delete behavior for person is currently not implemented.");
            return;
        }

        if (!await _personRepository.ExistsAsync(personEvent.Guid, context.CancellationToken))
        {
            _logger.LogInformation("Person does not exists. Message Skipped: {MessageId} \n {UserName} \n {Guid}",
                context.MessageId, personEvent.UserName, personEvent.Guid);
            return;
        }

        var person = await _personRepository.GetAsync(personEvent.Guid, context.CancellationToken);

        if (person.ProCoSys4LastUpdated > personEvent.LastUpdated)
        {
            _logger.LogWarning("Person Message Ignored because a newer LastUpdated already exits in db\n" +
                               "MessageId: {MessageId} \n Guid {Guid} \n " +
                               "EventLastUpdated: {LastUpdated} \n" +
                               "LastUpdatedFromDb: {PersonLastUpdated}",
                context.MessageId, personEvent.Guid, personEvent.LastUpdated, person.ProCoSys4LastUpdated);
            return;
        }

        MapFromEventToPerson(personEvent, person);

        _currentUserSetter.SetCurrentUserOid(_options.CurrentValue.CompletionApiObjectId);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Person Message Consumed: {MessageId} \n Guid {Guid} \n {UserName}",
            context.MessageId, personEvent.Guid, personEvent.UserName);
    }

    private static void MapFromEventToPerson(IPersonEventV1 personEvent, Person person)
    {
        person.FirstName = personEvent.FirstName;
        person.LastName = personEvent.LastName;
        person.Email = personEvent.Email;
        person.UserName = personEvent.UserName;
        person.Superuser = personEvent.SuperUser;
        person.SetProCoSys4LastUpdated(personEvent.LastUpdated);
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
    
