using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.TieImport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

[TestClass]
public class ImportHandlerTests
{
    private IImportHandler _dut;

    [TestInitialize]
    public void Setup()
    {
        var serviceProvider = TestFactory.Instance.Services;
   
         _dut = serviceProvider.GetRequiredService<IImportHandler>();
    }
    
    [TestMethod]
    public async Task Handle_ShouldReturnFailedResult_WhenInvokedWithOutObjects()   
    {
        //Arrange and Act
       var result = await _dut.Handle(new TIInterfaceMessage());
       
       Assert.IsNotNull(result);
       Assert.IsTrue(result.Result == MessageResults.Failed);
       
    }
    
    [TestMethod]
    public async Task Handle_ShouldReturnFailedResult_WhenInvokedWithMultipleObjects()   
    {
        //Arrange 
        var tiInterfaceMessage = new TIInterfaceMessage();
        tiInterfaceMessage.Objects.AddRange([new TIObject(), new TIObject()]);;
        
        //Act
        var result = await _dut.Handle(tiInterfaceMessage);
       
        //Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result == MessageResults.Failed);
    }
}
