using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.SWCRs;

[TestClass]
public class SWCRControllerNegativeTests
{
    [TestMethod]
    public async Task GetSWCR_BySearch_AsAnonymous_ShouldReturnUnauthorized()
        => await SWCRControllerTestsHelper.SearchForSWCRAsync(
            KnownData.SWCRNo[KnownData.SWCRGuid[KnownData.PlantA]].ToString(),
            UserType.Anonymous,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetSWCR_BySearch_AsNoPermissionUser_ShouldReturnForbidden()
        => await SWCRControllerTestsHelper.SearchForSWCRAsync(
            KnownData.SWCRNo[KnownData.SWCRGuid[KnownData.PlantA]].ToString(),
            UserType.NoPermissionUser,
            HttpStatusCode.Forbidden);
}
