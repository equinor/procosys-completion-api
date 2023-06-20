using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Command.EventHandlers;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.EventHandlers;

[TestClass]
public class EventDispatcherTests
{
    [TestMethod]
    public async Task DispatchPreSaveAsync_ShouldSendsOutEvents()
    {
        var mediator = new Mock<IMediator>();
        var dut = new EventDispatcher(mediator.Object);
        var entities = new List<TestableEntity>();

        for (var i = 0; i < 3; i++)
        {
            var entity = new Mock<TestableEntity>();
            entity.Object.AddDomainEvent(new TestableDomainEvent());
            entity.Object.AddPostSaveDomainEvent(new Mock<IPostSaveDomainEvent>().Object);
            entities.Add(entity.Object);
        }
        await dut.DispatchDomainEventsAsync(entities);

        mediator.Verify(x 
            => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Exactly(3));

        entities.ForEach(e => Assert.AreEqual(0, e.DomainEvents.Count));
        entities.ForEach(e => Assert.AreEqual(1, e.PostSaveDomainEvents.Count));
    }

    [TestMethod]
    public async Task DispatchPostSaveAsync_ShouldSendsOutEvents()
    {
        var mediator = new Mock<IMediator>();
        var dut = new EventDispatcher(mediator.Object);
        var entities = new List<TestableEntity>();

        for (var i = 0; i < 3; i++)
        {
            var entity = new Mock<TestableEntity>();
            entity.Object.AddDomainEvent(new TestableDomainEvent());
            entity.Object.AddPostSaveDomainEvent(new Mock<IPostSaveDomainEvent>().Object);
            entities.Add(entity.Object);
        }
        await dut.DispatchPostSaveEventsAsync(entities);

        mediator.Verify(x
            => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Exactly(3));

        entities.ForEach(e => Assert.AreEqual(1, e.DomainEvents.Count));
        entities.ForEach(e => Assert.AreEqual(0, e.PostSaveDomainEvents.Count));
    }
}

// The base classes are abstract, therefor sub classes needed to test it.
public class TestableEntity : EntityBase
{
}

public class TestableDomainEvent : IDomainEvent
{
}
