using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.WorkOrders;

public static class WorkOrderControllerTestsHelper
{
    private const string Route = "WorkOrders/Search";

    public static async Task<List<WorkOrderDto>> SearchForWorkOrderAsync(
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
        return JsonSerializer.Deserialize<List<WorkOrderDto>>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}

