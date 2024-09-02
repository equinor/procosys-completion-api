using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.CheckLists;

[TestClass]
public class CheckListsControllerTests : TestBase
{
    [TestMethod]
    public async Task GetPunchItemsAsync_AsReader_ShouldGetPunchItems()
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

    [TestMethod]
    public async Task GetDuplicateInfoAsync_AsReader_ShouldGetPunchItems()
    {
        // Act
        var duplicateInfoDto = await CheckListsControllerTestsHelper.GetDuplicateInfoAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            TestFactory.CheckListGuidNotRestricted
        );

        // Assert
        Assert.IsNotNull(duplicateInfoDto);
        Assert.IsNotNull(duplicateInfoDto.CheckList);
        Assert.IsNotNull(duplicateInfoDto.Responsibles);
        Assert.IsTrue(duplicateInfoDto.Responsibles.Any());
        Assert.IsNotNull(duplicateInfoDto.TagFunctions);
        Assert.IsTrue(duplicateInfoDto.TagFunctions.Any());
    }
}
