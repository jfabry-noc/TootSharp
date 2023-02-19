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

        public async static Task<string?> CreateApplication(string instance, string name, string redirectUri, string scopes, string website)
        {
            var payload = new Dictionary<string, string>
            {
                {"client_name", name},
                {"redirect_uris", redirectUri},
                {"scopes", scopes},
                {"website", website}
            };

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
        }

        public static void GetToken(string clientId)
        {
            var payload = new Dictionary<string, string>
            {
                {"client_id", clientId},
            };
        }
    }
}