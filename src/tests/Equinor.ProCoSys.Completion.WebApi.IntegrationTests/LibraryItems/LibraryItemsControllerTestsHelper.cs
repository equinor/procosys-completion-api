using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.LibraryItems;

public static class LibraryItemsControllerTestsHelper
{
    private const string Route = "LibraryItems";

    public static async Task<List<LibraryItemDto>> GetLibraryItemsAsync(
        UserType userType,
        string plant,
        LibraryType libraryType,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var parameters = new ParameterCollection
        {
            { "libraryTypes", libraryType.ToString() }
        };
        var url = $"{Route}{parameters}";

        var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync(url);

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (expectedStatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<LibraryItemDto>>(content);
    }
}
