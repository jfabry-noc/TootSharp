namespace TootSharp
{
    public class MastoClient
    {
        private string _token;
        public string Instance;
        public string ApiPath = "/api/v1";


        public HttpClient Client { get; set; }

        public MastoClient(string instance, string token)
        {
            this.Instance = instance;
            this._token = token;
            this.Client = new HttpClient();
        }

        public async static Task<string?> CreateApplication(
            string instance,
            string name,
            string redirectUri,
            string scopes,
            string website
        )
        {
            var payload = new Dictionary<string, string>
            {
                {"client_name", name},
                {"redirect_uris", redirectUri},
                {"scopes", scopes},
                {"website", website}
            };
            var currentUrl= $"https://{instance}/api/v1/apps";

            return await MastoClient.StaticCall(instance, payload, currentUrl);
            /*
            var tempClient = new HttpClient();
            var response = await tempClient.PostAsync($"https://{instance}/api/v1/apps", new FormUrlEncodedContent(payload));

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return responseContent;
            }
            else
            {
                return null;
            }
            */
        }

        public async static Task<string?> GetAccessToken(
            string clientId,
            string clientSecret,
            string authCode,
            string redirectUri,
            string scopes,
            string instance
        )
        {
            var payload = new Dictionary<string, string>
            {
                {"client_id", clientId},
                {"client_secret", clientSecret},
                {"redirect_uri", redirectUri},
                {"code", authCode},
                {"grant_type", "authorization_code"},
                {"scope", scopes}
            };
            var currentUrl = $"https://{instance}/oauth/token";

            return await MastoClient.StaticCall(instance, payload, currentUrl);
        }

        public async static Task<string?> StaticCall(
            string instance,
            Dictionary<string, string> form,
            string url)
        {
            var tempClient = new HttpClient();
            var response = await tempClient.PostAsync(url, new FormUrlEncodedContent(form));

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return responseContent;
            }
            else
            {
                Console.WriteLine(response.StatusCode);
                return null;
            }
        }
    }
}