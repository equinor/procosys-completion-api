using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.CheckLists;

[TestClass]
public class CheckListsControllerTests : TestBase
{
    private Guid _checkListGuidUnderTest;

    [TestInitialize]
    public void TestInitialize()
        => _checkListGuidUnderTest = TestFactory.Instance.SeededData[TestFactory.PlantWithAccess].PunchItemA.CheckListGuid;

    [TestMethod]
    public async Task GetPunchItemsByCheckListGuid_AsReader_ShouldGetPunchItems()
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
