using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PlayGround;

/// <summary>
/// 令牌刷新
/// 用于获取新的API访问令牌
/// </summary>
internal class TokenRefresh
{
    private HttpClient Client { get; }

    internal TokenFileModel TokenFile { get; }

    internal DateTime ExporesTime { get; set; }

    public TokenRefresh()
    {
        Client    = new HttpClient();
        TokenFile = JObject.Parse(File.ReadAllText("client.json")).ToObject<TokenFileModel>();
    }

    /// <summary>
    /// 刷新API访问令牌
    /// </summary>
    /// <returns>
    /// 令牌的下一次过期时间(s)
    /// </returns>
    internal async Task<(bool, Exception)> RefreshToken()
    {
        JObject apiResult;
        try
        {
            var pairs = new List<KeyValuePair<string, string>>
            {
                new("client_id", TokenFile.ClientId),
                new("redirect_uri", "http://localhost/auth"),
                new("client_secret", TokenFile.Secret),
                new("refresh_token", TokenFile.RefreshToken),
                new("grant_type", "refresh_token")
            };
            var content = new FormUrlEncodedContent(pairs);
            HttpResponseMessage response =
                await Client.PostAsync("https://login.microsoftonline.com/common/oauth2/v2.0/token", content);
            if (!response.IsSuccessStatusCode)
                return (false, new HttpRequestException($"response code is {response.StatusCode}"));
            string result = await response.Content.ReadAsStringAsync();
            apiResult = JObject.Parse(result);
        }
        catch (Exception e)
        {
            return (false, e);
        }

        string tokenStr = apiResult["access_token"]?.ToString() ?? string.Empty;
        if (string.IsNullOrEmpty(tokenStr)) return (false, new NullReferenceException("tokenStr is null"));

        TokenFile.AccessToken = tokenStr;

        await File.WriteAllTextAsync("client.json", JsonConvert.SerializeObject(TokenFile));

        ExporesTime = DateTime.Now + TimeSpan.FromSeconds(Convert.ToInt32(apiResult["expires_in"] ?? 0));

        return (true, null);
    }
}