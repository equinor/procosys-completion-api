using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchEvents;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Command.Tests.EventHandlers.DomainEvents;

[TestClass]
public class PunchCreatedEventHandlerTests
{
    // ToDo Rename to better test name
    [TestMethod]
    public async Task Handle_ShouldDoSomething()
    {
        // Arrange
        var punch = new Punch("X", new Project("X", Guid.NewGuid(), "Pro", "Desc"), "F");
        var punchCreatedEvent = new PunchCreatedEvent(punch);
        var dut = new PunchCreatedEventHandler();

        // Act
        await dut.Handle(punchCreatedEvent, default);

        // ToDo Assert something
        Assert.IsTrue(true);
    }
}
