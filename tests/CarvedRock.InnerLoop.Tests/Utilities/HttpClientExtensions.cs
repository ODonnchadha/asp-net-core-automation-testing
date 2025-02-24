using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace CarvedRock.InnerLoop.Tests.Utilities;

public static class HttpClientExtensions
{
    private static readonly JsonSerializerOptions _jsonPrintOptions = 
        new() { WriteIndented = true };

    private static readonly JsonSerializerOptions _jsonDeserializeOptions = 
        new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Generic method. Url, expected HTTP status code, and an ITestOutputHelper.
    /// Perform a GET against the Url and call the DeserializeAndCheckResponse method.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="client"></param>
    /// <param name="url"></param>
    /// <param name="expectedStatus"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public static async Task<T> GetJsonResultAsync<T>(this HttpClient client, string url,
        HttpStatusCode expectedStatus, ITestOutputHelper output)
    {
        var response = await client.GetAsync(url);
        return await DeserializeAndCheckResponse<T>(response, expectedStatus, output);
    }

    public static async Task<T> PostForJsonResultAsync<T>(this HttpClient client, string url,
               object content, HttpStatusCode expectedStatus, ITestOutputHelper output)
    {
        var response = await client.PostAsJsonAsync(url, content);
        return await DeserializeAndCheckResponse<T>(response, expectedStatus, output);
    }

    /// <summary>
    /// First, did we receive the correct response code? And then read the string response. 
    /// And attampt to deserialize. Is it not null? Or did an error occur?
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="response"></param>
    /// <param name="expectedStatus"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    private static async Task<T> DeserializeAndCheckResponse<T>(HttpResponseMessage response,
        HttpStatusCode expectedStatus, ITestOutputHelper output)
    {
        var stringContent = await response.Content.ReadAsStringAsync();
        try
        {
            var result = string.IsNullOrEmpty(stringContent) 
                ? Activator.CreateInstance<T>()
                : JsonSerializer.Deserialize<T>(stringContent, _jsonDeserializeOptions);
            Assert.Equal(expectedStatus, response.StatusCode);
            Assert.NotNull(result);
            return result;
        }
        catch (Exception)
        {
            WriteOutput(stringContent, output);
            throw;
        }
    }

    /// <summary>
    /// Attempt to parse the string content as JSON. If not, simply write as string.
    /// </summary>
    /// <param name="stringContent"></param>
    /// <param name="output"></param>
    private static void WriteOutput(string stringContent, ITestOutputHelper output)
    {
        string? outputText;
        try
        {
            var jsonContent = JsonDocument.Parse(stringContent);
            outputText = JsonSerializer.Serialize(jsonContent, _jsonPrintOptions);
        }
        catch
        {
            outputText = stringContent;
        }
        output.WriteLine(outputText);
    }
}
