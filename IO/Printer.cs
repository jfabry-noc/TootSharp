using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace TootSharp
{
    internal static class Printer
    {
        internal static void PrintToot(Toot toot)
        {
            // Process user data and time.
            var userLine = "\n--== ";
            if(toot.Account is not null && toot.Account.DisplayName is not null)
            {
                userLine += $"{toot.Account.DisplayName}";
            }
            if(toot.Account is not null && toot.Account.Acct is not null)
            {
                userLine += $" (@{toot.Account.Acct})";
            }
            if(toot.CreatedAt is not null)
            {
                userLine += $" at {toot.CreatedAt}";
            }
            userLine += " ==--";

            // Process reblog data.
            var reblogUserLine = " OP:";
            var reblogContentLine = "";
            var reblogMetaLine = $"  ID: {toot.InternalID}";
            if(toot.Reblog is not null)
            {
                if(toot.Reblog.Account is not null && toot.Reblog.Account.DisplayName is not null)
                {
                    reblogUserLine += $"{toot.Reblog.Account.DisplayName}";
                }
                if(toot.Reblog.Account is not null && toot.Reblog.Account.Acct is not null)
                {
                    reblogUserLine += $" ({toot.Reblog.Account.Acct})";
                }
                if(toot.Reblog.CreatedAt is not null)
                {
                    reblogUserLine += $" at {toot.Reblog.CreatedAt}";
                }

                if(toot.Reblog.SpoilerText is not null && toot.Reblog.SpoilerText != "")
                {
                    reblogContentLine = $"-= CW: {toot.Reblog.SpoilerText} =-\n";
                }
                if(toot.Reblog.Content is not null)
                {
                    reblogContentLine += Printer.ProcessTootContent(toot.Reblog.Content);
                }

                if(toot.Reblog.MediaAttachments is not null)
                {
                    foreach(var attachment in toot.Reblog.MediaAttachments)
                    {
                        if(attachment.Type is not null && attachment.Url is not null)
                        {
                            reblogContentLine += $"\n{attachment.Type}: {attachment.Url}";
                        }
                        if(attachment.Description is not null)
                        {
                            reblogContentLine += $"\nAlt Text: {attachment.Description}";
                        }
                    }
                }

                if(toot.Reblog.RepliesCount is not null)
                {
                    reblogMetaLine += $" | Replies: {toot.Reblog.RepliesCount}";
                }
                else
                {
                    reblogMetaLine += " | Replies: 0";
                }
                if(toot.Reblog.ReblogsCount is not null)
                {
                    reblogMetaLine += $" | Reblogs: {toot.Reblog.ReblogsCount}";
                }
                else
                {
                    reblogMetaLine += " | Reblogs: 0";
                }
                if(toot.Reblog.FavouritesCount is not null)
                {
                    reblogMetaLine += $" | Favs: {toot.Reblog.FavouritesCount}";
                }
                else
                {
                    reblogMetaLine += " | Favs: 0";
                }
            }

            // Process content.
            var contentLine = "";
            if(toot.SpoilerText is not null && toot.SpoilerText != "")
            {
                contentLine = $"-= CW: {toot.SpoilerText} =-\n";
            }
            if(toot.Content is not null)
            {
                contentLine += Printer.ProcessTootContent(toot.Content);
            }
            if(toot.MediaAttachments is not null)
            {
                foreach(var attachment in toot.MediaAttachments)
                {
                    if(attachment.Type is not null && attachment.Url is not null)
                    {
                        contentLine += $"\n{attachment.Type}: {attachment.Url}";
                    }
                    if(attachment.Description is not null)
                    {
                        contentLine += $"\nAlt Text: {attachment.Description}";
                    }
                }
            }

            // Process metadata.
            var metaLine = $"  ID: {toot.InternalID}";
            if(toot.RepliesCount is not null)
            {
                metaLine += $" | Replies: {toot.RepliesCount}";
            }
            else
            {
                metaLine += " | Replies: 0";
            }
            if(toot.ReblogsCount is not null)
            {
                metaLine += $" | Reblogs: {toot.ReblogsCount}";
            }
            else
            {
                metaLine += " | Reblogs: 0";
            }
            if(toot.FavouritesCount is not null)
            {
                metaLine += $" | Favs: {toot.FavouritesCount}";
            }
            else
            {
                metaLine += " | Favs: 0";
            }
            metaLine += "\n";

            Console.WriteLine(userLine);
            if(toot.Reblog is not null)
            {
                Console.WriteLine(reblogUserLine);
                if(reblogContentLine != "")
                {
                    Console.WriteLine(reblogContentLine);
                }
                Console.WriteLine(reblogMetaLine);
            }
            else{
                if(contentLine != "")
                {
                    Console.WriteLine(contentLine);
                }
                Console.WriteLine(metaLine);
            }
        }

        private static string ProcessTootContent(string content)
        {
            // https://www.nuget.org/packages/AngleSharp
            content = content.Replace("<br>", "\n    ");

            string result = "";
            IConfiguration config = Configuration.Default;
            IBrowsingContext context = BrowsingContext.New(config);
            var parser = new HtmlParser();
            var parsed = parser.ParseDocument(content);

            var resp = Task.Run(async() => await context
                .OpenAsync(req => req.Content(content)));
            resp.Wait();
            var doc = resp.Result;

            var cleanedLinks = new List<string>();
            var mentions = doc.QuerySelectorAll("span.h-card");
            var rawLinks = doc.QuerySelectorAll("a");

            foreach(var instance in rawLinks)
            {
                if(instance.ClassName is not null && instance.ClassName.Contains("hashtag"))
                {
                    cleanedLinks.Add($"{instance.TextContent} -> {instance.GetAttribute("href")}");
                    var newText = doc.CreateTextNode(instance.TextContent);
                    instance.ReplaceWith(newText);
                }
                else
                {
                    if(instance.ClassName is null)
                    {
                        var newText = doc.CreateTextNode(instance.TextContent);
                        instance.ReplaceWith(newText);
                    }
                }
            }

            foreach(var mention in mentions)
            {
                var link = mention.QuerySelector("a");
                if(link is not null)
                {
                    cleanedLinks.Add($"{link.TextContent} -> {link.GetAttribute("href")}");
                    var newText = doc.CreateTextNode(link.TextContent);
                    mention.ReplaceWith(newText);
                }
            }

            var paragraphs = doc.QuerySelectorAll("p");
            var counter = 0;
            foreach(var paragraph in paragraphs)
            {
                counter++;
                if(counter == paragraphs.Length)
                {
                    result += "    " + paragraph.InnerHtml + "\n";
                }
                else
                {
                    result += "    " + paragraph.InnerHtml + "\n\n";
                }
            }

            foreach(var link in cleanedLinks)
            {
                result += "  " + link + "\n";

            }

            return result;
        }

        internal static void PrintGreeting()
        {
            Console.WriteLine(@"___________            __   _________.__                         ");
            Console.WriteLine(@"\__    ___/___   _____/  |_/   _____/|  |__ _____ _____________  ");
            Console.WriteLine(@"  |    | /  _ \ /  _ \   __\_____  \ |  |  \\__  \\_  __ \____ \ ");
            Console.WriteLine(@"  |    |(  <_> |  <_> )  | /        \|   Y  \/ __ \|  | \/  |_> >");
            Console.WriteLine(@"  |____| \____/ \____/|__|/_______  /|___|  (____  /__|  |   __/ ");
            Console.WriteLine(@"                                  \/      \/     \/      |__|    ");
        }

        internal static void PrintHelp()
        {
            Console.WriteLine("Timeline Commands:");
            Console.WriteLine("  home:             Show home timeline");
            Console.WriteLine("  local:            Show local timeline");
            Console.WriteLine("  federated:        Show federated timeline");
            Console.WriteLine("  user {webfinger}: Show my timeline");
            Console.WriteLine("\nToot Commands:");
            Console.WriteLine("  toot:            Post a toot");
            Console.WriteLine("  cw:              Post a toot with a content warning");
            Console.WriteLine("  delete {id}:     Delete a toot");
            Console.WriteLine("  reply {id}:      Reply to a toot");
            Console.WriteLine("  fav {id}:        Favorite a toot");
            Console.WriteLine("  unfav {id}:      Unfavorite a toot");
            Console.WriteLine("  boost {id}:      Boost a toot");
            Console.WriteLine("  unboost {id}     Unboost a toot");
            Console.WriteLine("  bookmark {id}:   Bookmark a toot");
            Console.WriteLine("  unbookmark {id}: Unbookmark a toot");
            Console.WriteLine("\nUser Commands:");
            Console.WriteLine("  follow:          Follow a user");
            Console.WriteLine("  unfollow:        Unfollow a user");
            Console.WriteLine("  bio {webfinger}: Show a user's bio");
            Console.WriteLine("\nOther Commands:");
            Console.WriteLine("  note: Show notifications");
            Console.WriteLine("  help: Print this help message");
            Console.WriteLine("  quit: Quit");
        }

        internal static void PrintUser(User user)
        {
            string userOutput = "";
            if(user.DisplayName is not null && user.Acct is not null)
            {
                userOutput += $"--== {user.DisplayName}: @{user.Acct} ==--\n";
            }

            if(user.Url is not null)
            {
                userOutput += $"URL: {user.Url}\n";
            }
            if(user.StatusesCount is not null)
            {
                userOutput += $"Toots: {user.StatusesCount}\n";
            }
            if(user.FollowersCount is not null)
            {
                userOutput += $"Followers: {user.FollowersCount}\n";
            }
            if(user.FollowingCount is not null)
            {
                userOutput += $"Following: {user.FollowingCount}\n";
            }
            if(user.LastStatusAt is not null)
            {
                userOutput += $"Last Toot: {user.LastStatusAt}\n";
            }
            if(user.Note is not null)
            {
                userOutput += $"Bio:\n{Printer.ProcessTootContent(user.Note)}\n";
            }

            if(user.Fields is not null && user.Fields.Count > 0)
            {
                userOutput += "Links:\n";
                foreach(var field in user.Fields)
                {
                    if(field.Value is not null)
                    {
                        userOutput += $" -> {field.Name}: {Printer.ProcessBioLink(field.Value)}\n";
                    }
                }
            }

            Console.WriteLine(userOutput);
        }

        internal static string? ProcessBioLink(string content)
        {
            IConfiguration config = Configuration.Default;
            IBrowsingContext context = BrowsingContext.New(config);
            var resp = Task.Run(async() => await context
                .OpenAsync(req => req.Content(content)));
            resp.Wait();
            var doc = resp.Result;
            var rawLinks = doc.QuerySelectorAll("a");
            foreach(var instance in rawLinks)
            {
                return instance.GetAttribute("href");
            }
            return null;
        }
    }
}