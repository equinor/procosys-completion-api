using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchEvents;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Command.Tests.MassTransit;

[TestClass]
public class PunchCreatedTests
{

    [TestMethod]
    public async Task An_example_unit_test()
    {
        await using var provider = new ServiceCollection()
            .AddScoped<PunchCreatedEventHandler>()
            .AddMassTransitTestHarness(x =>
            {
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        var eventHandler = provider.CreateScope().ServiceProvider.GetRequiredService<PunchCreatedEventHandler>();
        await harness.Start();

        //act
       await eventHandler.Handle(new PunchCreatedEvent(
            new Punch("Plant", 
                new Project("Plant", new Guid(), "project", "sup"), 
                "TestItem1"),new Guid()), default);

       Assert.IsTrue(await harness.Published.Any<PunchCreatedIntegrationEvent>());
        
    }
}


