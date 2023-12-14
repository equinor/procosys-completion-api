using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.EventHandlers;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.WebApi.DIModules;
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Synchronization;

[TestClass]
public class ProjectEventConsumerTests
{
    [TestMethod]
    public async Task Consume_ShouldRejectMessage_IfNoProCoSysGuid()
    {
        var projectRepoMock = Substitute.For<IProjectRepository>();
        var unitOfWorkMock = Substitute.For<IUnitOfWork>();
        await using var provider = new ServiceCollection()
            .AddTransient<IProjectRepository>(_=> projectRepoMock)
            .AddPcsAuthIntegration()
            .AddScoped<IUnitOfWork>(_ => unitOfWorkMock)
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProjectEventConsumer>();
            })
            .BuildServiceProvider(true);
        
        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();


        var client = harness.GetRequestClient<ProjectEvent>();     
        
        var response = await  client.GetResponse<Response<Task>>(new ProjectEvent("test","description",false, 
            DateTime.MinValue, "SomePlant", Guid.Empty,"testName", null));
        

        Assert.IsTrue(await harness.Consumed.Any<ProjectEvent>());
        
        var consumerHarness = harness.GetConsumerHarness<ProjectEventConsumer>();

        Assert.IsTrue(await consumerHarness.Consumed.Any<ProjectEvent>());
    }
    
    
}
