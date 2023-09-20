using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.PunchItems;

public static class PunchItemsControllerTestsHelper
{
    private const string Route = "PunchItems";

    public static async Task<PunchItemDetailsDto> GetPunchItemAsync(
        UserType userType,
        string plant,
        Guid guid,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync($"{Route}/{guid}");

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (expectedStatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<PunchItemDetailsDto>(content);
    }

    public static async Task<List<LinkDto>> GetPunchItemLinksAsync(
        UserType userType,
        string plant,
        Guid guid,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync($"{Route}/{guid}/Links");

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (expectedStatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<LinkDto>>(content);
    }

    public static async Task<List<PunchItemDto>> GetAllPunchItemsInProjectAsync(
        UserType userType,
        string plant,
        Guid projectGuid,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var parameters = new ParameterCollection
        {
            { "projectGuid", projectGuid.ToString() },
            { "includeVoided", "true" }
        };
        var url = $"{Route}{parameters}";

        var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync(url);

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (expectedStatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<PunchItemDto>>(content);
    }

    public static async Task<GuidAndRowVersion> CreatePunchItemAsync(
        UserType userType,
        string plant,
        string description,
        Guid projectGuid,
        Guid checkListGuid,
        Guid raisedByOrgGuid,
        Guid clearingByOrgGuid,
        Guid? priorityGuid = null,
        Guid? sortingGuid = null,
        Guid? typeGuid = null,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var bodyPayload = new
        {
            description,
            projectGuid = projectGuid.ToString(),
            checkListGuid = checkListGuid.ToString(),
            raisedByOrgGuid = raisedByOrgGuid.ToString(),
            clearingByOrgGuid = clearingByOrgGuid.ToString(),
            priorityGuid = priorityGuid?.ToString(),
            sortingGuid = sortingGuid?.ToString(),
            typeGuid = typeGuid?.ToString()
        };

        var serializePayload = JsonConvert.SerializeObject(bodyPayload);
        var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).PostAsync(Route, content);
        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<GuidAndRowVersion>(jsonString);
    }

    public static async Task<GuidAndRowVersion> CreatePunchItemLinkAsync(
        UserType userType,
        string plant,
        Guid guid,
        string title,
        string url,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var bodyPayload = new
        {
            title,
            url
        };

        var serializePayload = JsonConvert.SerializeObject(bodyPayload);
        var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).PostAsync($"{Route}/{guid}/Links", content);
        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<GuidAndRowVersion>(jsonString);
    }

    public static async Task<GuidAndRowVersion> CreatePunchItemCommentAsync(
        UserType userType,
        string plant,
        Guid guid,
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
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).PostAsync($"{Route}/{guid}/Comments", content);
        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<GuidAndRowVersion>(jsonString);
    }


    public static async Task<List<AttachmentDto>> GetPunchItemAttachmentsAsync(
        UserType userType,
        string plant,
        Guid guid,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync($"{Route}/{guid}/Attachments");

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (expectedStatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<AttachmentDto>>(content);
    }

    public static async Task<string> GetPunchItemAttachmentDownloadUrlAsync(
        UserType userType,
        string plant,
        Guid guid,
        Guid attachmentGuid,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync($"{Route}/{guid}/Attachments/{attachmentGuid}");

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (expectedStatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<GuidAndRowVersion> UploadNewPunchItemAttachmentAsync(
        UserType userType,
        string plant,
        Guid guid,
        TestFile file,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var httpContent = file.CreateHttpContent();
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).PostAsync($"{Route}/{guid}/Attachments", httpContent);
        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        var jsonString = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<GuidAndRowVersion>(jsonString);
    }

    public static async Task<string> OverwriteExistingPunchItemAttachmentAsync(
        UserType userType,
        string plant,
        Guid guid,
        TestFile file,
        string rowVersion,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var httpContent = file.CreateHttpContent();
        httpContent.Add(new StringContent(rowVersion), nameof(rowVersion));
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).PutAsync($"{Route}/{guid}/Attachments", httpContent);
        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        return await response.Content.ReadAsStringAsync();
    }

    public static async Task DeletePunchItemAttachmentAsync(
        UserType userType,
        string plant,
        Guid guid,
        Guid attachmentGuid,
        string rowVersion,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var bodyPayload = new
        {
            rowVersion
        };
        var serializePayload = JsonConvert.SerializeObject(bodyPayload);
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{Route}/{guid}/Attachments/{attachmentGuid}")
        {
            Content = new StringContent(serializePayload, Encoding.UTF8, "application/json")
        };

        var response = await TestFactory.Instance.GetHttpClient(userType, plant).SendAsync(request);
        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);
    }

    public static async Task<string> UpdatePunchItemAsync(
        UserType userType,
        string plant,
        Guid guid,
        string description,
        string rowVersion,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var bodyPayload = new
        {
            description,
            rowVersion
        };

        var serializePayload = JsonConvert.SerializeObject(bodyPayload);
        var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
        // todo 104046 Refactor integration tests from put to patch
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).PutAsync($"{Route}/{guid}", content);

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<string> ClearPunchItemAsync(
        UserType userType,
        string plant,
        Guid guid,
        string rowVersion,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
        => await PostAsync(
            userType,
            plant,
            $"{Route}/{guid}/Clear",
            rowVersion,
            expectedStatusCode,
            expectedMessageOnBadRequest);

    public static async Task<string> UnclearPunchItemAsync(
        UserType userType,
        string plant,
        Guid guid,
        string rowVersion,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
        => await PostAsync(
            userType,
            plant,
            $"{Route}/{guid}/Unclear",
            rowVersion,
            expectedStatusCode,
            expectedMessageOnBadRequest);

    public static async Task<string> RejectPunchItemAsync(
        UserType userType,
        string plant,
        Guid guid,
        string rowVersion,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
        => await PostAsync(
            userType,
            plant,
            $"{Route}/{guid}/Reject",
            rowVersion,
            expectedStatusCode,
            expectedMessageOnBadRequest);

    public static async Task<string> VerifyPunchItemAsync(
        UserType userType,
        string plant,
        Guid guid,
        string rowVersion,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
        => await PostAsync(
            userType,
            plant,
            $"{Route}/{guid}/Verify",
            rowVersion,
            expectedStatusCode,
            expectedMessageOnBadRequest);

    public static async Task<string> UnverifyPunchItemAsync(
        UserType userType,
        string plant,
        Guid guid,
        string rowVersion,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
        => await PostAsync(
            userType,
            plant,
            $"{Route}/{guid}/Unverify",
            rowVersion,
            expectedStatusCode,
            expectedMessageOnBadRequest);

    public static async Task<string> UpdatePunchItemLinkAsync(
        UserType userType,
        string plant,
        Guid guid,
        Guid linkGuid,
        string title,
        string url,
        string rowVersion,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var bodyPayload = new
        {
            title,
            url,
            rowVersion
        };

        var serializePayload = JsonConvert.SerializeObject(bodyPayload);
        var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).PutAsync($"{Route}/{guid}/Links/{linkGuid}", content);

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        return await response.Content.ReadAsStringAsync();
    }

    public static async Task DeletePunchItemAsync(
        UserType userType,
        string plant,
        Guid guid,
        string rowVersion,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var bodyPayload = new
        {
            rowVersion
        };
        var serializePayload = JsonConvert.SerializeObject(bodyPayload);
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{Route}/{guid}")
        {
            Content = new StringContent(serializePayload, Encoding.UTF8, "application/json")
        };

        var response = await TestFactory.Instance.GetHttpClient(userType, plant).SendAsync(request);
        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);
    }

    public static async Task DeletePunchItemLinkAsync(
        UserType userType,
        string plant,
        Guid guid,
        Guid linkGuid,
        string rowVersion,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var bodyPayload = new
        {
            rowVersion
        };
        var serializePayload = JsonConvert.SerializeObject(bodyPayload);
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{Route}/{guid}/Links/{linkGuid}")
        {
            Content = new StringContent(serializePayload, Encoding.UTF8, "application/json")
        };

        var response = await TestFactory.Instance.GetHttpClient(userType, plant).SendAsync(request);
        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);
    }

    public static async Task<List<CommentDto>> GetPunchItemCommentsAsync(
        UserType userType,
        string plant,
        Guid guid,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync($"{Route}/{guid}/Comments");

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (expectedStatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<CommentDto>>(content);
    }

    private static async Task<string> PostAsync(
        UserType userType,
        string plant,
        string requestUri,
        string rowVersion,
        HttpStatusCode expectedStatusCode,
        string expectedMessageOnBadRequest)
    {
        var bodyPayload = new
        {
            rowVersion
        };

        var serializePayload = JsonConvert.SerializeObject(bodyPayload);
        var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).PostAsync(requestUri, content);

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<(Guid guid, string rowVersion)> CreateVerifiedPunchItemAsync(
        UserType userType,
        string plant,
        Guid projectGuid,
        Guid checkListGuid,
        Guid raisedByOrgGuid,
        Guid clearingByOrgGuid)
    {
        var (guid, rowVersionAfterClear) = await CreateClearedPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            projectGuid,
            checkListGuid,
            raisedByOrgGuid,
            clearingByOrgGuid);
        var rowVersionAfterVerify = await VerifyPunchItemAsync(
            userType,
            plant,
            guid,
            rowVersionAfterClear);

        return (guid, rowVersionAfterVerify);
    }

    public static async Task<(Guid guid, string rowVersion)> CreateClearedPunchItemAsync(
        UserType userType,
        string plant,
        Guid projectGuid,
        Guid checkListGuid,
        Guid raisedByOrgGuid,
        Guid clearingByOrgGuid)
    {
        var guidAndRowVersion = await CreatePunchItemAsync(
            userType,
            plant,
            Guid.NewGuid().ToString(),
            projectGuid,
            checkListGuid,
            raisedByOrgGuid,
            clearingByOrgGuid);
        var rowVersionAfterClear = await ClearPunchItemAsync(
            userType,
            plant,
            guidAndRowVersion.Guid,
            guidAndRowVersion.RowVersion);

        return (guidAndRowVersion.Guid, rowVersionAfterClear);
    }
}
