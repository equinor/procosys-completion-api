using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Labels;

public static class LabelsControllerTestsHelper
{
    private const string Route = "Labels";

    public static async Task<List<LabelDto>> GetLabelsAsync(
        UserType userType,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var response = await TestFactory.Instance.GetHttpClient(userType, null).GetAsync(Route);

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (expectedStatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<LabelDto>>(content);
    }

    public static async Task<string> CreateLabelAsync(
        UserType userType,
        string text,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var bodyPayload = new
        {
            text
        };

        var serializePayload = JsonConvert.SerializeObject(bodyPayload);
        var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
        var response = await TestFactory.Instance.GetHttpClient(userType, null).PostAsync(Route, content);
        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        return await response.Content.ReadAsStringAsync();
    }

    public static async Task UpdateLabelAsync(
        UserType userType,
        string text,
        List<EntityTypeWithLabel> availableForLabels,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var bodyPayload = new
        {
            text,
            availableForLabels
        };

        var serializePayload = JsonConvert.SerializeObject(bodyPayload);
        var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
        var response = await TestFactory.Instance.GetHttpClient(userType, null).PutAsync(Route, content);
        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);
    }
}
