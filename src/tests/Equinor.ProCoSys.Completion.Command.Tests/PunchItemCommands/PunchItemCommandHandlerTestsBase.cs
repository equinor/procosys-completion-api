using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
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
        protected IProjectRepository _projectRepositoryMock;
        protected IPersonRepository _personRepositoryMock;
        protected IPunchItemRepository _punchItemRepositoryMock;
        protected ILibraryItemRepository _libraryItemRepositoryMock;
        protected IWorkOrderRepository _workOrderRepositoryMock;
        protected ISWCRRepository _swcrRepositoryMock;
        protected IDocumentRepository _documentRepositoryMock;
        protected ISyncToPCS4Service _syncToPCS4ServiceMock;
        protected IMessageProducer _messageProducerMock;
        protected Person _currentPerson;
        protected Person _existingPerson1;
        protected Person _existingPerson2;

        protected Dictionary<string, Project> _existingProject = new();
        protected Dictionary<string, PunchItem> _existingPunchItem = new();
        protected Dictionary<string, PunchItem> _punchItemPa = new();
        protected Dictionary<string, LibraryItem> _existingRaisedByOrg1 = new();
        protected Dictionary<string, LibraryItem> _existingRaisedByOrg2 = new();
        protected Dictionary<string, LibraryItem> _existingClearingByOrg1 = new();
        protected Dictionary<string, LibraryItem> _existingClearingByOrg2 = new();
        protected Dictionary<string, LibraryItem> _existingPriority1 = new();
        protected Dictionary<string, LibraryItem> _existingPriority2 = new();
        protected Dictionary<string, LibraryItem> _existingSorting1 = new();
        protected Dictionary<string, LibraryItem> _existingSorting2 = new();
        protected Dictionary<string, LibraryItem> _existingType1 = new();
        protected Dictionary<string, LibraryItem> _existingType2 = new();
        protected Dictionary<string, WorkOrder> _existingWorkOrder1 = new();
        protected Dictionary<string, WorkOrder> _existingWorkOrder2 = new();
        protected Dictionary<string, SWCR> _existingSWCR1 = new();
        protected Dictionary<string, SWCR> _existingSWCR2 = new();
        protected Dictionary<string, Document> _existingDocument1 = new();
        protected Dictionary<string, Document> _existingDocument2 = new();

        [TestInitialize]
        public void PunchItemCommandHandlerTestsBaseSetup()
        {
            _projectRepositoryMock = Substitute.For<IProjectRepository>();
            _punchItemRepositoryMock = Substitute.For<IPunchItemRepository>();
            _personRepositoryMock = Substitute.For<IPersonRepository>();
            _libraryItemRepositoryMock = Substitute.For<ILibraryItemRepository>();
            _workOrderRepositoryMock = Substitute.For<IWorkOrderRepository>();
            _swcrRepositoryMock = Substitute.For<ISWCRRepository>();
            _documentRepositoryMock = Substitute.For<IDocumentRepository>();
            _syncToPCS4ServiceMock = Substitute.For<ISyncToPCS4Service>();
            _messageProducerMock = Substitute.For<IMessageProducer>();
            var id = 5;
            _currentPerson = SetupPerson(++id);
            _personRepositoryMock.GetCurrentPersonAsync(default)
                .Returns(_currentPerson);
            _existingPerson1 = SetupPerson(++id);
            _existingPerson2 = SetupPerson(++id);

            AddTestDataToPlant(TestPlantA, id);
            AddTestDataToPlant(TestPlantB, id);
        }

        private void AddTestDataToPlant(string testPlant, int id)
        {
            var project = SetupProject(testPlant, ref id);

            _existingProject.Add(testPlant, project);
            _existingPunchItem.Add(testPlant, SetupPunchItem(testPlant, project, ref id));
            _punchItemPa.Add(testPlant, SetupPunchItem(testPlant, project, ref id));

            _existingRaisedByOrg1.Add(testPlant, SetupLibraryItem(testPlant, LibraryType.COMPLETION_ORGANIZATION, ++id));
            _existingRaisedByOrg2.Add(testPlant, SetupLibraryItem(testPlant, LibraryType.COMPLETION_ORGANIZATION, ++id));
            _existingClearingByOrg1.Add(testPlant, SetupLibraryItem(testPlant, LibraryType.COMPLETION_ORGANIZATION, ++id));
            _existingClearingByOrg2.Add(testPlant, SetupLibraryItem(testPlant, LibraryType.COMPLETION_ORGANIZATION, ++id));
            _existingPriority1.Add(testPlant, SetupLibraryItem(testPlant, LibraryType.PUNCHLIST_PRIORITY, ++id));
            _existingPriority2.Add(testPlant, SetupLibraryItem(testPlant, LibraryType.PUNCHLIST_PRIORITY, ++id));
            _existingSorting1.Add(testPlant, SetupLibraryItem(testPlant, LibraryType.PUNCHLIST_SORTING, ++id));
            _existingSorting2.Add(testPlant, SetupLibraryItem(testPlant, LibraryType.PUNCHLIST_SORTING, ++id));
            _existingType1.Add(testPlant, SetupLibraryItem(testPlant, LibraryType.PUNCHLIST_TYPE, ++id));
            _existingType2.Add(testPlant, SetupLibraryItem(testPlant, LibraryType.PUNCHLIST_TYPE, ++id));

            _existingWorkOrder1.Add(testPlant, SetupWorkOrder(testPlant, ++id));
            _existingWorkOrder2.Add(testPlant, SetupWorkOrder(testPlant, ++id));

            _existingSWCR1.Add(testPlant, SetupSWCR(testPlant, ++id));
            _existingSWCR2.Add(testPlant, SetupSWCR(testPlant, ++id));

            _existingDocument1.Add(testPlant, SetupDocument(testPlant, ++id));
            _existingDocument2.Add(testPlant, SetupDocument(testPlant, ++id));
        }

        private Project SetupProject(string testPlant, ref int id)
        {
            var project = new Project(testPlant, Guid.NewGuid(), null!, null!);
            project.SetProtectedIdForTesting(++id);
            _projectRepositoryMock
                .GetAsync(project.Guid, default)
                .Returns(project);
            return project;
        }

        private PunchItem SetupPunchItem(string testPlant, Project project, ref int id)
        {
            var punchItem = new PunchItem(
                testPlant,
                project,
                Guid.NewGuid(),
                Category.PA,
                Guid.NewGuid().ToString(),
                SetupLibraryItem(testPlant, LibraryType.COMPLETION_ORGANIZATION, ++id),
                SetupLibraryItem(testPlant, LibraryType.COMPLETION_ORGANIZATION, ++id)
            );

            punchItem.SetCreated(_currentPerson);
            punchItem.SetModified(_currentPerson);
            punchItem.SetProtectedIdForTesting(++id);
            punchItem.SetRowVersion(OriginalRowVersion);
            _punchItemRepositoryMock.GetAsync(punchItem.Guid, default).Returns(punchItem);
            return punchItem;
        }

        private Document SetupDocument(string plant, int id)
        {
            var document = new Document(plant, Guid.NewGuid(), Guid.NewGuid().ToString());
            document.SetProtectedIdForTesting(++id);

            _documentRepositoryMock
                .GetAsync(document.Guid, default)
                .Returns(document);

            return document;
        }

        private SWCR SetupSWCR(string plant, int id)
        {
            var swcr = new SWCR(plant, Guid.NewGuid(), id);
            swcr.SetProtectedIdForTesting(id);
            _swcrRepositoryMock
                .GetAsync(swcr.Guid, default)
                .Returns(swcr);

            return swcr;
        }

        private WorkOrder SetupWorkOrder(string plant, int id)
        {
            var workOrder = new WorkOrder(plant, Guid.NewGuid(), Guid.NewGuid().ToString());
            workOrder.SetProtectedIdForTesting(id);
            _workOrderRepositoryMock
                .GetAsync(workOrder.Guid, default)
                .Returns(workOrder);

            return workOrder;
        }

        private Person SetupPerson(int id)
        {
            var person = new Person(Guid.NewGuid(), $"F{id}", $"L{id}", $"U{id}", "@", false);
            person.SetProtectedIdForTesting(id);
            _personRepositoryMock
                .GetOrCreateAsync(person.Guid, default)
                .Returns(person);
            return person;
        }

        protected LibraryItem SetupLibraryItem(string plant, LibraryType libraryType, int id)
        {
            var libraryGuid = Guid.NewGuid();
            var libraryItem = new LibraryItem(
                plant,
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
