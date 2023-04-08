using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace TootSharp
{
    public class IOController
    {
        private List<Toot> _toots { get; set; }
        private const int _maxToots = 100;
        private const int _viewCount = 20;
        private int _nextTootID;

        public IOController()
        {
            this._toots = new List<Toot>();
            this._nextTootID = 1;
        }

        public void PrintGreeting()
        {
            Console.WriteLine(@"___________            __   _________.__                         ");
            Console.WriteLine(@"\__    ___/___   _____/  |_/   _____/|  |__ _____ _____________  ");
            Console.WriteLine(@"  |    | /  _ \ /  _ \   __\_____  \ |  |  \\__  \\_  __ \____ \ ");
            Console.WriteLine(@"  |    |(  <_> |  <_> )  | /        \|   Y  \/ __ \|  | \/  |_> >");
            Console.WriteLine(@"  |____| \____/ \____/|__|/_______  /|___|  (____  /__|  |   __/ ");
            Console.WriteLine(@"                                  \/      \/     \/      |__|    ");
        }

        public string AskForInstance()
        {
            Console.WriteLine("Please enter the instance you want to connect to:");
            Console.Write("> ");
            var instance = Console.ReadLine();
            if (instance == null)
            {
                Console.WriteLine("No instance entered. Exiting.");
                Environment.Exit(1);
            }

            if(instance.ToLower().StartsWith("https://"))
            {
                instance = instance.Substring(8);
            }

            return instance.ToLower();
        }

        public string GetAuthCode(string instance, string clientId)
        {
            var codeUrl = $"https://{instance}/oauth/authorize?client_id={clientId}&redirect_uri=urn:ietf:wg:oauth:2.0:oob&response_type=code&scope=read+write+follow";

            Console.WriteLine("Please open the following URL in your browser and enter the code you get:");
            Console.WriteLine(codeUrl);

            Console.Write("> ");
            var code = Console.ReadLine();
            if (code == null)
            {
                Console.WriteLine("No code entered. Exiting.");
                Environment.Exit(1);
            }
            return code;
        }

        private void ManageTootList(string source, List<Toot>? toots, Toot? singleToot)
        {
            if(toots is not null)
            {
                toots = toots.OrderBy(t => t.CreatedAt).ToList();
                foreach(var toot in toots)
                {
                    if(!this._toots.Where(t => t.Id == toot.Id).Any())
                    {
                        toot.InternalID = this._nextTootID;
                        this._nextTootID++;
                        toot.ViewSource.Add(source);
                        this._toots.Add(toot);
                    }
                }
            }

            if(singleToot is not null)
            {
                if(!this._toots.Where(t => t.Id == singleToot.Id).Any())
                {
                    singleToot.InternalID = this._nextTootID;
                    this._nextTootID++;
                    this._toots.Add(singleToot);
                }
            }

            if(this._toots.Count > IOController._maxToots)
            {
                Console.WriteLine("Trimming toot list...");
                this._toots.RemoveRange(0, this._toots.Count - IOController._maxToots);
            }

            this._toots = this._toots.OrderBy(t => t.CreatedAt).ToList();
        }

        internal void PrintHelp()
        {
            Console.WriteLine("Timeline Commands:");
            Console.WriteLine("  home:      Show home timeline");
            Console.WriteLine("  local:     Show local timeline");
            Console.WriteLine("  federated: Show federated timeline");
            Console.WriteLine("  me:        Show my timeline");
            Console.WriteLine("\nToot Commands:");
            Console.WriteLine("  toot:       Post a toot");
            Console.WriteLine("  delete:     Delete a toot");
            Console.WriteLine("  reply:      Reply to a toot");
            Console.WriteLine("  fav:        Favorite a toot");
            Console.WriteLine("  unfav:      Unfavorite a toot");
            Console.WriteLine("  boost:      Boost a toot");
            Console.WriteLine("  bookmark:   Bookmark a toot");
            Console.WriteLine("  unbookmark: Unbookmark a toot");
            Console.WriteLine("\nUser Commands:");
            Console.WriteLine("  follow:   Follow a user");
            Console.WriteLine("  unfollow: Unfollow a user");
            Console.WriteLine("\nOther Commands:");
            Console.WriteLine("  note: Show notifications");
            Console.WriteLine("  quit: Quit");
        }

        internal void PostToot(MastoClient client)
        {
            Console.WriteLine("Enter your toot (HTML permitted):");
            Console.Write("> ");
            var toot = Console.ReadLine();
            if (toot == null)
            {
                Console.WriteLine("No toot entered. Exiting.");
                return;
            }
            Console.WriteLine($"Would post: {toot}");
        }

        internal void PrintToot(Toot toot)
        {
            // Process user data and time.
            var userLine = "\n--== ";
            if(toot.Account is not null && toot.Account.DisplayName is not null)
            {
                userLine += $"{toot.Account.DisplayName}";
            }
            if(toot.Account is not null && toot.Account.Acct is not null)
            {
                userLine += $" ({toot.Account.Acct})";
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
                    reblogContentLine += this.ProcessTootContent(toot.Reblog.Content);
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
                contentLine += this.ProcessTootContent(toot.Content);
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

        internal string ProcessTootContent(string content)
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
                //Console.WriteLine(paragraph.InnerHtml + "\n");
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

        internal void PrintToots(List<Toot> toots, string? source)
        {
            var currentList = new List<Toot>();
            toots = toots.OrderByDescending(t => t.CreatedAt).ToList();
            foreach(var toot in toots)
            {
                if(source is not null)
                {
                    if(toot.ViewSource.Contains(source))
                    {
                        currentList.Add(toot);
                    }
                }
                else
                {
                    currentList.Add(toot);
                }
                if (currentList.Count == IOController._viewCount)
                {
                    break;
                }
            }

            for(int i = currentList.Count - 1; i >= 0; i--)
            {
                this.PrintToot(currentList[i]);
            }
        }

        internal void PrintTimeline(MastoClient client, string timeline)
        {
            string timelineRoute = "timelines/";
            Dictionary<string, string>? queryParams = null;
            if(timeline.ToLower() == "local")
            {
                queryParams = new Dictionary<string, string>()
                {
                    { "local", "true" }
                };
            }

            if(timeline.ToLower() == "home")
            {
                timelineRoute = $"{timelineRoute}{timeline}";
            }
            else {
                timelineRoute = $"{timelineRoute}public";
            }
            var resp = Task.Run(async() => await client.Call(timelineRoute, HttpMethod.Get, queryParams));

            var processed = client.ProcessResults<Toot>(resp);
            if(processed == null)
            {
                Console.WriteLine("No toots found.");
                return;
            }

            this.ManageTootList(timeline, processed, null);
            this.PrintToots(this._toots, timeline);
        }

        internal void FavoriteToot(MastoClient client, string id, bool unfav = false)
        {
            string endpoint;
            if(unfav)
            {
                endpoint = "unfavourite";
            }
            else
            {
                endpoint = "favourite";
            }
            int idConv;
            var success = int.TryParse(id, out idConv);
            if(!success)
            {
                Console.WriteLine($"Invalid ID: {id}");
                return;
            }

            var toot = this._toots.Find(t => t.InternalID == idConv);
            if(toot is null)
            {
                Console.WriteLine($"No toot found with ID: {idConv}");
                return;
            }

            string? tootId = toot.Id;
            if(toot.Reblog is not null)
            {
                tootId = toot.Reblog.Id;
            }

            var resp = Task.Run(async() => await client.Call($"statuses/{tootId}/{endpoint}", HttpMethod.Post));
            var processed = client.ProcessResult<Toot>(resp);
            if(processed is not null)
            {
                if(unfav)
                {
                    Console.WriteLine("Unfavorited: ");
                }
                else
                {
                    Console.WriteLine("Favorited: ");
                }
                this.PrintToot(toot);
            }
        }

        internal void BoostToot(MastoClient client, string id, bool unboost = false)
        {
            string endpoint;
            if(unboost)
            {
                endpoint = "unreblog";
            }
            else
            {
                endpoint = "reblog";
            }
            int idConv;
            var success = int.TryParse(id, out idConv);
            if(!success)
            {
                Console.WriteLine($"Invalid ID: {id}");
                return;
            }

            var toot = this._toots.Find(t => t.InternalID == idConv);
            if(toot is null)
            {
                Console.WriteLine($"No toot found with ID: {idConv}");
                return;
            }

            string? tootId = toot.Id;
            if(toot.Reblog is not null)
            {
                tootId = toot.Reblog.Id;
            }

            var resp = Task.Run(async() => await client.Call($"statuses/{tootId}/{endpoint}", HttpMethod.Post));
            var processed = client.ProcessResult<Toot>(resp);
            if(processed is not null)
            {
                if(unboost)
                {
                    Console.WriteLine("Unboosted: ");
                }
                else
                {
                    Console.WriteLine("Boosted: ");
                }
                this.PrintToot(toot);
            }
        }

        internal void BookmarkToot(MastoClient client, string id, bool unbookmark = false)
        {
            string endpoint;
            if(unbookmark)
            {
                endpoint = "unbookmark";
            }
            else
            {
                endpoint = "bookmark";
            }
            int idConv;
            var success = int.TryParse(id, out idConv);
            if(!success)
            {
                Console.WriteLine($"Invalid ID: {id}");
                return;
            }

            var toot = this._toots.Find(t => t.InternalID == idConv);
            if(toot is null)
            {
                Console.WriteLine($"No toot found with ID: {idConv}");
                return;
            }
            string? tootId = toot.Id;
            if(toot.Reblog is not null)
            {
                tootId = toot.Reblog.Id;
            }

            var resp = Task.Run(async() => await client.Call($"statuses/{tootId}/{endpoint}", HttpMethod.Post));
            var processed = client.ProcessResult<Toot>(resp);
            if(processed is not null)
            {
                if(unbookmark)
                {
                    Console.WriteLine("Unbookmarked: ");
                }
                else
                {
                    Console.WriteLine("Bookmarked: ");
                }
                this.PrintToot(toot);
            }
        }

        internal void DeleteToot(MastoClient client, string id)
        {
            int idConv;
            var success = int.TryParse(id, out idConv);

            var toot = this._toots.Find(t => t.InternalID == idConv);
            if(toot is null)
            {
                Console.WriteLine($"No toot found with ID: {idConv}");
                return;
            }

            var resp = Task.Run(async() => await client.Call($"statuses/{toot.Id}", HttpMethod.Delete));
            var processed = client.ProcessResult<Toot>(resp);
            if(processed is not null)
            {
                Console.WriteLine("Deleted: ");
                this.PrintToot(toot);
            }

            this._toots.Remove(toot);
        }

        private string? ProcessCommandData(string command)
        {
            string? result = null;
            if(command.Contains(" "))
            {
                result = command.Substring(command.IndexOf(" ") + 1);
            }
            return result;
        }

        public void MainLoop(MastoClient client)
        {
            string? command = "";
            do
            {
                Console.WriteLine("Enter a command key. 'help' for help.");
                Console.Write("> ");
                command = Console.ReadLine();
                if(command is null)
                {
                    Console.WriteLine("No command entered. Enter 'help' for command options.");
                    command = "";
                    continue;
                }
                else
                {
                    command = command.ToLower().Trim();
                }
                if(command == "quit")
                {
                    Console.WriteLine("Quitting.");
                }
                else if(command == "help")
                {
                    this.PrintHelp();
                }
                else if(command == "local")
                {
                    this.PrintTimeline(client, "local");
                }
                else if(command == "federated")
                {
                    this.PrintTimeline(client, "federated");
                }
                else if(command == "me")
                {
                    Console.WriteLine("Printing my timeline.");
                }
                else if(command == "home")
                {
                    this.PrintTimeline(client, "home");
                }
                else if(command == "toot")
                {
                    this.PostToot(client);
                }
                else if(command.StartsWith("delete"))
                {
                    var id = this.ProcessCommandData(command);
                    if(id is not null)
                    {
                        this.DeleteToot(client, id);
                    }
                    else
                    {
                        Console.WriteLine("Invalid ID.");
                    }
                }
                else if(command == "note")
                {
                    Console.WriteLine("Printing notifications.");
                }
                else if(command.StartsWith("reply"))
                {
                    Console.WriteLine("Replying to a toot.");
                }
                else if(command.StartsWith("fav"))
                {
                    var id = this.ProcessCommandData(command);
                    if(id is not null)
                    {
                        this.FavoriteToot(client, id);
                    }
                    else
                    {
                        Console.WriteLine("Invalid ID.");
                    }
                }
                else if(command.StartsWith("unfav"))
                {
                    var id = this.ProcessCommandData(command);
                    if(id is not null)
                    {
                        this.FavoriteToot(client, id, true);
                    }
                    else
                    {
                        Console.WriteLine("Invalid ID.");
                    }
                }
                else if(command.StartsWith("boost"))
                {
                    var id = this.ProcessCommandData(command);
                    if(id is not null)
                    {
                        this.BoostToot(client, id);
                    }
                    else
                    {
                        Console.WriteLine("Invalid ID.");
                    }
                }
                else if(command.StartsWith("unboost"))
                {
                    var id = this.ProcessCommandData(command);
                    if(id is not null)
                    {
                        this.BoostToot(client, id, true);
                    }
                    else
                    {
                        Console.WriteLine("Invalid ID.");
                    }
                }
                else if(command.StartsWith("bookmark"))
                {
                    var id = this.ProcessCommandData(command);
                    if(id is not null)
                    {
                        this.BookmarkToot(client, id);
                    }
                    else
                    {
                        Console.WriteLine("Invalid ID.");
                    }
                }
                else if(command.StartsWith("unbookmark"))
                {
                    var id = this.ProcessCommandData(command);
                    if(id is not null)
                    {
                        this.BookmarkToot(client, id, true);
                    }
                    else
                    {
                        Console.WriteLine("Invalid ID.");
                    }
                }
                else if(command.StartsWith("search"))
                {
                    Console.WriteLine("Search for a toot.");
                }
                else if(command.StartsWith("follow"))
                {
                    Console.WriteLine("Follow a user.");
                }
                else if(command.StartsWith("unfollow"))
                {
                    Console.WriteLine("Unfollow a user.");
                }
            } while (command.ToLower() != "quit");
        }
    }
}