using System;
using Equinor.ProCoSys.Completion.DbSyncToPCS4;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands
{
    public class PunchItemCommandHandlerTestsBase : TestsBase
    {
        protected const string OriginalRowVersion = "BBBBBBBBABA=";
        protected const string RowVersion = "AAAAAAAAABA=";
        protected IPersonRepository _personRepositoryMock;
        protected IPunchItemRepository _punchItemRepositoryMock;
        protected ILibraryItemRepository _libraryItemRepositoryMock;
        protected IWorkOrderRepository _workOrderRepositoryMock;
        protected ISWCRRepository _swcrRepositoryMock;
        protected IDocumentRepository _documentRepositoryMock;
        protected ISyncToPCS4Service _syncToPCS4ServiceMock;
        protected PunchItem _existingPunchItem;
        protected PunchItem _punchItemPa;
        protected Person _currentPerson;
        protected LibraryItem _existingRaisedByOrg1;
        protected LibraryItem _existingRaisedByOrg2;
        protected LibraryItem _existingClearingByOrg1;
        protected LibraryItem _existingClearingByOrg2;
        protected LibraryItem _existingPriority1;
        protected LibraryItem _existingPriority2;
        protected LibraryItem _existingSorting1;
        protected LibraryItem _existingSorting2;
        protected LibraryItem _existingType1;
        protected LibraryItem _existingType2;
        protected Person _existingPerson1;
        protected Person _existingPerson2;
        protected WorkOrder _existingWorkOrder1;
        protected WorkOrder _existingWorkOrder2;
        protected SWCR _existingSWCR1;
        protected SWCR _existingSWCR2;
        protected Document _existingDocument1;
        protected Document _existingDocument2;

        [TestInitialize]
        public void PunchItemCommandHandlerTestsBaseSetup()
        {
            _punchItemRepositoryMock = Substitute.For<IPunchItemRepository>();
            _personRepositoryMock = Substitute.For<IPersonRepository>();
            _libraryItemRepositoryMock = Substitute.For<ILibraryItemRepository>();
            _workOrderRepositoryMock = Substitute.For<IWorkOrderRepository>();
            _swcrRepositoryMock = Substitute.For<ISWCRRepository>();
            _documentRepositoryMock = Substitute.For<IDocumentRepository>();
            _syncToPCS4ServiceMock = Substitute.For<ISyncToPCS4Service>();

            var id = 5;
            var project = new Project(TestPlantA, Guid.NewGuid(), null!, null!);
            _existingPunchItem = new PunchItem(
                TestPlantA,
                project,
                Guid.NewGuid(),
                Category.PA,
                Guid.NewGuid().ToString(),
                SetupLibraryItem(LibraryType.COMPLETION_ORGANIZATION, ++id),
                SetupLibraryItem(LibraryType.COMPLETION_ORGANIZATION, ++id));
            _existingPunchItem.SetRowVersion(OriginalRowVersion);
            _punchItemPa = _existingPunchItem;

            _punchItemRepositoryMock.GetAsync(_existingPunchItem.Guid, default)
                .Returns(_existingPunchItem);

            _existingRaisedByOrg1 = SetupLibraryItem(LibraryType.COMPLETION_ORGANIZATION, ++id);
            _existingRaisedByOrg2 = SetupLibraryItem(LibraryType.COMPLETION_ORGANIZATION, ++id);
            _existingClearingByOrg1 = SetupLibraryItem(LibraryType.COMPLETION_ORGANIZATION, ++id);
            _existingClearingByOrg2 = SetupLibraryItem(LibraryType.COMPLETION_ORGANIZATION, ++id);
            _existingPriority1 = SetupLibraryItem(LibraryType.PUNCHLIST_PRIORITY, ++id);
            _existingPriority2 = SetupLibraryItem(LibraryType.PUNCHLIST_PRIORITY, ++id);
            _existingSorting1 = SetupLibraryItem(LibraryType.PUNCHLIST_SORTING, ++id);
            _existingSorting2 = SetupLibraryItem(LibraryType.PUNCHLIST_SORTING, ++id);
            _existingType1 = SetupLibraryItem(LibraryType.PUNCHLIST_TYPE, ++id);
            _existingType2 = SetupLibraryItem(LibraryType.PUNCHLIST_TYPE, ++id);

            _currentPerson = SetupPerson(++id);
            _personRepositoryMock.GetCurrentPersonAsync(default)
                .Returns(_currentPerson);
            _existingPerson1 = SetupPerson(++id);
            _existingPerson2 = SetupPerson(++id);

            _existingWorkOrder1 = SetupWorkOrder(++id);
            _existingWorkOrder2 = SetupWorkOrder(++id);

            _existingSWCR1 = SetupSWCR(++id);
            _existingSWCR2 = SetupSWCR(++id);

            _existingDocument1 = SetupDocument(++id);
            _existingDocument2 = SetupDocument(++id);
        }

        private Document SetupDocument(int id)
        {
            var document = new Document(TestPlantA, Guid.NewGuid(), null!);
            document.SetProtectedIdForTesting(++id);

            _documentRepositoryMock
                .GetAsync(document.Guid, default)
                .Returns(document);

            return document;
        }

        private SWCR SetupSWCR(int id)
        {
            var swcr = new SWCR(TestPlantA, Guid.NewGuid(), id);
            swcr.SetProtectedIdForTesting(id);
            _swcrRepositoryMock
                .GetAsync(swcr.Guid, default)
                .Returns(swcr);

            return swcr;
        }

        private WorkOrder SetupWorkOrder(int id)
        {
            var workOrder = new WorkOrder(TestPlantA, Guid.NewGuid(), null!);
            workOrder.SetProtectedIdForTesting(id);
            _workOrderRepositoryMock
                .GetAsync(workOrder.Guid, default)
                .Returns(workOrder);

            return workOrder;
        }

        private Person SetupPerson(int id)
        {
            var person = new Person(Guid.NewGuid(), $"F{id}", $"L{id}", $"U{id}", "@");
            person.SetProtectedIdForTesting(id);
            _personRepositoryMock
                .GetAsync(person.Guid, default)
                .Returns(person);
            _personRepositoryMock
                .ExistsAsync(person.Guid, default)
                .Returns(true);
            return person;
        }

        protected LibraryItem SetupLibraryItem(LibraryType libraryType, int id)
        {
            var libraryGuid = Guid.NewGuid();
            var libraryItem = new LibraryItem(
                TestPlantA,
                libraryGuid,
                libraryGuid.ToString().Substring(0, 4),
                libraryGuid.ToString(),
                libraryType);
            libraryItem.SetProtectedIdForTesting(id);
            _libraryItemRepositoryMock.GetByGuidAndTypeAsync(libraryGuid, libraryType, default)
                .Returns(libraryItem);

            return libraryItem;
        }
    }
}
