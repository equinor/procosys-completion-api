using System.Threading.Tasks;
using MassTransit;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization.Services;

public interface IDocumentConsumerService
{
    Task ConsumeDocumentEvent(ConsumeContext context, DocumentEvent busEvent);
}
