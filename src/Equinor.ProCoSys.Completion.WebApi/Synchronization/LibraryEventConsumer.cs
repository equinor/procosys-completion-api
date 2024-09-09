using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class LibraryEventConsumer(
    ILogger<LibraryEventConsumer> logger,
    ILibraryItemRepository libraryRepository,
    IUnitOfWork unitOfWork)
    : IConsumer<LibraryEvent>
{
    
    public async Task Consume(ConsumeContext<LibraryEvent> context)
    {
        var busEvent = context.Message;
        ValidateMessage(busEvent);

        if (busEvent.Behavior == "delete")
        {
            if (!await libraryRepository.RemoveByGuidAsync(busEvent.ProCoSysGuid, context.CancellationToken))
            {
                logger.LogWarning("Library with Guid {Guid} was not found and could not be deleted",
                    busEvent.ProCoSysGuid);
            }
        }
        else
        {
            // Test if message library type is not present in LibraryType enum
            if (!Enum.IsDefined(typeof(LibraryType), busEvent.Type))
            {
                logger.LogInformation("{EventName} not in scope of import: {LibraryType}",
                    nameof(LibraryEvent), busEvent.Type);
                return;
            }

            if (await libraryRepository.ExistsAsync(busEvent.ProCoSysGuid, context.CancellationToken))
            {
                var library = await libraryRepository.GetAsync(busEvent.ProCoSysGuid, context.CancellationToken);

                if (library.ProCoSys4LastUpdated == busEvent.LastUpdated)
                {
                    logger.LogDebug("{EventName} Ignored because LastUpdated is the same as in db\n" +
                                    "MessageId: {MessageId} \n ProCoSysGuid: {ProCoSysGuid} \n " +
                                    "EventLastUpdated: {LastUpdated} \n" +
                                    "SyncedToCompletion: {SyncedTimeStamp} \n",
                        nameof(LibraryEvent), context.MessageId, busEvent.ProCoSysGuid, busEvent.LastUpdated,
                        library.SyncTimestamp);
                    return;
                }

                if (library.ProCoSys4LastUpdated > busEvent.LastUpdated)
                {
                    logger.LogWarning("{EventName} Ignored because a newer LastUpdated already exits in db\n" +
                                      "MessageId: {MessageId} \n ProCoSysGuid: {ProCoSysGuid} \n " +
                                      "EventLastUpdated: {EventLastUpdated} \n" +
                                      "LastUpdatedFromDb: {LastUpdated}",
                        nameof(LibraryEvent), context.MessageId, busEvent.ProCoSysGuid, busEvent.LastUpdated,
                        library.ProCoSys4LastUpdated);
                    return;
                }

                MapFromEventToLibrary(busEvent, library);
                library.SyncTimestamp = DateTime.UtcNow;
            }
            else
            {
                var lib = CreateLibraryEntity(busEvent);
                lib.SyncTimestamp = DateTime.UtcNow;
                libraryRepository.Add(lib);
            }
        }

        await unitOfWork.SaveChangesFromSyncAsync(context.CancellationToken);

        logger.LogDebug("{EventName} Message Consumed: {MessageId} \n Guid {Guid} \n Code {LibraryCode}",
            nameof(LibraryEvent), context.MessageId, busEvent.ProCoSysGuid, busEvent.Type);
    }

    private static void ValidateMessage(LibraryEvent busEvent)
    {
        if (busEvent.ProCoSysGuid == Guid.Empty)
        {
            throw new Exception($"{nameof(LibraryEvent)} is missing {nameof(LibraryEvent.ProCoSysGuid)}");
        }

        if (string.IsNullOrEmpty(busEvent.Plant))
        {
            throw new Exception($"{nameof(LibraryEvent)} is missing {nameof(LibraryEvent.Plant)}");
        }

        if (string.IsNullOrEmpty(busEvent.Type) && busEvent.Behavior != "delete")
        {
            throw new Exception($"{nameof(LibraryEvent)} is missing {nameof(LibraryEvent.Type)}");
        }
    }

    private static LibraryItem CreateLibraryEntity(LibraryEvent libraryEvent)
    {
        var library = new LibraryItem(libraryEvent.Plant,
            libraryEvent.ProCoSysGuid,
            libraryEvent.Code,
            libraryEvent.Description!,
            Enum.Parse<LibraryType>(libraryEvent.Type,true)
            )
        {
            IsVoided = libraryEvent.IsVoided, 
            ProCoSys4LastUpdated = libraryEvent.LastUpdated
        };
        return library;
    }
  
    private static void MapFromEventToLibrary(LibraryEvent libraryEvent, LibraryItem library)
    {
        library.IsVoided = libraryEvent.IsVoided;
        library.Description = libraryEvent.Description!;
        library.Code = libraryEvent.Code;
        library.Type = Enum.Parse<LibraryType>(libraryEvent.Type,true);
        library.ProCoSys4LastUpdated = libraryEvent.LastUpdated;
    }
}

public record LibraryEvent(
    string Plant,
    Guid ProCoSysGuid,
    string Code,
    string? Description,
    bool IsVoided,
    string Type,
    DateTime LastUpdated,
    string? Behavior
); // all these fields adhere to LibraryEventV1

