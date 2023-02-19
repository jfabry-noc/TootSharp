using System.Text.Json;

namespace TootSharp
{
    public class ConfigController
    {
        internal string? ConfigPath { get; set; }
        
        internal string ClientId = "pmRQFWQ8weQ4MyPcz9CIoZR5sXbe58JLbt9PvbC99Do";

        internal string? AuthCode { get; set; }

        internal string? Instance { get; set; }

        public ConfigController()
        {
            DetermineConfigPath();
            var config = GetConfigContent();
            if (config != null)
            {
                this.Instance = config.Instance;
                this.AuthCode = config.AuthCode;
            }
        }

        public UserConfig? GetConfigContent()
        {
            if (ConfigPath == null)
            {
                return null;
            }
            try {
                var configJson = System.IO.File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<UserConfig>(configJson);
            } catch (Exception e) {
                if(e is System.IO.FileNotFoundException || e is System.IO.DirectoryNotFoundException)
                {
                    return null;
                }
                throw;
            }
        }

        public void DetermineConfigPath()
        {
            var osVersion = System.Runtime.InteropServices.RuntimeInformation.OSDescription;
            if(osVersion.ToLower().StartsWith("linux") || osVersion.ToLower().StartsWith("mac"))
            {
                this.ConfigPath = "/home/" + System.Environment.UserName + "/.config/tootsharp/config.json";
            }
            else if(osVersion.ToLower().StartsWith("windows"))
            {
                this.ConfigPath = "C:\\Users\\" + System.Environment.UserName + "\\AppData\\Roaming\\tootsharp\\config.json";
            }
            else
            {
                this.ConfigPath = null;
            }
        }

        public void WriteConfig(string instance, string authCode)
        {
            if (ConfigPath == null)
            {
                DetermineConfigPath();
            }

            if(ConfigPath == null)
            {
                throw new System.Exception("Could not determine config path. OS support may not be implemented.");
            }

            var config = new UserConfig
            {
                Instance = instance,
                AuthCode = authCode
            };
            var configJson = JsonSerializer.Serialize(config);
            System.IO.File.WriteAllText(ConfigPath, configJson);
        }
    }
}