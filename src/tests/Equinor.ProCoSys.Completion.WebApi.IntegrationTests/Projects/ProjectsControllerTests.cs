using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Projects;

[TestClass]
public class ProjectsControllerTests : TestBase
{
    [TestMethod]
    public async Task GetAllPunchItemsAsync_AsReader_ShouldGetPunchItems()
    {
        // Act
        var punchItems = await ProjectsControllerTestsHelper
            .GetAllPunchItemsAsync(UserType.Reader, TestFactory.PlantWithAccess, TestFactory.ProjectGuidWithAccess);

        // Assert (can't assert the exact number since other tests creates items in in-memory db)
        Assert.IsTrue(punchItems.Any());
    }

    [TestMethod]
    public async Task SearchCheckListsAsync_AsReader_ShouldGetCheckLists()
    {
        // Act
        var searchResult = await ProjectsControllerTestsHelper
            .SearchCheckListsAsync(UserType.Reader, TestFactory.PlantWithAccess, TestFactory.ProjectGuidWithAccess);

        Assert.IsNotNull(searchResult);
        Assert.IsTrue(searchResult.MaxAvailable > 0);
        Assert.IsTrue(searchResult.Items.Any());
    }
}
