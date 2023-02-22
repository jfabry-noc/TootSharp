namespace TootSharp
{
    internal class Program
    {
        public const string Redirect = "urn:ietf:wg:oauth:2.0:oob";
        public const string Scopes = "read write follow";
        public const string AppName = "TootSharp";
        public const string Website = "https://github.com/jfabry-noc/TootSharp";

        static async Task Main(string[] args)
        {
            var io = new IOController();
            io.PrintGreeting();

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

            if(! configController.ValidateConfig())
            {
                Console.WriteLine("Config is invalid. Exiting.");
                Environment.Exit(1);
            }
        }
    }

}
