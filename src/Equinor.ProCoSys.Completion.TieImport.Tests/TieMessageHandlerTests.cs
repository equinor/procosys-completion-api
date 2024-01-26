using Equinor.ProCoSys.Completion.TieImport.Adapter;
using Equinor.ProCoSys.Completion.TieImport.Configuration;
using Equinor.TI.TIE.Adapter.TIE1.Message;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Tests;

[TestClass]

public class TieMessageHandlerTests
{
    Tie1MessageHandler _dut;

    [TestInitialize]
    public void Setup()
    {
        _dut = new Tie1MessageHandler(Substitute.For<ILogger<Tie1MessageHandler>>(), Substitute.For<IImportHandler>());
    }

    [TestMethod]
    public async Task HandleSingle_ReturnsReceipt()
    {
        //Arrange
        var tieMessage = new Tie1Message {Message = new TIInterfaceMessage {Guid = Guid.Empty, Site = "TestSite"}};

        //Act
        var result = await _dut.HandleSingle(new TieAdapterConfig(), tieMessage, CancellationToken.None);

        //Assert
        Assert.AreEqual(ReceiptStatus.Successful, result.Receipt.Status);
        Assert.AreEqual("Always successful for now...", result.Receipt.Comment);
    }

    [TestMethod]
    public async Task HandleSinglePerPartition_ReturnsReceipt()
    {
        //Arrange
        var tieMessage = new Tie1Message { Message = new TIInterfaceMessage { Guid = Guid.Empty, Site = "TestSite" } };

        //Act
        var result = await _dut.HandleSinglePerPartition(new TieAdapterConfig(), new TieAdapterPartitionConfig(), tieMessage, CancellationToken.None);

        //Assert
        Assert.AreEqual(ReceiptStatus.Successful, result.Receipt.Status);
        Assert.AreEqual("Always successful for now...", result.Receipt.Comment);
    }
}
