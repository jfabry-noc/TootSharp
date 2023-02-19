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
    }
}