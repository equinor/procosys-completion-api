using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
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
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserSetter _currentUserSetter;
    private readonly IOptionsMonitor<ApplicationOptions> _applicationOptions;

    public DocumentEventConsumer(ILogger<DocumentEventConsumer> logger,
        IDocumentRepository documentRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserSetter currentUserSetter,
        IOptionsMonitor<ApplicationOptions> applicationOptions)
    {
        _logger = logger;
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
        _currentUserSetter = currentUserSetter;
        _applicationOptions = applicationOptions;
    }

    public async Task Consume(ConsumeContext<DocumentEvent> context)
    {
        var busEvent = context.Message;

        if (busEvent.ProCoSysGuid == Guid.Empty)
        {
            throw new Exception("Message is missing ProCoSysGuid");
        }


        if (await _documentRepository.ExistsAsync(busEvent.ProCoSysGuid, context.CancellationToken))
        {
            // TODO Implement mapping ?
            var document = await _documentRepository.GetAsync(busEvent.ProCoSysGuid, context.CancellationToken);
           // MapFromEventToDocument(busEvent, document);
        }
        else
        {
            var document = CreateDocumentEntity(busEvent);
            _documentRepository.Add(document);
        }

        _currentUserSetter.SetCurrentUserOid(_applicationOptions.CurrentValue.ObjectId);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Document Message Consumed: {MessageId} \n Guid {Guid} \n {No}",
            context.MessageId, busEvent.ProCoSysGuid, busEvent.DocumentNo);
    }

    private static Document CreateDocumentEntity(IDocumentEventV1 documentEvent)
    {
        var document = new Document(
            documentEvent.Plant,
            documentEvent.ProCoSysGuid,
            documentEvent.DocumentNo
        );
        return document;
    }

    
    private static void MapFromEventToDocument(IDocumentEventV1 documentEvent, Document document)
    { 
        // Todo Is there anything to update?
        // document.IsVoided = documentEvent.;
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
