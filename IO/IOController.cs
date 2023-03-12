namespace TootSharp
{
    public class IOController
    {
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

        internal void PrintTimeline(MastoClient client, string timeline)
        {
            var timelineRoute = $"timelines/{timeline}";
            var resp = Task.Run(async() => await client.Call(timelineRoute, HttpMethod.Get));
            //var tootTask = await client.Call(timelineRoute, HttpMethod.Get);
            // Need to process the Task response prior to returning.
            var processed = client.ProcessResults<Toot>(resp);
            if(processed == null)
            {
                Console.WriteLine("No toots found.");
                return;
            }

            foreach (var toot in processed)
            {
                var managedToot = this.ProcessToot(toot);
                Console.WriteLine($"{managedToot.Username}: {managedToot.Content}");
            }
        }

        internal TootView ProcessToot(Toot toot)
        {
            string username = "";
            string content = "";
            if(toot.Account is not null && toot.Account.Username is not null)
            {
                username = toot.Account.Username;
            }

            if(toot.Content is not null)
            {
                content = toot.Content;
            }

            return new TootView(username, content);
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