namespace TootSharp
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var io = new IOController();
            io.PrintGreeting();

            var configController = new ConfigController();

            if(configController.Instance is null || configController.AuthCode is null)
            {
                var instance = io.AskForInstance();
                var authCode = io.GetAuthCode(instance, configController.ClientId);
                configController.WriteConfig(instance, authCode);
            }

        }
    }

}
