using System.Text.Json.Serialization;

namespace TootSharp
{
    public class User
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("acct")]
        public string? Acct { get; set; }

        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("locked")]
        public bool? Locked { get; set; }

        [JsonPropertyName("bot")]
        public bool? Bot { get; set; }

        [JsonPropertyName("discoverable")]
        public bool? Discoverable { get; set; }

        [JsonPropertyName("group")]
        public bool? Group { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("note")]
        public string? Note { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("avatar")]
        public string? Avatar { get; set; }

        [JsonPropertyName("avatar_static")]
        public string? AvatarStatic { get; set; }

        [JsonPropertyName("header")]
        public string? Header { get; set; }

        [JsonPropertyName("header_static")]
        public string? HeaderStatic { get; set; }

        [JsonPropertyName("followers_count")]
        public int? FollowersCount { get; set; }

        [JsonPropertyName("following_count")]
        public int? FollowingCount { get; set; }

        [JsonPropertyName("statuses_count")]
        public int? StatusesCount { get; set; }

        [JsonPropertyName("last_status_at")]
        public string? LastStatusAt { get; set; }

        [JsonPropertyName("emojis")]
        public List<Emoji>? Emojis { get; set; }

        [JsonPropertyName("fields")]
        public List<Field>? Fields { get; set; }
    }
}