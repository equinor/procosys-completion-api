using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Misc;

[TestClass]
public class MainApiPersonControllerTest
{
    private const string Route = "MainPersons";

    [TestMethod]
    public async Task GetAll_AsReader_ShouldReturnOk() => await GetAllPersons(UserType.Reader, TestFactory.PlantWithAccess);

    [TestMethod]
    public async Task GetAll_AsReader_ShouldReturnBadRequest_WhenUnknownPlant() 
        => await GetAllPersons(
            UserType.Reader, 
            "NOT_A_PLANT",
            HttpStatusCode.BadRequest,
            "Plant 'NOT_A_PLANT' is not a valid plant");

    [TestMethod]
    public async Task GetAll_AsReader_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await GetAllPersons(
            UserType.Reader,
            TestFactory.PlantWithoutAccess,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetAll_AsAnonymous_ShouldReturnUnauthorized()
        => await GetAllPersons(
            UserType.Anonymous,
            TestFactory.PlantWithAccess,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetAll_AsAnonymous_ShouldReturnUnauthorized_WhenUnknownPlant()
        => await GetAllPersons(
            UserType.Anonymous,
            "NOT_A_PLANT",
            HttpStatusCode.Unauthorized);

    private static async Task GetAllPersons(UserType userType, string plant, HttpStatusCode expectedHttpStatusCode = HttpStatusCode.OK, string expectedMessageOnBadRequest = null)
    {
        // Act
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync($"{Route}/All");

        // Assert
        await TestsHelper.AssertResponseAsync(response, expectedHttpStatusCode, expectedMessageOnBadRequest);
        
        if (expectedHttpStatusCode != HttpStatusCode.OK)
        {
            return;
        }
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsNotNull(content);
        var result = JsonSerializer.Deserialize<List<ProCoSysTestPerson>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count == 2);
        Assert.IsTrue(result.Any(x => x.AzureOid == TestFactory.Person1.AzureOid));
        Assert.IsTrue(result.Any(x => x.AzureOid == TestFactory.Person2.AzureOid));
    }
}

