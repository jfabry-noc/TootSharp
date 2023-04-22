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

    public class Poll
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("expires_at")]
        public DateTime? ExpiresAt { get; set; }

        [JsonPropertyName("expired")]
        public bool? Expired { get; set; }

        [JsonPropertyName("multiple")]
        public bool? Multiple { get; set; }

        [JsonPropertyName("votes_count")]
        public int? VotesCount { get; set; }

        [JsonPropertyName("voters_count")]
        public int? VotersCount { get; set; }

        [JsonPropertyName("voted")]
        public bool? Voted { get; set; }

        [JsonPropertyName("own_votes")]
        public List<int?>? OwnVotes { get; set; }

        [JsonPropertyName("options")]
        public List<Option>? Options { get; set; }

        [JsonPropertyName("emojis")]
        public List<object>? Emojis { get; set; }
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