using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.PcsServiceBus.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class LibraryEventConsumer : IConsumer<LibraryEvent>
{
    private readonly ILogger<LibraryEventConsumer> _logger;
    private readonly IPlantSetter _plantSetter;
    private readonly ILibraryItemRepository _libraryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserSetter _currentUserSetter;
    private readonly IOptionsMonitor<ApplicationOptions> _applicationOptions;

    public LibraryEventConsumer(ILogger<LibraryEventConsumer> logger,
        IPlantSetter plantSetter,
        ILibraryItemRepository libraryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserSetter currentUserSetter,
        IOptionsMonitor<ApplicationOptions> applicationOptions)
    {
        _logger = logger;
        _plantSetter = plantSetter;
        _libraryRepository = libraryRepository;
        _unitOfWork = unitOfWork;
        _currentUserSetter = currentUserSetter;
        _applicationOptions = applicationOptions;
    }

    public async Task Consume(ConsumeContext<LibraryEvent> context)
    {
        var busEvent = context.Message;

        ValidateMessage(busEvent);

        // Test if message library type is not present in LibraryType enum
        if (!Enum.IsDefined(typeof(LibraryType), busEvent.Type) && !busEvent.Type.ToUpper().Equals("COMM_PRIORITY"))
        {
            _logger.LogInformation("LibraryType not in scope of import: {libraryType}", busEvent.Type );
            return;
        }

        _plantSetter.SetPlant(busEvent.Plant);

        if (await _libraryRepository.ExistsAsync(busEvent.ProCoSysGuid, context.CancellationToken))
        {
            var library = await _libraryRepository.GetAsync(busEvent.ProCoSysGuid, context.CancellationToken);
            _logger.LogInformation("LibraryItem exists, updating {id}, {guid}, {type}", library.Id, library.Guid.ToString(), library.Type.ToString());
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

    private static void ValidateMessage(LibraryEvent busEvent)
    {
        if (busEvent.ProCoSysGuid == Guid.Empty)
        {
            throw new Exception("Message is missing ProCoSysGuid");
        }

        if (string.IsNullOrEmpty(busEvent.Plant))
        {
            throw new Exception("Message is missing Plant");
        }

        if (string.IsNullOrEmpty(busEvent.Type))
        {
            throw new Exception("Message is missing Type");
        }

        if (string.IsNullOrEmpty(busEvent.Description))
        {
            throw new Exception("Message is missing Description");
        }
    }

    private static LibraryItem CreateLibraryEntity(ILibraryEventV1 libraryEvent)
    {
        var library = new LibraryItem(libraryEvent.Plant,
            libraryEvent.ProCoSysGuid,
            libraryEvent.Code,
            libraryEvent.Description!,
            !Enum.TryParse(libraryEvent.Type, true, out LibraryType libType)
                && libraryEvent.Type.Equals("COMM_PRIORITY", StringComparison.CurrentCultureIgnoreCase)
                ? LibraryType.PUNCHLIST_PRIORITY : libType
            );
        return library;
    }
  
    private static void MapFromEventToLibrary(ILibraryEventV1 libraryEvent, LibraryItem library)
    {
        library.IsVoided = libraryEvent.IsVoided;
        library.Description = libraryEvent.Description!;
        library.Code = libraryEvent.Code;
        library.Type = !Enum.TryParse(libraryEvent.Type, true, out LibraryType libType) 
                       && libraryEvent.Type.Equals("COMM_PRIORITY", StringComparison.CurrentCultureIgnoreCase) 
                       ? LibraryType.PUNCHLIST_PRIORITY : libType;
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

