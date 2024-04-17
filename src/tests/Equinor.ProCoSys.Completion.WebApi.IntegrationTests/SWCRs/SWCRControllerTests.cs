using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.SWCRs;

[TestClass]
public class SWCRControllerTests
{

    [TestMethod]
    public async Task GetSWCR_BySearch_AsReader_ShouldReturnWorkOrder()
    {
        // Act
        var swcrs = await SWCRControllerTestsHelper.SearchForSWCRAsync(KnownData.OriginalSWCRNoAsString, UserType.Reader);

        // Assert
        Assert.IsTrue(swcrs.Count == 1);
        Assert.IsTrue(swcrs.Any(a => a.Guid == TestFactory.SWCRGuid));
    }
}
