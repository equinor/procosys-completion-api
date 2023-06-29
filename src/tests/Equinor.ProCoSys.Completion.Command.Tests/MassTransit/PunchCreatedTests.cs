// using System;
// using System.Threading.Tasks;
// using Equinor.ProCoSys.Completion.MessageContracts.Punch;
// using MassTransit;
// using MassTransit.Testing;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.VisualStudio.TestTools.UnitTesting;
//
// namespace Equinor.ProCoSys.Completion.Command.Tests.MassTransit;
//
// [TestClass]
// public class PunchCreatedTests
// {
//     private static readonly NewId s_guid1 = new();
//     private static readonly NewId s_guid2 = new();
//     private static readonly NewId s_guid3 = new();
//    
//     [TestMethod]
//     public async Task An_example_unit_test()
//     {
//         await using var provider = new ServiceCollection()
//             .AddMassTransitTestHarness(x =>
//             {
//                 x.AddConsumer<PunchCreatedTestV1Consumer>();
//             })
//             .BuildServiceProvider(true);
//
//         var harness = provider.GetRequiredService<ITestHarness>();
//
//         await harness.Start();
//
//         var client = harness.GetRequestClient<IPunchCreatedV1>();
//
//       
//     
//         var response = await client.GetResponse<IPunchCreatedV1>(new
//         {
//             Guid = s_guid1,
//             ProjectGuid = new Guid(),
//             ItemNo = "ItemNo",
//             CreatedByOid = new Guid(),
//             CreatedAtUtc = DateTime.UtcNow
//         });
//         
//         var response2 = await client.GetResponse<IPunchCreatedV1>(new
//         {
//             Guid = s_guid2,
//             ProjectGuid = new Guid(),
//             ItemNo = "ItemNo",
//             CreatedByOid = new Guid(),
//             CreatedAtUtc = DateTime.UtcNow
//         });
//
//         var response3 = await client.GetResponse<IPunchCreatedV1>(new
//         {
//             Guid = s_guid3,
//             ProjectGuid = new Guid(),
//             ItemNo = "ItemNo",
//             CreatedByOid = new Guid(),
//             CreatedAtUtc = DateTime.UtcNow
//         });
//
//         Assert.IsTrue(await harness.Sent.Any<IPunchCreatedV1>());
//
//         var consumerHarness = harness.GetConsumerHarness<PunchCreatedTestV1Consumer>();
//
//
//         Assert.IsTrue(await consumerHarness.Consumed.Any<IPunchCreatedV1>());
//
//         // test side effects of the SubmitOrderConsumer here
//     }
//
//     // ReSharper disable once ClassNeverInstantiated.Local
//     private class PunchCreatedTestV1Consumer : IConsumer<IPunchCreatedV1>
//     {
//         public Task Consume(ConsumeContext<IPunchCreatedV1> context)
//         {
//
//             if (context.Message.Guid == s_guid2.ToGuid())
//             {
//                 return context.RespondAsync("failed");
//             }
//                 
//         
//             return context.RespondAsync(context.Message.Guid);
//         }
//     }
// }
//
//
