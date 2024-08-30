using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public static class TestsHelper
{
    public static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task AssertResponseAsync(
        HttpResponseMessage response, 
        HttpStatusCode expectedStatusCode,
        string expectedMessagePartOnBadRequest)
    {
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Bad request details: {jsonString}");
                
            if (!string.IsNullOrEmpty(expectedMessagePartOnBadRequest))
            {
                var problemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(
                    jsonString, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                Assert.IsTrue(
                    // ReSharper disable once PossibleNullReferenceException
                    problemDetails.Errors.SelectMany(e => e.Value)
                        .Any(e => e.Contains(expectedMessagePartOnBadRequest)), 
                    $"Expected to find message part '{expectedMessagePartOnBadRequest}'");
            }
        }

        Assert.AreEqual(expectedStatusCode, response.StatusCode);
    }
}
