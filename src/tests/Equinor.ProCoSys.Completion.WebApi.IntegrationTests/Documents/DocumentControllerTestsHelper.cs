﻿using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Documents;

public class DocumentControllerTestsHelper
{
    private const string Route = "Documents/Search";

    public static async Task<List<DocumentDto>> SearchForDocumentAsync(
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
        return JsonConvert.DeserializeObject<List<DocumentDto>>(result);
    }
}
