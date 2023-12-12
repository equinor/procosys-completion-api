using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.LabelEntities;

[TestClass]
public class LabelEntitiesControllerTests : TestBase
{
    [TestMethod]
    public async Task GetLabelsForKnownEntity_AsReader_ShouldGetLabels()
    {
        // Act
        var punchItems = await LabelEntitiesControllerTestsHelper
            .GetLabelsForEntityAsync(UserType.Reader, KnownData.EntityTypeWithLabels.ToString());

        Assert.IsTrue(punchItems.Count > 0);
    }
}
