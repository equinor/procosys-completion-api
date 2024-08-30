using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Projects;

public class ProjectsControllerTestsHelper
{
    private const string Route = "Projects";

    public static async Task<List<PunchItemDto>> GetAllPunchItemsAsync(
        UserType userType,
        string plant,
        Guid guid,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync($"{Route}/{guid}/PunchItems");

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (expectedStatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<PunchItemDto>>(content, TestsHelper.JsonSerializerOptions);
    }
}
