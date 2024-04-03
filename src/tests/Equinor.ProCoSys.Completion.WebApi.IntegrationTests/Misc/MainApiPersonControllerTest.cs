using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Person;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Misc;

[TestClass]
public class MainApiPersonControllerTest
{
    private const string Route = "MainPersons";

    [TestMethod]
    public async Task Get_All_AsReader_ShouldReturnOk() => await GetAllPersons(UserType.Reader, TestFactory.PlantWithAccess);

    [TestMethod]
    public async Task Get_All_AsReader_ShouldReturnBadRequest_WhenUnknownPlant() 
        => await GetAllPersons(
            UserType.Reader, 
            "NOT_A_PLANT",
            HttpStatusCode.BadRequest);

    [TestMethod]
    public async Task Get_All_AsReader_ShouldReturnNoContent_WhenNoPersonsInPlant()
        => await GetAllPersons(
            UserType.Reader,
            TestFactory.PlantWithoutAccess,
            HttpStatusCode.NoContent);

    [TestMethod]
    public async Task Get_All_AsAnonymous_ShouldReturnUnauthorized()
        => await GetAllPersons(
            UserType.Anonymous,
            TestFactory.PlantWithAccess,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task Get_All_AsAnonymous_ShouldReturnUnauthorized_WhenUnknownPlant()
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
        var result = JsonConvert.DeserializeObject<List<ProCoSysPerson>>(content);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count == 2);
        Assert.IsTrue(result.Any(x => x.AzureOid == TestFactory.Person1.AzureOid));
        Assert.IsTrue(result.Any(x => x.AzureOid == TestFactory.Person2.AzureOid));
    }
}

