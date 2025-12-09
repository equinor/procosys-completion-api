using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;

namespace Equinor.ProCoSys.Completion.TieImport.References;

public class CommandReferencesServiceFactory(
    IWorkOrderRepository workOrderRepository,
    IDocumentRepository documentRepository,
    ISWCRRepository swcrRepository,
    IPersonRepository personRepository) : ICommandReferencesServiceFactory
{
    public ICommandReferencesService Create(ImportDataBundle bundle) =>
        new CommandReferencesService(
            bundle,
            workOrderRepository,
            documentRepository,
            swcrRepository,
            personRepository);
}
