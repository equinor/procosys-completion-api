using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.LibraryItems;

[TestClass]
public class LibraryItemsControllerTests : TestBase
{
    [TestMethod]
    public async Task GetLibraryItems_AsReader_ShouldGetLibraryItems()
    {
        // Arrange
        var testPlant = TestFactory.PlantWithAccess;

        // Act
        var libraryItems = await LibraryItemsControllerTestsHelper
            .GetLibraryItemsAsync(
                UserType.Reader,
                testPlant,
                [LibraryType.PUNCHLIST_SORTING, LibraryType.COMM_PRIORITY]);

        // Assert
        var expectedLibraryItemGuid =
            TestFactory.Instance.SeededData[testPlant].PunchLibraryItemGuids;
        Assert.IsNotNull(libraryItems);
        Assert.AreEqual(expectedLibraryItemGuid.Count, libraryItems.Count);

        foreach (var guid in expectedLibraryItemGuid)
        {
            Assert.IsTrue(libraryItems.Any(l => l.Guid == guid));
        }
    }
}
