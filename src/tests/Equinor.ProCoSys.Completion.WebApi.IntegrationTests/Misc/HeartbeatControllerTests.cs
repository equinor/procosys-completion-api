using System.Net;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Misc;

[TestClass]
public class HeartbeatControllerTests : TestBase
{
    private const string Route = "Heartbeat";

    [TestMethod]
    public async Task Get_IsAlive_AsAnonymous_ShouldReturnOk() => await AssertIsAlive(UserType.Anonymous);

    [TestMethod]
    public async Task Get_IsAlive_AsHacker_ShouldReturnOk() => await AssertIsAlive(UserType.NoPermissionUser);

    private static async Task AssertIsAlive(UserType userType, HttpStatusCode expectedHttpStatusCode = HttpStatusCode.OK)
    {
        var response = await TestFactory.Instance.GetHttpClient(userType, null).GetAsync($"{Route}/IsAlive");

        // Assert
        Assert.AreEqual(expectedHttpStatusCode, response.StatusCode);
        if (expectedHttpStatusCode != HttpStatusCode.OK)
        {
            return;
        }
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsNotNull(content);
        var dto = JsonSerializer.Deserialize<HeartbeatDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.IsNotNull(dto);
        Assert.IsTrue(dto.IsAlive);
        Assert.IsNotNull(dto.TimeStamp);
    }
}
