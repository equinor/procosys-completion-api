using System.Net;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.LibraryItems;

[TestClass]
public class LibraryItemsControllerNegativeTests : TestBase
{
    #region GetLibraryItems
    [TestMethod]
    public async Task GetLibraryItems_AsAnonymous_ShouldReturnUnauthorized()
        => await LibraryItemsControllerTestsHelper.GetLibraryItemsAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            [LibraryType.PUNCHLIST_SORTING, LibraryType.PUNCHLIST_PRIORITY],
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetLibraryItems_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await LibraryItemsControllerTestsHelper.GetLibraryItemsAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            [LibraryType.PUNCHLIST_SORTING, LibraryType.PUNCHLIST_PRIORITY],
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetLibraryItems_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await LibraryItemsControllerTestsHelper.GetLibraryItemsAsync(
            UserType.Writer,
            TestFactory.Unknown,
            [LibraryType.PUNCHLIST_SORTING, LibraryType.PUNCHLIST_PRIORITY], 
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetLibraryItems_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await LibraryItemsControllerTestsHelper.GetLibraryItemsAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            [LibraryType.PUNCHLIST_SORTING, LibraryType.PUNCHLIST_PRIORITY], 
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetLibraryItems_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await LibraryItemsControllerTestsHelper.GetLibraryItemsAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess, 
            [LibraryType.PUNCHLIST_SORTING, LibraryType.PUNCHLIST_PRIORITY], 
            HttpStatusCode.Forbidden);
    #endregion
}
