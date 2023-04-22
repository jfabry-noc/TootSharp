using System.Text.Json.Serialization;

namespace TootSharp
{
    public class TootContext
    {
        [JsonPropertyName("ancestors")]
        public List<Toot>? Ancestors { get; set; }

        [JsonPropertyName("descendants")]
        public List<Toot>? Descendants { get; set; }
    }
}