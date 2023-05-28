namespace TootSharp
{
    internal class Program
    {
        private const string Redirect = "urn:ietf:wg:oauth:2.0:oob";
        private const string Scopes = "read write follow";
        private const string AppName = "TootSharp";
        private const string Website = "https://github.com/jfabry-noc/TootSharp";

        static async Task Main(string[] args)
        {
            var io = new IOController();
            Printer.PrintGreeting();

            var configController = new ConfigController();

            if(configController.Instance is null || configController.AuthCode is null ||
                configController.ClientId is null || configController.ClientSecret is null
                || configController.AccessToken is null)
            {
                var instance = io.AskForInstance();
                var appRegResponse = await MastoClient.CreateApplication(instance, AppName, Redirect, Scopes, Website);

                configController.ParseAppRegistration(appRegResponse);

                if(configController.ClientId is null || configController.ClientSecret is null)
                {
                    Console.WriteLine("Client ID or Client Secret is null. Exiting.");
                    Environment.Exit(1);
                }

                var authCode = io.GetAuthCode(instance, configController.ClientId);

                var tokenResponse = await MastoClient.GetAccessToken(
                    configController.ClientId,
                    configController.ClientSecret,
                    authCode,
                    Redirect,
                    Scopes,
                    instance);

                configController.ParseAccessTokenResponse(tokenResponse);
                configController.WriteConfig(instance, authCode);
            }

            if(configController.Instance is null || configController.AccessToken is null)
            {
                Console.WriteLine("Config is invalid. Exiting.");
                Environment.Exit(1);
            }

            var client = new MastoClient(configController.Instance, configController.AccessToken);

            io.MainLoop(client);
        }
    }
}
