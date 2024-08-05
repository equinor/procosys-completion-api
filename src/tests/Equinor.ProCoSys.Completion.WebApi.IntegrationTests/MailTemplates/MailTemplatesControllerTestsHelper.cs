using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.MailTemplates;

public static class MailTemplatesControllerTestsHelper
{
    private const string Route = "MailTemplates";

    public static async Task<List<MailTemplateDto>> GetAllMailTemplatesAsync(
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
        return JsonSerializer.Deserialize<List<MailTemplateDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
