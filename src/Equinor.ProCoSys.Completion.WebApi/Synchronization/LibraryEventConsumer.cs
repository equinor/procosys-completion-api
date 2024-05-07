using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.PcsServiceBus.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class LibraryEventConsumer : IConsumer<LibraryEvent>
{
    private readonly ILogger<LibraryEventConsumer> _logger;
    private readonly ILibraryItemRepository _libraryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserSetter _currentUserSetter;
    private readonly IOptionsMonitor<ApplicationOptions> _applicationOptions;

    public LibraryEventConsumer(ILogger<LibraryEventConsumer> logger,
        ILibraryItemRepository libraryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserSetter currentUserSetter,
        IOptionsMonitor<ApplicationOptions> applicationOptions)
    {
        _logger = logger;
        _libraryRepository = libraryRepository;
        _unitOfWork = unitOfWork;
        _currentUserSetter = currentUserSetter;
        _applicationOptions = applicationOptions;
    }

    public async Task Consume(ConsumeContext<LibraryEvent> context)
    {
        var busEvent = context.Message;

        if (busEvent.ProCoSysGuid == Guid.Empty)
        {
            throw new Exception("Message is missing ProCoSysGuid");
        }

        
        if (Enum.IsDefined(typeof(LibraryType), busEvent.Type) &&
            await _libraryRepository.ExistsAsync(busEvent.ProCoSysGuid, context.CancellationToken))
        {
            var library = await _libraryRepository.GetAsync(busEvent.ProCoSysGuid, context.CancellationToken);
            MapFromEventToLibrary(busEvent, library);
        }
        else
        {
            var lib = CreateLibraryEntity(busEvent);
            _libraryRepository.Add(lib);
        }

        _currentUserSetter.SetCurrentUserOid(_applicationOptions.CurrentValue.ObjectId);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("LibraryItem Message Consumed: {MessageId} \n Guid {Guid} \n {LibraryCode}",
            context.MessageId, busEvent.ProCoSysGuid, busEvent.Type);
    }

    private static LibraryItem CreateLibraryEntity(ILibraryEventV1 libraryEvent)
    {
        var library = new LibraryItem(libraryEvent.Plant,
            libraryEvent.ProCoSysGuid,
            libraryEvent.Code,
            libraryEvent.Description!,
            (LibraryType)Enum.Parse(typeof(LibraryType), libraryEvent.Type));
        return library;
    }
  
    private static void MapFromEventToLibrary(ILibraryEventV1 libraryEvent, LibraryItem library)
    {
        library.IsVoided = libraryEvent.IsVoided;
    }

}


public record LibraryEvent(
    string EventType,
    string Plant,
    Guid ProCoSysGuid,
    long LibraryId,
    int? ParentId,
    Guid? ParentGuid,
    string Code,
    string? Description,
    bool IsVoided,
    string Type,
    DateTime LastUpdated
) : ILibraryEventV1;

