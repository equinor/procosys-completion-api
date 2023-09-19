using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Command.EventHandlers;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
 using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.EventHandlers;

[TestClass]
public class EventDispatcherTests
{
    [TestMethod]
    public async Task DispatchPreSaveAsync_ShouldSendsOutEvents()
    {
        var mediator = Substitute.For<IMediator>();
        var dut = new EventDispatcher(mediator);
        var entities = new List<TestableEntity>();

        for (var i = 0; i < 3; i++)
        {
            var entity = new TestableEntity();
            entity.AddDomainEvent(new TestableDomainEvent());
            entity.AddPostSaveDomainEvent(Substitute.For<IPostSaveDomainEvent>());
            entities.Add(entity);
        }
        await dut.DispatchDomainEventsAsync(entities);

        await mediator.Received(3) 
            .Publish(Arg.Any<INotification>(), Arg.Any<CancellationToken>());

        entities.ForEach(e => Assert.AreEqual(0, e.DomainEvents.Count));
        entities.ForEach(e => Assert.AreEqual(1, e.PostSaveDomainEvents.Count));
    }

    [TestMethod]
    public async Task DispatchPostSaveAsync_ShouldSendsOutEvents()
    {
        var mediatorMock = Substitute.For<IMediator>();
        var dut = new EventDispatcher(mediatorMock);
        var entities = new List<TestableEntity>();

        for (var i = 0; i < 3; i++)
        {
            var entity = Substitute.For<TestableEntity>();
            entity.AddDomainEvent(new TestableDomainEvent());
            entity.AddPostSaveDomainEvent(Substitute.For<IPostSaveDomainEvent>());
            entities.Add(entity);
        }
        await dut.DispatchPostSaveEventsAsync(entities);

        await mediatorMock.Received(3)
            .Publish(Arg.Any<INotification>(), Arg.Any<CancellationToken>());

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
