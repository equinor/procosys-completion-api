using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class LibraryEventConsumer(
    ILogger<LibraryEventConsumer> logger,
    IPlantSetter plantSetter,
    ILibraryItemRepository libraryRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserSetter currentUserSetter,
    IOptionsMonitor<ApplicationOptions> applicationOptions)
    : IConsumer<LibraryEvent>
{
    
    private const string CommPriority = "COMM_PRIORITY";
    public async Task Consume(ConsumeContext<LibraryEvent> context)
    {
        var busEvent = context.Message;

        ValidateMessage(busEvent);

        // Test if message library type is not present in LibraryType enum
        if (!Enum.IsDefined(typeof(LibraryType), busEvent.Type) && !busEvent.Type.ToUpper().Equals(CommPriority))
        {
            logger.LogInformation($"{nameof(LibraryEvent)} not in scope of import: {{libraryType}}", busEvent.Type );
            return;
        }

        plantSetter.SetPlant(busEvent.Plant);

        if (await libraryRepository.ExistsAsync(busEvent.ProCoSysGuid, context.CancellationToken))
        {
            var library = await libraryRepository.GetAsync(busEvent.ProCoSysGuid, context.CancellationToken);
            
            if (library.ProCoSys4LastUpdated == busEvent.LastUpdated)
            {
                logger.LogInformation("Library Message Ignored because LastUpdated is the same as in db\n" +
                                      "MessageId: {MessageId} \n ProCoSysGuid {ProCoSysGuid} \n " +
                                      "EventLastUpdated: {LastUpdated} \n" +
                                      "SyncedToCompletion: {SyncedTimeStamp} \n",
                    context.MessageId, busEvent.ProCoSysGuid, busEvent.LastUpdated, library.SyncedTimeStamp );
                return;
            }

            if (library.ProCoSys4LastUpdated > busEvent.LastUpdated)
            {
                logger.LogWarning("Library Message Ignored because a newer LastUpdated already exits in db\n" +
                                  "MessageId: {MessageId} \n ProCoSysGuid {ProCoSysGuid} \n " +
                                  "EventLastUpdated: {EventLastUpdated} \n" +
                                  "LastUpdatedFromDb: {LastUpdated}",
                    context.MessageId, busEvent.ProCoSysGuid, busEvent.LastUpdated, library.ProCoSys4LastUpdated);
                return;
            }
            
            MapFromEventToLibrary(busEvent, library);
            library.SyncedTimeStamp = DateTime.UtcNow;
        }
        else
        {
            var lib = CreateLibraryEntity(busEvent);
            lib.SyncedTimeStamp = DateTime.UtcNow;
            libraryRepository.Add(lib);
        }

        currentUserSetter.SetCurrentUserOid(applicationOptions.CurrentValue.ObjectId);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation($"{nameof(LibraryEvent)} Message Consumed: {{MessageId}} \n Guid {{Guid}} \n Code {{LibraryCode}}",
            context.MessageId, busEvent.ProCoSysGuid, busEvent.Type);
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

        if (string.IsNullOrEmpty(busEvent.Type))
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
            !Enum.TryParse(libraryEvent.Type, true, out LibraryType libType)
                && libraryEvent.Type.Equals(CommPriority, StringComparison.CurrentCultureIgnoreCase)
                ? LibraryType.PUNCHLIST_PRIORITY : libType
            ) { IsVoided = libraryEvent.IsVoided, ProCoSys4LastUpdated = libraryEvent.LastUpdated };
        return library;
    }
  
    private static void MapFromEventToLibrary(LibraryEvent libraryEvent, LibraryItem library)
    {
        library.IsVoided = libraryEvent.IsVoided;
        library.Description = libraryEvent.Description!;
        library.Code = libraryEvent.Code;
        library.Type = !Enum.TryParse(libraryEvent.Type, true, out LibraryType libType) 
                       && libraryEvent.Type.Equals(CommPriority, StringComparison.CurrentCultureIgnoreCase) 
                       ? LibraryType.PUNCHLIST_PRIORITY : libType;
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
    DateTime LastUpdated
); // all these fields adhere to LibraryEventV1

