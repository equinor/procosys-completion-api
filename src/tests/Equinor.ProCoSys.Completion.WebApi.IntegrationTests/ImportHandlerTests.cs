using Equinor.ProCoSys.Completion.TieImport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

[TestClass]
public class ImportHandlerTests
{
    [TestMethod]
    public void Handle_ShouldReturnServiceResult_WhenInvokedWithNull()   
    {
        // Arrange
        var serviceProvider = TestFactory.Instance.Services;
   
        var importHandler = serviceProvider.GetRequiredService<IImportHandler>();

        // Act
        var result = importHandler.Handle(null!);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result.Results.Count == 0);
    }
}
