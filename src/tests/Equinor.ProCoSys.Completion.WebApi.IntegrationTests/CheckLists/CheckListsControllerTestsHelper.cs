using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.CheckLists;

public class CheckListsControllerTestsHelper
{
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
    private const string Route = "CheckLists";

    public static async Task<IReadOnlyCollection<PunchItemDetailsDto>> GetPunchItemsAsync(
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
        return JsonSerializer.Deserialize<List<PunchItemDetailsDto>>(
                content,
                s_jsonSerializerOptions)
            .AsReadOnly();
    }
}
