using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Misc;

[TestClass]
public class CacheControllerTests : TestBase
{
    private const string Route = "Cache";

    [TestMethod]
    public async Task Get_PrefetchCheckListByGuid_ShouldReturnOk() => await PrefetchCheckListByGuid(UserType.Writer);

    private static async Task PrefetchCheckListByGuid(UserType userType, HttpStatusCode expectedHttpStatusCode = HttpStatusCode.OK)
    {
        var checkListGuid = KnownData.CheckListGuidA.Values.First();
        var response = await TestFactory.Instance.GetHttpClient(userType, null).GetAsync($"{Route}/PrefetchCheckListByGuid/{checkListGuid}");

        // Assert
        Assert.AreEqual(expectedHttpStatusCode, response.StatusCode);
    }
}

