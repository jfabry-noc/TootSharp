using System.Text.Json;

namespace TootSharp
{
    public class ConfigController
    {
        internal string? ConfigPath { get; set; }

        internal string? ClientId {get; set; }

        internal string? ClientSecret {get; set; }

        internal string? AuthCode { get; set; }

        internal string? Instance { get; set; }

        internal string? AccessToken { get; set; }

        public ConfigController()
        {
            DetermineConfigPath();
            LoadConfig();
        }

        private void LoadConfig()
        {
            var config = GetConfigContent();
            if(config != null)
            {
                this.Instance = config.Instance;
                this.ClientId = config.ClientId;
                this.ClientSecret = config.ClientSecret;
                this.AuthCode = config.AuthCode;
                this.AccessToken = config.AccessToken;
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
                AuthCode = authCode,
                AccessToken = this.AccessToken,
                ClientId = this.ClientId,
                ClientSecret = this.ClientSecret
            };
            var configJson = JsonSerializer.Serialize(config);

            System.IO.FileInfo configFileInfo = new System.IO.FileInfo(ConfigPath);
            if(configFileInfo.Directory is not null)
            {
                configFileInfo.Directory.Create();
                System.IO.File.WriteAllText(ConfigPath, configJson);
            }
            else
            {
                throw new System.Exception("Could not determine config directory.");
            }

            LoadConfig();
        }

        public bool ValidateConfig()
        {
            if (Instance == null || AuthCode == null)
            {
                return false;
            }

            return true;
        }

        public void DeleteConfig()
        {
            if (ConfigPath == null)
            {
                DetermineConfigPath();
            }

            if(ConfigPath == null)
            {
                throw new System.Exception("Could not determine config path. OS support may not be implemented.");
            }

            System.IO.File.Delete(ConfigPath);
        }

        public void ParseAppRegistration(string? rawResp)
        {
            if(rawResp is null)
            {
                throw new System.Exception("Could not parse app registration response.");
            }

            var appReg = JsonSerializer.Deserialize<AppRegistration>(rawResp);
            if(appReg is null)
            {
                throw new System.Exception("Could not parse app registration response.");
            }
            this.ClientId = appReg.ClientId;
            this.ClientSecret = appReg.ClientSecret;

            if(this.ClientId is null || this.ClientSecret is null)
            {
                throw new System.Exception("Could not parse app registration response.");
            }
        }

        public void ParseAccessTokenResponse(string? rawResp)
        {
            if(rawResp is null)
            {
                throw new System.Exception("Could not parse access token response.");
            }
            var token = JsonSerializer.Deserialize<AccessToken>(rawResp);
            if(token is null)
            {
                throw new System.Exception("Could not parse access token response.");
            }

            this.AccessToken = token.AccessTokenValue;
            if(this.AccessToken is null)
            {
                throw new System.Exception("Could not parse access token response.");
            }
        }
    }
}