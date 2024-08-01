using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.SWCRs;

public class SWCRControllerTestsHelper
{
    private const string Route = "SWCR/Search";

    public static async Task<List<SWCRDto>> SearchForSWCRAsync(
        string text,
        UserType userType,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {

        var parameters = new ParameterCollection
        {
            { "searchPhrase", text}
        };
        var url = $"{Route}{parameters}";

        var response = await TestFactory.Instance.GetHttpClient(userType, TestFactory.PlantWithAccess).GetAsync(url);

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (expectedStatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var result = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<SWCRDto>>(result, new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
    }
}
