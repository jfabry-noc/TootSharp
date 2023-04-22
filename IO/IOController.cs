using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace TootSharp
{
    public class IOController
    {
        private List<Toot> _toots { get; set; }
        private const int _maxToots = 1000;
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
            if(toots is null && singleToot is null)
            {
                return;
            }
            if(toots is null && singleToot is not null)
            {
                toots = new List<Toot>(){ singleToot };
            }
            if(toots is not null && singleToot is not null)
            {
                toots.Add(singleToot);
            }
            if(toots is null)
            {
                return;
            }

            if(singleToot is not null)
            {
                if(toots is not null)
                {
                    toots.Add(singleToot);
                }
                else
                {
                    toots = new List<Toot>() { singleToot };
                }
            }
            toots = toots.OrderBy(t => t.CreatedAt).ToList();
            foreach(var toot in toots)
            {
                var matching = this._toots.Where(t => t.Id == toot.Id);
                if(!matching.Any())
                {
                    toot.InternalID = this._nextTootID;
                    this._nextTootID++;
                    toot.ViewSource.Add(source);
                    this._toots.Add(toot);
                }
                else
                {
                    foreach(var item in matching)
                    {
                        if(!item.ViewSource.Contains(source))
                        {
                            item.ViewSource.Add(source);
                        }
                    }
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

        private void GetNotifications(MastoClient client)
        {
            var route = "notifications";
            var resp = Task.Run(async() => await client.Call(route, HttpMethod.Get));
            var processed = client.ProcessResults<Notification>(resp);
            // TODO: Figure out how we want to print notificatins. May need to do conditionals
            // based around the Type property?
            if(processed is not null)
            {
                processed = processed.OrderBy(t => t.CreatedAt).ToList();
                foreach(var note in processed)
                {
                    //Console.WriteLine($"####### {note.Type}: {note.Account} - {note.Status} - {note.CreatedAt}");

                    if(note.Type == "mention")
                    {
                        Console.WriteLine($"####### {note.Type}: {note.Account} - {note.Status} - {note.CreatedAt}");
                        this.ManageTootList("note", null, note.Status);
                        this.PrintToots(this._toots, "note", 1);
                    }
                    else if(note.Type == "follow")
                    {
                        /*
                        if(note.Account is null)
                        {
                            Console.WriteLine("\n--++ New Follow From Unknown Account ++--");
                            continue;
                        }
                        Console.WriteLine($"\n--++ New Follow: {note.Account.DisplayName} - @{note.Account.Acct} ++--");
                        Console.WriteLine($"  Followed At: {note.CreatedAt}");
                        */
                    }
                    else if(note.Type == "poll")
                    {
                        //Console.WriteLine("\nPoll ended. I need to add poll support still.");
                    }
                }
            }
        }

        private string MakeTempFileName(Guid guid)
        {
            string filePath = "";
            if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                filePath = "/private/tmp/";
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                filePath = @"C:\Temp\";
            }
            else
            {
                filePath = "/tmp/";
            }

            return $"{filePath}toot-{guid}.txt";
        }

        private void RunEditor(string exePath, string filePath)
        {
            Process process = new Process();
            process.StartInfo.FileName = exePath;
            process.StartInfo.Arguments = filePath;
            process.StartInfo.UseShellExecute = true;
            process.Start();
            process.WaitForExit();
        }

        private string? ReadFileContent(string filePath)
        {
            if(File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            else
            {
                Console.WriteLine($"No file found at: {filePath}");
                return null;
            }
        }

        private void CleanUpFile(string filePath)
        {
            try
            {
                if(File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch(IOException exc)
            {
                Console.WriteLine($"Failed to delete '{filePath}' with error: {exc.Message}");
            }
        }

        private Dictionary<string, string> CreateContentContainer(string content, string? replyId = null, string? cw = null)
        {
            var form = new Dictionary<string, string>()
            {
                {"status", content}
            };

            if(replyId is not null)
            {
                form["in_reply_to_id"] = replyId;
            }
            if(cw is not null)
            {
                form["spoiler_text"] = cw;
            }

            return form;
        }

        private bool SendToot(MastoClient client, Dictionary<string, string> form)
        {
            var route = "statuses";
            var resp = Task.Run(async() => await client.Call(route, HttpMethod.Post, form: form));
            var content = resp.Result;
            return true;
        }

        private Toot? GetMastoId(string friendlyId)
        {
            int idConv;
            var success = int.TryParse(friendlyId, out idConv);

            var toot = this._toots.Find(t => t.InternalID == idConv);
            return toot;
        }

        private void PostCW(MastoClient client)
        {
            Console.WriteLine("Enter your CW text:");
            Console.Write("> ");
            var cwContent = Console.ReadLine();
            if(cwContent is null || cwContent == "")
            {
                Console.WriteLine("No CW given...");
                return;
            }

            this.PostToot(client, cw: cwContent.Trim());
        }

        internal void PostToot(MastoClient client, string? replyId = null, string? cw = null)
        {
            var editor = Environment.GetEnvironmentVariable("EDITOR");
            if(editor is null)
            {
                Console.WriteLine("No value set for $EDITOR. Please set the environment variable.");
                return;
            }

            var guid = Guid.NewGuid();
            var filePath = this.MakeTempFileName(guid);

            Console.WriteLine($"Opening: {filePath}...");
            Thread.Sleep(1000);
            this.RunEditor(editor, filePath);

            var content = this.ReadFileContent(filePath);
            if(content is null || content == "")
            {
                Console.WriteLine("Skipping toot since the content was empty");
                return;
            }

            string? mastoId = null;
            if(replyId is not null)
            {
                var toot = this.GetMastoId(replyId);
                if(toot is null)
                {
                    Console.WriteLine($"No toot found with ID: {replyId}");
                    return;
                }
                mastoId = toot.Id;
            }
            var form = this.CreateContentContainer(content, mastoId, cw);

            Console.WriteLine("Posting...");
            Thread.Sleep(500);

            var postSuccessful = this.SendToot(client, form);
            this.CleanUpFile(filePath);
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

        internal void PrintUser(User user)
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
                userOutput += $"Bio:\n{this.ProcessTootContent(user.Note)}\n";
            }

            if(user.Fields is not null && user.Fields.Count > 0)
            {
                userOutput += "Links:\n";
                foreach(var field in user.Fields)
                {
                    if(field.Value is not null)
                    {
                        userOutput += $" -> {field.Name}: {this.ProcessBioLink(field.Value)}\n";
                    }
                }
            }

            Console.WriteLine(userOutput);
        }

        internal string? ProcessBioLink(string content)
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

        internal void PrintToots(List<Toot> toots, string? source, int? limit = null)
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

                if(limit is not null && currentList.Count == limit)
                {
                    break;
                }
                if (currentList.Count == IOController._viewCount)
                {
                    break;
                }
            }

            if(currentList.Count == 0)
            {
                Console.WriteLine($"No toots found for source: {source}");
            }

            for(int i = currentList.Count - 1; i >= 0; i--)
            {
                this.PrintToot(currentList[i]);
            }
        }

        internal void PrintStandaloneToots(List<Toot> toots)
        {
            toots = toots.OrderBy(t => t.CreatedAt).ToList();
            foreach(var toot in toots)
            {
                this.PrintToot(toot);
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

            var toot = this.GetMastoId(id);
            if(toot is null)
            {
                Console.WriteLine($"No toot found with ID: {id}");
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

            var toot = this.GetMastoId(id);
            if(toot is null)
            {
                Console.WriteLine($"No toot found with ID: {id}");
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

            var toot = this.GetMastoId(id);
            if(toot is null)
            {
                Console.WriteLine($"No toot found with ID: {id}");
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
            var toot = this.GetMastoId(id);
            if(toot is null)
            {
                Console.WriteLine($"No toot found with ID: {id}");
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

        internal void FollowUser(MastoClient client, string id, string webFinger, bool unfollow= false)
        {
            string endpoint;
            if(unfollow)
            {
                endpoint = "unfollow";
            }
            else
            {
                endpoint = "follow";
            }
            long idConv;
            var success = long.TryParse(id, out idConv);
            if(!success)
            {
                Console.WriteLine($"Invalid ID: {id}");
                return;
            }

            var resp = Task.Run(async() => await client.Call($"accounts/{id}/{endpoint}", HttpMethod.Post));
            var processed = client.ProcessResult<Follow>(resp);
            if(processed is not null)
            {
                if(unfollow)
                {
                    Console.WriteLine($"Unfollowed: {webFinger}");
                }
                else
                {
                    Console.WriteLine($"Followed: {webFinger}");
                }
            }
        }

        internal void GetUserToots(MastoClient client, string id, string webFinger)
        {
            var path = $"accounts/{id}/statuses";
            long idConv;
            var success = long.TryParse(id, out idConv);
            if(!success)
            {
                Console.WriteLine($"Invalid ID: {id}");
                return;
            }

            var resp = Task.Run(async() => await client.Call(path, HttpMethod.Get));
            var processed = client.ProcessResults<Toot>(resp);
            if(processed is not null)
            {
                this.ManageTootList(webFinger, processed, null);
                this.PrintToots(this._toots, webFinger);
            }
        }

        internal void GetTootThread(MastoClient client, string id)
        {
            var toot = this.GetMastoId(id);
            if(toot is null)
            {
                Console.WriteLine($"No toot found with ID: {id}");
                return;
            }
            string? tootId = toot.Id;
            if(toot.Reblog is not null)
            {
                tootId = toot.Reblog.Id;
            }
            if(tootId is null)
            {
                Console.WriteLine($"Invalid ID: {tootId}");
                return;
            }
            var path = $"statuses/{tootId}/context";

            var resp = Task.Run(async() => await client.Call(path, HttpMethod.Get));
            var processed = client.ProcessResult<TootContext>(resp);
            if(processed is not null)
            {
                var threadCount = 1;
                if(processed.Ancestors is not null)
                {
                    this.ManageTootList(tootId, processed.Ancestors, null);
                    threadCount += processed.Ancestors.Count;
                }
                this.ManageTootList(tootId, null, toot);
                if(processed.Descendants is not null)
                {
                    this.ManageTootList(tootId, processed.Descendants, null);
                    threadCount += processed.Descendants.Count;
                }

                this.PrintToots(this._toots, tootId, threadCount);
            }
        }

        internal string? LookupUser(MastoClient client, string webFinger, bool print = false)
        {
            var queryParams = new Dictionary<string, string>()
            {
                {"acct", webFinger}
            };
            var resp = Task.Run(async() => await client.Call("accounts/lookup", HttpMethod.Get, queryParams));
            var processed = client.ProcessResult<User>(resp);
            if(processed is not null)
            {
                if(print)
                {
                    this.PrintUser(processed);
                }
                return processed.Id;
            }
            return null;
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
                Console.WriteLine("Enter a command. 'help' for help.");
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
                else if(command.StartsWith("user"))
                {
                    var webfinger = this.ProcessCommandData(command);
                    if(webfinger is not null)
                    {
                        var id = this.LookupUser(client, webfinger);
                        if(id is not null)
                        {
                            this.GetUserToots(client, id, webfinger);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid WebFinger address.");
                    }
                }
                else if(command == "home")
                {
                    this.PrintTimeline(client, "home");
                }
                else if(command == "toot")
                {
                    this.PostToot(client);
                }
                else if(command == "cw")
                {
                    this.PostCW(client);
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
                    this.GetNotifications(client);
                }
                else if(command.StartsWith("reply"))
                {
                    var id = this.ProcessCommandData(command);
                    if(id is not null)
                    {
                        this.PostToot(client, id);
                    }
                    else
                    {
                        Console.WriteLine("Invalid ID.");
                    }
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
                else if(command.StartsWith("thread"))
                {
                    var id = this.ProcessCommandData(command);
                    if(id is not null)
                    {
                        this.GetTootThread(client, id);
                    }
                    else
                    {
                        Console.WriteLine("Invalid ID.");
                    }
                }
                else if(command.StartsWith("follow"))
                {
                    var webfinger = this.ProcessCommandData(command);
                    if(webfinger is not null)
                    {
                        var id = this.LookupUser(client, webfinger);
                        if(id is not null)
                        {
                            this.FollowUser(client, id, webfinger);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid WebFinger address.");
                    }
                }
                else if(command.StartsWith("unfollow"))
                {
                    var webfinger = this.ProcessCommandData(command);
                    if(webfinger is not null)
                    {
                        var id = this.LookupUser(client, webfinger);
                        if(id is not null)
                        {
                            this.FollowUser(client, id, webfinger, true);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid WebFinger address.");
                    }
                }
                else if(command.StartsWith("bio"))
                {
                    var webFinger = this.ProcessCommandData(command);
                    if(webFinger is not null)
                    {
                        this.LookupUser(client, webFinger, true);
                    }
                    else
                    {
                        Console.WriteLine("Invalid WebFinger address.");
                    }
                }
            } while (command.ToLower() != "quit");
        }
    }
}