using System;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Command.Tests.EventHandlers.DomainEvents;

public class EventHandlerTestBase
{
    protected DateTime _now = new(2021, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    protected Person _person;
    protected PunchItem _punchItem;
    protected Project _project;

    [TestInitialize]
    public void SetupBase()
    {
        TimeService.SetProvider(new ManualTimeProvider(_now));
        _person = new Person(Guid.NewGuid(), null!, null!, null!, null!);
        _person.SetProtectedIdForTesting(3);

        var testPlant = "X";
        _project = new Project(testPlant, Guid.NewGuid(), null!, null!);
        var raisedByOrg = new LibraryItem(testPlant, Guid.NewGuid(), null!, null!, LibraryType.COMPLETION_ORGANIZATION);
        var clearingByOrg = new LibraryItem(testPlant, Guid.NewGuid(), null!, null!, LibraryType.COMPLETION_ORGANIZATION);
        _punchItem = new PunchItem(testPlant, _project, Guid.NewGuid(), null!, raisedByOrg, clearingByOrg);
    }
}
