using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System.Diagnostics;
using System.Runtime.InteropServices;

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

        private void GetNotifications(MastoClient client)
        {
            var route = "notifications";
            var resp = Task.Run(async() => await client.Call(route, HttpMethod.Get));
            var processed = client.ProcessResults<Notification>(resp);
            // TODO: Figure out what other notification types need to be handled.
            if(processed is not null)
            {
                processed = processed.OrderBy(t => t.CreatedAt).ToList();
                foreach(var note in processed)
                {

                    if(note.Type == "mention" || note.Type == "poll")
                    {
                        this.ManageTootList("note", null, note.Status);
                        this.PrintToots(this._toots, "note", 1);
                    }
                    else if(note.Type == "follow")
                    {
                        Printer.PrintFollow(note);
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

        private bool SendToot(MastoClient client, Dictionary<string, string> form, string route = "statuses")
        {
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
                Printer.PrintToot(currentList[i]);
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
                Printer.PrintToot(toot);
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
                Printer.PrintToot(toot);
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
                Printer.PrintToot(toot);
            }
        }

        private Poll? ValidatePoll(Toot toot)
        {
            Poll? poll = null;
            if(toot.Poll is not null)
            {
                poll = toot.Poll;
            }
            else if(toot.Reblog is not null && toot.Reblog.Poll is not null)
            {
                poll = toot.Reblog.Poll;
            }
            if(poll is null)
            {
                Console.WriteLine($"No poll found associated with too: {toot.Id}");
                return poll;
            }
            if(poll.Expired is true)
            {
                Console.WriteLine("That poll has expired. No further voting permitted.");
                return null;
            }
            if(poll.Voted is true)
            {
                Console.WriteLine("You have already voted in this poll.");
                return null;
            }
            if(poll.Options is null || poll.Options.Count < 1)
            {
                Console.WriteLine("Poll has no options!");
                return null;
            }

            return poll;
        }

        private List<int>? ValidateVote(int ceiling, string? votes, bool multiple = false)
        {
            if(votes is null)
            {
                Console.WriteLine("Ignoring since no vote was supplied.");
                return null;
            }

            var voteItems = votes.Split(",");
            var results = new List<int>();
            foreach(var singleVote in voteItems)
            {
                bool parseSuccess = Int32.TryParse(singleVote.Trim(), out int voteIndex);
                if(!parseSuccess)
                {
                    Console.WriteLine($"Ignoring since {singleVote} is not a valid integer.");
                    continue;
                }
                if(voteIndex > ceiling)
                {
                    Console.WriteLine($"Ignoring since {voteIndex} is larger than the maximum of {ceiling}");
                    continue;
                }
                if(voteIndex < 1)
                {
                    Console.WriteLine("Ignoring since 1 is the minimum vote index value.");
                    continue;
                }
                results.Add(voteIndex-1);
            }

            if(multiple && results.Count > 1)
            {
                Console.WriteLine($"Specified {results.Count} items when only one is permitted. Using first option.");
                results = results.GetRange(0, 1);
            }
            return results;
        }

        internal void SendPoll(MastoClient client, List<int> votes, string id)
        {
            var parsedVotes = String.Join(",", votes);
            var endpoint = $"polls/{id}";
            var form = new Dictionary<string, string>
            {
                { "choices", parsedVotes },
            };
            var success = this.SendToot(client, form, endpoint);

            if(success)
            {
                Console.WriteLine("Voted successfully.");
            }
            else
            {
                Console.WriteLine("Voting failed!");
            }
        }

        internal void VotePoll(MastoClient client, string id)
        {
            var toot = this.GetMastoId(id);
            if(toot is null)
            {
                Console.WriteLine($"No toot found with ID: {id}");
                return;
            }

            Poll? poll = this.ValidatePoll(toot);
            if(poll is null || poll.Options is null || poll.Options.Count < 1 || poll.Id is null)
            {
                return;
            }

            Printer.PrintToot(toot);

            bool multiple = false;
            if(poll.Multiple is not null && poll.Multiple == true)
            {
                Console.WriteLine($"Enter comma separated votes between 1 and {poll.Options.Count}");
                multiple = true;
            }
            else
            {
                Console.WriteLine($"Enter vote between 1 and {poll.Options.Count}");
            }
            Console.Write("> ");
            var vote = Console.ReadLine();
            if(vote is null)
            {
                Console.WriteLine("Ignoring since no vote was specified.");
            }
            var voteIndices = this.ValidateVote(poll.Options.Count, vote, multiple);
            if(voteIndices is null)
            {
                Console.WriteLine("Aborting vote since no votes were specified.");
                return;
            }

            this.SendPoll(client, voteIndices, poll.Id);
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
                Printer.PrintToot(toot);
            }

            this._toots.Remove(toot);
        }

        internal void FollowUser(MastoClient client, string id, string webFinger, bool unfollow = false)
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
                    Printer.PrintUser(processed);
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

        internal void MainLoop(MastoClient client)
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
                    Printer.PrintHelp();
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
                else if(command.StartsWith("vote"))
                {
                    var id = this.ProcessCommandData(command);
                    if(id is not null)
                    {
                        this.VotePoll(client, id);
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
            } while (command.ToLower() != "quit" && command.ToLower() != "exit");
        }
    }
}