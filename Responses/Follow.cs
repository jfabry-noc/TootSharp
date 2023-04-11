using System.Text.Json.Serialization;

namespace TootSharp
{
    public class Follow
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("following")]
        public bool? Following { get; set; }

        [JsonPropertyName("showing_reblogs")]
        public bool? ShowingReblogs { get; set; }

        [JsonPropertyName("notifying")]
        public bool? Notifying { get; set; }

        [JsonPropertyName("languages")]
        public object? Languages { get; set; }

        [JsonPropertyName("followed_by")]
        public bool? FollowedBy { get; set; }

        [JsonPropertyName("blocking")]
        public bool? Blocking { get; set; }

        [JsonPropertyName("blocked_by")]
        public bool? BlockedBy { get; set; }

        [JsonPropertyName("muting")]
        public bool? Muting { get; set; }

        [JsonPropertyName("muting_notifications")]
        public bool? MutingNotifications { get; set; }

        [JsonPropertyName("requested")]
        public bool? Requested { get; set; }

        [JsonPropertyName("domain_blocking")]
        public bool? DomainBlocking { get; set; }

        [JsonPropertyName("endorsed")]
        public bool? Endorsed { get; set; }

        [JsonPropertyName("note")]
        public string? Note { get; set; }
    }
}