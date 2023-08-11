using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands
{
    public class PunchItemCommandHandlerTestsBase : TestsBase
    {
        protected int _currentPersonId = 13;
        protected string _rowVersion = "AAAAAAAAABA=";
        protected IPersonRepository _personRepositoryMock;
        protected IPunchItemRepository _punchItemRepositoryMock;
        protected PunchItem _existingPunchItem;
        protected Person _currentPerson;

        [TestInitialize]
        public void PunchItemCommandHandlerTestsBaseSetup()
        {
            var project = new Project(TestPlantA, Guid.NewGuid(), null!, null!);
            var raisedByOrg = new LibraryItem(TestPlantA, Guid.NewGuid(), null!, null!, LibraryType.COMPLETION_ORGANIZATION);
            var clearingByOrg = new LibraryItem(TestPlantA, Guid.NewGuid(), null!, null!, LibraryType.COMPLETION_ORGANIZATION);
            _existingPunchItem = new PunchItem(TestPlantA, project, Guid.NewGuid(),null!, raisedByOrg, clearingByOrg);

        _punchItemRepositoryMock = new Mock<IPunchItemRepository>();
        _punchItemRepositoryMock.Setup(r => r.GetByGuidAsync(_existingPunchItem.Guid))
            .ReturnsAsync(_existingPunchItem);

            _currentPerson = new Person(Guid.NewGuid(), null!, null!, null!, null!);
            _currentPerson.SetProtectedIdForTesting(_currentPersonId);

            _personRepositoryMock = Substitute.For<IPersonRepository>();
            _personRepositoryMock.GetCurrentPersonAsync()
                .Returns(_currentPerson);
        }
    }
}
