using System.Text;
using Equinor.TI.CommonLibrary.Mapper;
using Equinor.TI.CommonLibrary.SchemaModel;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.Completion.TieImport.Services;
public class TieApiSource(IHttpClientFactory httpClientFactory) : ISchemaCacheSource
{
    public static string ClientName = "TieHttpClient";
    
    public SchemaDTO Get(string schemaFrom, string schemaTo)
    {
        if (string.IsNullOrWhiteSpace(schemaFrom) || string.IsNullOrWhiteSpace(schemaTo) || schemaFrom == schemaTo)
        {
            throw new ArgumentException("`schemaFrom` and `schemaTo` must specify two different schemata");
        }

        var query = $"{{\"schemaName\":\"{schemaFrom}\",\"version\":null,\"includeEnums\":false,\"includeAllExtensions\":true,\"scope\":null,\"includeMappingForSchema\":\"{schemaTo}\"}}";
        var content = new StringContent(query, Encoding.UTF8, "application/json");

        var response = SendRequestAsync(HttpMethod.Post, "/api/schema", content).GetAwaiter().GetResult();

        var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        var schema = JsonConvert.DeserializeObject<SchemaDTO>(body);

        return schema!;
    }

    public DateTime GetLastUpdatedDate(string schemaFrom, string schemaTo)
    {
        if (string.IsNullOrWhiteSpace(schemaFrom) || string.IsNullOrWhiteSpace(schemaTo) || schemaFrom == schemaTo)
        {
            throw new ArgumentException("`schemaFrom` and `schemaTo` must specify two different schemata");
        }

        var response = SendRequestAsync(HttpMethod.Get, $"/api/schema/lastUpdated/{schemaFrom}/{schemaTo}").GetAwaiter().GetResult();

        var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        var time = JsonConvert.DeserializeObject<DateTime>(body);

        return time;
    }

    public async Task<HttpResponseMessage> SendRequestAsync(HttpMethod verb, string url, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(verb, url);

        if (content != null)
        {
            request.Content = content;
        }

        try
        {
            // Send the request asynchronously and return the response
            var client = httpClientFactory.CreateClient(ClientName);
            return await client.SendAsync(request);
        }
        catch (HttpRequestException ex)
        {
            // Handle specific HTTP request exceptions as needed
            // Log the exception or rethrow it based on your application's needs
            throw new Exception("An error occurred while sending the request.", ex);
        }
        catch (Exception ex)
        {
            // Handle other exceptions as necessary
            throw new Exception("An unexpected error occurred.", ex);
        }
    }
}

