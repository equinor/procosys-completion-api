using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Documents;

[TestClass]
public class DocumentControllerTests
{
    [TestMethod]
    public async Task GetDocument_BySearch_AsReader_ShouldReturnDocument()
    {
        // Act
        var documents = await DocumentControllerTestsHelper.SearchForDocumentAsync(KnownData.DocumentNo[KnownData.PlantA], UserType.Reader);
        
        // Assert
        Assert.IsTrue(documents.Count == 1);
        Assert.IsTrue(documents.Any(a => a.guid == TestFactory.DocumentGuid));
    }
}

