using System.Text.Json.Serialization;

namespace TootSharp
{
    public class Option
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("votes_count")]
        public int? VotesCount { get; set; }
    }

    public class Notification
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("account")]
        public Account? Account { get; set; }

        [JsonPropertyName("status")]
        public Toot? Status { get; set; }
    }
}