using System.Text.Json.Serialization;

namespace TootSharp
{
    public class AppRegistration
    {
        [JsonPropertyName("client_id")]
        public string? ClientId { get; set; }

        [JsonPropertyName("client_secret")]
        public string? ClientSecret { get; set; }
    }
}