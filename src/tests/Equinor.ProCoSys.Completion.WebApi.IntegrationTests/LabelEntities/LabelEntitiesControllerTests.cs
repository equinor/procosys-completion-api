using System.Linq;
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
        var labels = await LabelEntitiesControllerTestsHelper
            .GetLabelsForEntityAsync(UserType.Reader, KnownData.EntityTypeWithLabels);

        Assert.IsTrue(labels.Count >= 2);
        Assert.IsTrue(labels.Any(l => l == KnownData.LabelA));
        Assert.IsTrue(labels.Any(l => l == KnownData.LabelB));
    }

    // The NoPermissionUser has no specific permissions, but are authorized.
    // Should be able to use endpoint
    [TestMethod]
    public async Task GetLabelsForKnownEntity_AsNoPermissionUser_ShouldGetLabels()
    {
        // Act
        var labels = await LabelEntitiesControllerTestsHelper
            .GetLabelsForEntityAsync(UserType.NoPermissionUser, KnownData.EntityTypeWithLabels);

        Assert.IsTrue(labels.Count >= 2);
        Assert.IsTrue(labels.Any(l => l == KnownData.LabelA));
        Assert.IsTrue(labels.Any(l => l == KnownData.LabelB));
    }
}
