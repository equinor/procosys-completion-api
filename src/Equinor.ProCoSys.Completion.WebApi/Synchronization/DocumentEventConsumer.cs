using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.PcsServiceBus.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class DocumentEventConsumer : IConsumer<DocumentEvent>
{
    private readonly ILogger<DocumentEventConsumer> _logger;
    private readonly IPlantSetter _plantSetter;
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserSetter _currentUserSetter;
    private readonly IOptionsMonitor<ApplicationOptions> _applicationOptions;

    public DocumentEventConsumer(ILogger<DocumentEventConsumer> logger,
        IPlantSetter plantSetter,
        IDocumentRepository documentRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserSetter currentUserSetter,
        IOptionsMonitor<ApplicationOptions> applicationOptions)
    {
        _logger = logger;
        _plantSetter = plantSetter;
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
        _currentUserSetter = currentUserSetter;
        _applicationOptions = applicationOptions;
    }

    public async Task Consume(ConsumeContext<DocumentEvent> context)
    {
        var busEvent = context.Message;

        ValidateMessage(busEvent);
        _plantSetter.SetPlant(busEvent.Plant);

        if (await _documentRepository.ExistsAsync(busEvent.ProCoSysGuid, context.CancellationToken))
        {
            var document = await _documentRepository.GetAsync(busEvent.ProCoSysGuid, context.CancellationToken);
            MapFromEventToDocument(busEvent, document);
        }
        else
        {
            var document = CreateDocumentEntity(busEvent);
            _documentRepository.Add(document);
        }
        
        _currentUserSetter.SetCurrentUserOid(_applicationOptions.CurrentValue.ObjectId);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation($"{nameof(DocumentEvent)} Message Consumed: {{MessageId}} \n Guid {{Guid}} \n No {{No}}",
            context.MessageId, busEvent.ProCoSysGuid, busEvent.DocumentNo);
    }

    private static void ValidateMessage(DocumentEvent busEvent)
    {
        if (busEvent.ProCoSysGuid == Guid.Empty)
        {
            throw new Exception($"{nameof(DocumentEvent)} is missing {nameof(DocumentEvent.ProCoSysGuid)}");
        }

        if (string.IsNullOrEmpty(busEvent.Plant))
        {
            throw new Exception($"{nameof(DocumentEvent)} is missing {nameof(DocumentEvent.Plant)}");
        }
    }

    private static Document CreateDocumentEntity(IDocumentEventV1 busEvent)
    {
        var document = new Document(
            busEvent.Plant,
            busEvent.ProCoSysGuid,
            busEvent.DocumentNo
        );
        return document;
    }


    private static void MapFromEventToDocument(IDocumentEventV1 busEvent, Document document)
    {
        document.No = busEvent.DocumentNo;
        // TODO No input for setting isVoided?
        //document.IsVoided = busEvent.xxxxxx
    }
}


public record DocumentEvent
(
    string EventType, 
    string Plant, 
    Guid ProCoSysGuid,
    string ProjectName,
    long DocumentId,
    string DocumentNo,
    string? Title,
    string? AcceptanceCode,
    string? Archive,
    string? AccessCode,
    string? Complex,
    string? DocumentType,
    string? DisciplineId,
    string? DocumentCategory,
    string? HandoverStatus,
    string? RegisterType,
    string? RevisionNo,
    string? RevisionStatus,
    string? ResponsibleContractor,
    DateTime LastUpdated,
    DateOnly? RevisionDate
) : IDocumentEventV1;
