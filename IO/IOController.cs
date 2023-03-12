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
            var userLine = "\n";
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

            // Process reblog data.
            var reblogUserLine = " OP:";
            var reblogContentLine = "";
            var reblogMetaLine = $"ID: {toot.InternalID}";
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
                    reblogContentLine += toot.Reblog.Content;
                }

                if(toot.Reblog.MediaAttachments is not null)
                {
                    foreach(var attachment in toot.Reblog.MediaAttachments)
                    {
                        if(attachment.Type is not null && attachment.Url is not null)
                        {
                            reblogContentLine += $"\n{attachment.Type}: {attachment.Url}";
                        }
                    }
                }

                if(toot.Reblog.RepliesCount is not null)
                {
                    reblogMetaLine += $"\nReplies: {toot.Reblog.RepliesCount}";
                }
                else
                {
                    reblogMetaLine += "\nReplies: 0";
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
                contentLine += toot.Content;
            }
            if(toot.MediaAttachments is not null)
            {
                foreach(var attachment in toot.MediaAttachments)
                {
                    if(attachment.Type is not null && attachment.Url is not null)
                    {
                        contentLine += $"\n{attachment.Type}: {attachment.Url}";
                    }
                }
            }

            // Process metadata.
            var metaLine = $"ID: {toot.InternalID}";
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

        internal void PrintToots(List<Toot> toots, string? source)
        {
            var currentList = new List<Toot>();
            //toots = toots.OrderByDescending(t => t.CreatedAt).ToList();
            toots = toots.OrderBy(t => t.CreatedAt).ToList();
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

            foreach(var singleToot in currentList)
            {
                this.PrintToot(singleToot);
            }
        }

        internal void PrintTimeline(MastoClient client, string timeline)
        {
            var timelineRoute = $"timelines/{timeline}";
            var resp = Task.Run(async() => await client.Call(timelineRoute, HttpMethod.Get));

            var processed = client.ProcessResults<Toot>(resp);
            if(processed == null)
            {
                Console.WriteLine("No toots found.");
                return;
            }

            this.ManageTootList(timeline, processed, null);
            this.PrintToots(this._toots, timeline);
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
                switch(command.ToLower())
                {
                    case "quit":
                        Console.WriteLine("Quitting.");
                        break;
                    case "help":
                        this.PrintHelp();
                        break;
                    case "local":
                        Console.WriteLine("Printing local timeline.");
                        break;
                    case "federated":
                        Console.WriteLine("Printing federated timeline.");
                        break;
                    case "me":
                        Console.WriteLine("Printing my timeline.");
                        break;
                    case "home":
                        this.PrintTimeline(client, "home");
                        break;
                    case "toot":
                        this.PostToot(client);
                        break;
                    case "delete":
                        Console.WriteLine("Deleting a toot.");
                        break;
                    case "note":
                        Console.WriteLine("Printing notifications.");
                        break;
                    case "reply":
                        Console.WriteLine("Reply to a toot.");
                        break;
                    case "fav":
                        Console.WriteLine("Favorite a toot.");
                        break;
                    case "unfav":
                        Console.WriteLine("Unfavorite a toot.");
                        break;
                    case "boost":
                        Console.WriteLine("Boost a toot.");
                        break;
                    case "bookmark":
                        Console.WriteLine("Bookmark a toot.");
                        break;
                    case "unbookmark":
                        Console.WriteLine("Unbookmark a toot.");
                        break;
                    case "follow":
                        Console.WriteLine("Follow a user.");
                        break;
                    case "unfollow":
                        Console.WriteLine("Unfollow a user.");
                        break;
                    default:
                        Console.WriteLine("Unknown command.");
                        break;
                }
            } while (command.ToLower() != "quit");
        }
    }
}