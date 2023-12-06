using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Labels;

[TestClass]
public class LabelsControllerTests : TestBase
{
    [TestMethod]
    public async Task GetLabelsForKnownHost_AsReader_ShouldGetLabels()
    {
        // Act
        var punchItems = await LabelsControllerTestsHelper
            .GetLabelsForHostAsync(UserType.Reader, KnownData.HostTypeWithLabels.ToString());

        Assert.IsTrue(punchItems.Count > 0);
    }
}
