using System.Text.Json.Serialization;

namespace TootSharp
{
    public class AccessToken
    {
        [JsonPropertyName("access_token")]
        public string? AccessTokenValue { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }

        [JsonPropertyName("created_at")]
        public int? CreatedAt { get; set; }
    }
}