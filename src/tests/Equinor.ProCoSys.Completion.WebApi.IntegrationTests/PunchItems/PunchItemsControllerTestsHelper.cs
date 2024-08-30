using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.PunchItems;

public static class PunchItemsControllerTestsHelper
{
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
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
        return JsonSerializer.Deserialize<PunchItemDetailsDto>(content, s_jsonSerializerOptions);
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
        return JsonSerializer.Deserialize<List<LinkDto>>(content, s_jsonSerializerOptions);
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
        return JsonSerializer.Deserialize<List<PunchItemDto>>(content, s_jsonSerializerOptions);
    }

    public static async Task<GuidAndRowVersion> CreatePunchItemAsync(
        UserType userType,
        string plant,
        string category,
        string description,
        Guid checkListGuid,
        Guid raisedByOrgGuid,
        Guid clearingByOrgGuid,
        DateTime? dueTimeUtc = null,
        Guid? priorityGuid = null,
        Guid? sortingGuid = null,
        Guid? typeGuid = null,
        Guid? originalWorkOrderGuid = null,
        Guid? workOrderGuid = null,
        Guid? swcrGuid = null,
        Guid? documentGuid = null,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var bodyPayload = new
        {
            category,
            description,
            checkListGuid = checkListGuid.ToString(),
            raisedByOrgGuid = raisedByOrgGuid.ToString(),
            clearingByOrgGuid = clearingByOrgGuid.ToString(),
            dueTimeUtc = dueTimeUtc?.ToString("O"),
            priorityGuid = priorityGuid?.ToString(),
            sortingGuid = sortingGuid?.ToString(),
            typeGuid = typeGuid?.ToString(),
            originalWorkOrderGuid = originalWorkOrderGuid?.ToString(),
            workOrderGuid = workOrderGuid?.ToString(),
            swcrGuid = swcrGuid?.ToString(),
            documentGuid = documentGuid?.ToString()
        };

        var serializePayload = JsonSerializer.Serialize(bodyPayload);
        var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).PostAsync(Route, content);
        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GuidAndRowVersion>(jsonString, s_jsonSerializerOptions);
    }

    public static async Task<List<GuidAndRowVersion>> DuplicatePunchItemAsync(
        UserType userType,
        string plant,
        Guid guid,
        List<Guid> checkListGuids,
        bool duplicateAttachments,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var bodyPayload = new
        {
            checkListGuids,
            duplicateAttachments
        };

        var serializePayload = JsonSerializer.Serialize(bodyPayload);
        var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).PostAsync($"{Route}/{guid}/Duplicate", content);
        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<GuidAndRowVersion>>(jsonString, s_jsonSerializerOptions);
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

        var serializePayload = JsonSerializer.Serialize(bodyPayload);
        var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).PostAsync($"{Route}/{guid}/Links", content);
        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GuidAndRowVersion>(jsonString, s_jsonSerializerOptions);
    }

    public static async Task<GuidAndRowVersion> CreatePunchItemCommentAsync(
        UserType userType,
        string plant,
        Guid guid,
        string text,
        List<string> labels,
        List<Guid> mentions,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var bodyPayload = new
        {
            text,
            labels,
            mentions
        };

        var serializePayload = JsonSerializer.Serialize(bodyPayload);
        var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).PostAsync($"{Route}/{guid}/Comments", content);
        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GuidAndRowVersion>(jsonString, s_jsonSerializerOptions);
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
        return JsonSerializer.Deserialize<List<AttachmentDto>>(content, s_jsonSerializerOptions);
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

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GuidAndRowVersion>(jsonString, s_jsonSerializerOptions);
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

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

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
        var serializePayload = JsonSerializer.Serialize(bodyPayload);
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{Route}/{guid}/Attachments/{attachmentGuid}")
        {
            Content = new StringContent(serializePayload, Encoding.UTF8, "application/json")
        };

        var response = await TestFactory.Instance.GetHttpClient(userType, plant).SendAsync(request);
        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);
    }

    public static async Task<string> UpdatePunchItemAttachmentAsync(
        UserType userType,
        string plant,
        Guid guid,
        Guid attachmentGuid,
        string description,
        List<string> labels,
        string rowVersion,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var bodyPayload = new
        {
            description,
            labels,
            rowVersion
        };
        var serializePayload = JsonSerializer.Serialize(bodyPayload);
        var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).PutAsync($"{Route}/{guid}/Attachments/{attachmentGuid}", content);
        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<string> UpdatePunchItemAsync(
        UserType userType,
        string plant,
        Guid guid,
        JsonPatchDocument patchDocument,
        string rowVersion,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var bodyPayload = new
        {
            patchDocument = patchDocument.Operations,
            rowVersion
        };

        var serializePayload = JsonSerializer.Serialize(bodyPayload);
        var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).PatchAsync($"{Route}/{guid}", content);

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<string> UpdatePunchItemCategoryAsync(
        UserType userType,
        string plant,
        Guid guid,
        string category,
        string rowVersion,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var bodyPayload = new
        {
            category,
            rowVersion
        };

        var serializePayload = JsonSerializer.Serialize(bodyPayload);
        var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).PatchAsync($"{Route}/{guid}/UpdateCategory", content);

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
        string comment,
        List<Guid> mentions,
        string rowVersion,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var bodyPayload = new
        {
            comment,
            mentions,
            rowVersion
        };

        var serializePayload = JsonSerializer.Serialize(bodyPayload);
        var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).PostAsync($"{Route}/{guid}/Reject", content);

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        return await response.Content.ReadAsStringAsync();
    }

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

        var serializePayload = JsonSerializer.Serialize(bodyPayload);
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
        var serializePayload = JsonSerializer.Serialize(bodyPayload);
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
        var serializePayload = JsonSerializer.Serialize(bodyPayload);
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
        return JsonSerializer.Deserialize<List<CommentDto>>(content, s_jsonSerializerOptions);
    }

    public static async Task<List<HistoryDto>> GetPunchItemHistoryAsync(
        UserType userType,
        string plant,
        Guid guid,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync($"{Route}/{guid}/History");

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (expectedStatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<HistoryDto>>(content, s_jsonSerializerOptions);
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

        var serializePayload = JsonSerializer.Serialize(bodyPayload);
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
        Guid checkListGuid,
        Guid raisedByOrgGuid,
        Guid clearingByOrgGuid)
    {
        var (guid, rowVersionAfterClear) = await CreateClearedPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
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
        Guid checkListGuid,
        Guid raisedByOrgGuid,
        Guid clearingByOrgGuid)
    {
        var guidAndRowVersion = await CreatePunchItemAsync(
            userType,
            plant,
            "PA",
            Guid.NewGuid().ToString(),
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
