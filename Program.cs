namespace TootSharp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var io = new IOController();
            io.PrintGreeting();

            var configController = new ConfigController();

            if(configController.Instance is null || configController.AuthCode is null || configController.ClientId is null || configController.ClientSecret is null)
            {
                var instance = io.AskForInstance();
                var appRegResponse = await MastoClient.CreateApplication(instance, "TootSharp", "urn:ietf:wg:oauth:2.0:oob", "read write follow", "https://github.com/jfabry-noc");

                configController.ParseAppRegistration(appRegResponse);

                if(configController.ClientId is null || configController.ClientSecret is null)
                {
                    Console.WriteLine("Client ID or Client Secret is null. Exiting.");
                    Environment.Exit(1);
                }

                var authCode = io.GetAuthCode(instance, configController.ClientId);
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
