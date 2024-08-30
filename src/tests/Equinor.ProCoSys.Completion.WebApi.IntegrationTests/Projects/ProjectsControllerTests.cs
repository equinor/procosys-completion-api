using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Projects;

[TestClass]
public class ProjectsControllerTests : TestBase
{
    [TestMethod]
    public async Task GetAllPunchItemsInProject_AsReader_ShouldGetPunchItems()
    {
        // Act
        var punchItems = await ProjectsControllerTestsHelper
            .GetAllPunchItemsAsync(UserType.Reader, TestFactory.PlantWithAccess, TestFactory.ProjectGuidWithAccess);

        // Assert (can't assert the exact number since other tests creates items in in-memory db)
        Assert.IsTrue(punchItems.Count > 0);
    }
}
