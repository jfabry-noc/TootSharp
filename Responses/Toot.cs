using System.Text.Json.Serialization;

namespace TootSharp
{
    public class Account
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
        public List<object>? Emojis { get; set; }

        [JsonPropertyName("fields")]
        public List<Field>? Fields { get; set; }

        [JsonPropertyName("noindex")]
        public bool? Noindex { get; set; }

        [JsonPropertyName("roles")]
        public List<object>? Roles { get; set; }

        public override string ToString()
        {
            var baseValue = base.ToString();
            if(Acct is not null)
            {
                return $"@{Acct}";
            }
            else if(baseValue is not null)
            {
                return baseValue;
            }
            return "";
        }
    }

    public class Application
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("website")]
        public object? Website { get; set; }
    }

    public class Card
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("author_name")]
        public string? AuthorName { get; set; }

        [JsonPropertyName("author_url")]
        public string? AuthorUrl { get; set; }

        [JsonPropertyName("provider_name")]
        public string? ProviderName { get; set; }

        [JsonPropertyName("provider_url")]
        public string? ProviderUrl { get; set; }

        [JsonPropertyName("html")]
        public string? Html { get; set; }

        [JsonPropertyName("width")]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }

        [JsonPropertyName("image")]
        public string? Image { get; set; }

        [JsonPropertyName("embed_url")]
        public string? EmbedUrl { get; set; }

        [JsonPropertyName("blurhash")]
        public string? Blurhash { get; set; }
    }

    public class Emoji
    {
        [JsonPropertyName("shortcode")]
        public string? Shortcode { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("static_url")]
        public string? StaticUrl { get; set; }

        [JsonPropertyName("visible_in_picker")]
        public bool? VisibleInPicker { get; set; }
    }

    public class Field
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("verified_at")]
        public DateTime? VerifiedAt { get; set; }
    }

    public class Focus
    {
        [JsonPropertyName("x")]
        public double? X { get; set; }

        [JsonPropertyName("y")]
        public double? Y { get; set; }
    }

    public class MediaAttachment
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("preview_url")]
        public string? PreviewUrl { get; set; }

        [JsonPropertyName("remote_url")]
        public string? RemoteUrl { get; set; }

        [JsonPropertyName("preview_remote_url")]
        public object? PreviewRemoteUrl { get; set; }

        [JsonPropertyName("text_url")]
        public object? TextUrl { get; set; }

        [JsonPropertyName("meta")]
        public Meta? Meta { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("blurhash")]
        public string? Blurhash { get; set; }
    }

    public class Mention
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("acct")]
        public string? Acct { get; set; }
    }

    public class Meta
    {
        [JsonPropertyName("focus")]
        public Focus? Focus { get; set; }

        [JsonPropertyName("original")]
        public Original? Original { get; set; }

        [JsonPropertyName("small")]
        public Small? Small { get; set; }
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

    public class Original
    {
        [JsonPropertyName("width")]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }

        [JsonPropertyName("frame_rate")]
        public string? FrameRate { get; set; }

        [JsonPropertyName("duration")]
        public double? Duration { get; set; }

        [JsonPropertyName("bitrate")]
        public int? Bitrate { get; set; }

        [JsonPropertyName("size")]
        public string? Size { get; set; }

        [JsonPropertyName("aspect")]
        public double? Aspect { get; set; }
    }

    public class Reblog
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("in_reply_to_id")]
        public string? InReplyToId { get; set; }

        [JsonPropertyName("in_reply_to_account_id")]
        public string? InReplyToAccountId { get; set; }

        [JsonPropertyName("sensitive")]
        public bool? Sensitive { get; set; }

        [JsonPropertyName("spoiler_text")]
        public string? SpoilerText { get; set; }

        [JsonPropertyName("visibility")]
        public string? Visibility { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("uri")]
        public string? Uri { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("replies_count")]
        public int? RepliesCount { get; set; }

        [JsonPropertyName("reblogs_count")]
        public int? ReblogsCount { get; set; }

        [JsonPropertyName("favourites_count")]
        public int? FavouritesCount { get; set; }

        [JsonPropertyName("edited_at")]
        public DateTime? EditedAt { get; set; }

        [JsonPropertyName("favourited")]
        public bool? Favourited { get; set; }

        [JsonPropertyName("reblogged")]
        public bool? Reblogged { get; set; }

        [JsonPropertyName("muted")]
        public bool? Muted { get; set; }

        [JsonPropertyName("bookmarked")]
        public bool? Bookmarked { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("filtered")]
        public List<object>? Filtered { get; set; }

        [JsonPropertyName("reblog")]
        public object? Rebloggy { get; set; }

        [JsonPropertyName("account")]
        public Account? Account { get; set; }

        [JsonPropertyName("media_attachments")]
        public List<MediaAttachment>? MediaAttachments { get; set; }

        [JsonPropertyName("mentions")]
        public List<Mention>? Mentions { get; set; }

        [JsonPropertyName("tags")]
        public List<Tag>? Tags { get; set; }

        [JsonPropertyName("emojis")]
        public List<object>? Emojis { get; set; }

        [JsonPropertyName("card")]
        public Card? Card { get; set; }

        [JsonPropertyName("poll")]
        public Poll? Poll { get; set; }
    }

    public class Toot
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        public int InternalID
        {
            get { return internalID; }
            set { internalID = value; }
        }
        private int internalID = 0;

        public List<string> ViewSource
        {
            get { return viewSource; }
            set { viewSource = value; }
        }
        private List<string> viewSource = new List<string>();

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("in_reply_to_id")]
        public string? InReplyToId { get; set; }

        [JsonPropertyName("in_reply_to_account_id")]
        public string? InReplyToAccountId { get; set; }

        [JsonPropertyName("sensitive")]
        public bool? Sensitive { get; set; }

        [JsonPropertyName("spoiler_text")]
        public string? SpoilerText { get; set; }

        [JsonPropertyName("visibility")]
        public string? Visibility { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("uri")]
        public string? Uri { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("replies_count")]
        public int? RepliesCount { get; set; }

        [JsonPropertyName("reblogs_count")]
        public int? ReblogsCount { get; set; }

        [JsonPropertyName("favourites_count")]
        public int? FavouritesCount { get; set; }

        [JsonPropertyName("edited_at")]
        public object? EditedAt { get; set; }

        [JsonPropertyName("favourited")]
        public bool? Favourited { get; set; }

        [JsonPropertyName("reblogged")]
        public bool? Reblogged { get; set; }

        [JsonPropertyName("muted")]
        public bool? Muted { get; set; }

        [JsonPropertyName("bookmarked")]
        public bool? Bookmarked { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("filtered")]
        public List<object>? Filtered { get; set; }

        [JsonPropertyName("reblog")]
        public Reblog? Reblog { get; set; }

        [JsonPropertyName("account")]
        public Account? Account { get; set; }

        [JsonPropertyName("media_attachments")]
        public List<MediaAttachment>? MediaAttachments { get; set; }

        [JsonPropertyName("mentions")]
        public List<object>? Mentions { get; set; }

        [JsonPropertyName("tags")]
        public List<Tag>? Tags { get; set; }

        [JsonPropertyName("emojis")]
        public List<object>? Emojis { get; set; }

        [JsonPropertyName("card")]
        public Card? Card { get; set; }

        [JsonPropertyName("poll")]
        public Poll? Poll { get; set; }

        [JsonPropertyName("pinned")]
        public bool? Pinned { get; set; }

        [JsonPropertyName("application")]
        public Application? Application { get; set; }
    }

    public class Small
    {
        [JsonPropertyName("width")]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }

        [JsonPropertyName("size")]
        public string? Size { get; set; }

        [JsonPropertyName("aspect")]
        public double? Aspect { get; set; }
    }

    public class Tag
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}