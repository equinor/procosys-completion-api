using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Punches;

public static class PunchesControllerTestsHelper
{
    private const string _route = "Punches";

    public static async Task<PunchDetailsDto> GetPunchAsync(
        UserType userType,
        string plant,
        Guid guid,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync($"{_route}/{guid}");

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (expectedStatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<PunchDetailsDto>(content);
    }

    public static async Task<List<LinkDto>> GetPunchLinksAsync(
        UserType userType,
        string plant,
        Guid guid,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync($"{_route}/{guid}/Links");

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (expectedStatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<LinkDto>>(content);
    }

    public static async Task<List<PunchDto>> GetAllPunchesInProjectAsync(
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
        var url = $"{_route}{parameters}";

        var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync(url);

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (expectedStatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<PunchDto>>(content);
    }

    public static async Task<GuidAndRowVersion> CreatePunchAsync(
        UserType userType,
        string plant,
        string itemNo,
        Guid projectGuid,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var bodyPayload = new
        {
            itemNo,
            projectGuid = projectGuid.ToString()
        };

        var serializePayload = JsonConvert.SerializeObject(bodyPayload);
        var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).PostAsync(_route, content);
        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<GuidAndRowVersion>(jsonString);
    }

    public static async Task<GuidAndRowVersion> CreatePunchLinkAsync(
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
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).PostAsync($"{_route}/{guid}/Links", content);
        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<GuidAndRowVersion>(jsonString);
    }

    public static async Task<GuidAndRowVersion> CreatePunchCommentAsync(
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
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).PostAsync($"{_route}/{guid}/Comments", content);
        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<GuidAndRowVersion>(jsonString);
    }


    public static async Task<List<AttachmentDto>> GetPunchAttachmentsAsync(
        UserType userType,
        string plant,
        Guid guid,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync($"{_route}/{guid}/Attachments");

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (expectedStatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<AttachmentDto>>(content);
    }

    public static async Task<string> GetPunchAttachmentDownloadUrlAsync(
        UserType userType,
        string plant,
        Guid guid,
        Guid attachmentGuid,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync($"{_route}/{guid}/Attachments/{attachmentGuid}");

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (expectedStatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<GuidAndRowVersion> UploadNewPunchAttachmentAsync(
        UserType userType,
        string plant,
        Guid guid,
        TestFile file,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var httpContent = file.CreateHttpContent();
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).PostAsync($"{_route}/{guid}/Attachments", httpContent);
        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        var jsonString = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<GuidAndRowVersion>(jsonString);
    }

    public static async Task<string> OverwriteExistingPunchAttachmentAsync(
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
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).PutAsync($"{_route}/{guid}/Attachments", httpContent);
        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        return await response.Content.ReadAsStringAsync();
    }

    public static async Task DeletePunchAttachmentAsync(
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
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{_route}/{guid}/Attachments/{attachmentGuid}")
        {
            Content = new StringContent(serializePayload, Encoding.UTF8, "application/json")
        };

        var response = await TestFactory.Instance.GetHttpClient(userType, plant).SendAsync(request);
        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);
    }

    public static async Task<string> UpdatePunchAsync(
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
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).PutAsync($"{_route}/{guid}", content);

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<string> UpdatePunchLinkAsync(
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
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).PutAsync($"{_route}/{guid}/Links/{linkGuid}", content);

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<string> VoidPunchAsync(
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
        var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).PutAsync($"{_route}/{guid}/Void", content);

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        return await response.Content.ReadAsStringAsync();
    }

    public static async Task DeletePunchAsync(
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
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{_route}/{guid}")
        {
            Content = new StringContent(serializePayload, Encoding.UTF8, "application/json")
        };

        var response = await TestFactory.Instance.GetHttpClient(userType, plant).SendAsync(request);
        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);
    }

    public static async Task DeletePunchLinkAsync(
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
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{_route}/{guid}/Links/{linkGuid}")
        {
            Content = new StringContent(serializePayload, Encoding.UTF8, "application/json")
        };

        var response = await TestFactory.Instance.GetHttpClient(userType, plant).SendAsync(request);
        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);
    }

    public static async Task<List<CommentDto>> GetPunchCommentsAsync(
        UserType userType,
        string plant,
        Guid guid,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null)
    {
        var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync($"{_route}/{guid}/Comments");

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

        if (expectedStatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<CommentDto>>(content);
    }
}
