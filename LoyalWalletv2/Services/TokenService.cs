using System.Text;
using System.Text.Json;

namespace LoyalWalletv2.Services;

public class TokenService : ITokenService
{
    private readonly HttpClient _httpClient;

    public TokenService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<string> GetToken()
    {
        var values = new Dictionary<string, object?>
        {
            {
                "apiId", OsmiInformation.ApiId
            },
            {
                "apiKey", OsmiInformation.ApiKey 
            }
        };

        var serializedValues = JsonSerializer.Serialize(values);

        using var requestMessage =
            new HttpRequestMessage(HttpMethod.Post, OsmiInformation.HostPrefix);
        requestMessage.Content = new StringContent(
            serializedValues,
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(requestMessage);
        
        var responseSerialised = await response.Content.ReadAsStringAsync();
        var (_, token) = JsonSerializer.Deserialize<KeyValuePair<string, string>>(responseSerialised);

        return token;
    }
}