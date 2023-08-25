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
        protected const int CurrentPersonId = 13;
        protected const string RowVersion = "AAAAAAAAABA=";
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

        _punchItemRepositoryMock = Substitute.For<IPunchItemRepository>();
        _punchItemRepositoryMock.GetByGuidAsync(_existingPunchItem.Guid)
            .Returns(_existingPunchItem);

            _currentPerson = new Person(Guid.NewGuid(), null!, null!, null!, null!);
            _currentPerson.SetProtectedIdForTesting(CurrentPersonId);

            _personRepositoryMock = Substitute.For<IPersonRepository>();
            _personRepositoryMock.GetCurrentPersonAsync()
                .Returns(_currentPerson);
        }
    }
}
