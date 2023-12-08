using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.LabelEntities;

public static class LabelEntitiesControllerTestsHelper
{
    private const string Route = "Labels";

    public static async Task<List<string>> GetLabelsForEntityAsync(
        UserType userType,
        string entityWithLabelsType,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var parameters = new ParameterCollection
        {
            { "entityWithLabelsType", entityWithLabelsType }
        };
        var url = $"{Route}{parameters}";

        var response = await TestFactory.Instance.GetHttpClient(userType, null).GetAsync(url);

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (expectedStatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<string>>(content);
    }
}
