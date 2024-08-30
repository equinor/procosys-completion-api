using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.CheckLists;

[TestClass]
public class CheckListsControllerTests : TestBase
{
    [TestMethod]
    public async Task GetPunchItems_AsReader_ShouldGetPunchItems()
    {
        // Act
        var punchItems = await CheckListsControllerTestsHelper.GetPunchItemsAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            TestFactory.CheckListGuidNotRestricted
        );

        // Assert
        Assert.IsTrue(punchItems.Any());
    }
}
