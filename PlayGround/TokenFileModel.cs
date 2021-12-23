using Newtonsoft.Json;

namespace PlayGround;

internal class TokenFileModel
{
    [JsonProperty("client-id")]
    public string ClientId { get; set; }

    [JsonProperty("client-code")]
    public string ClientCode { get; set; }

    [JsonProperty("secret")]
    public string Secret { get; set; }

    [JsonProperty("access-token")]
    public string AccessToken { get; set; }

    [JsonProperty("refresh-token")]
    public string RefreshToken { get; set; }
}