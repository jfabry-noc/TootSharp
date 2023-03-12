using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TootSharp
{
    public class MastoClient
    {
        private string _token;
        public string Instance;
        public string ApiPath = "/api/v1/";


        public HttpClient Client { get; set; }

        public MastoClient(string instance, string token)
        {
            this.Instance = instance;
            this._token = token;
            this.Client = new HttpClient();
            this.Client.DefaultRequestHeaders.Clear();
            this.Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            this.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this._token);
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

        public async Task<string?> Call(
            string path,
            HttpMethod? method,
            Dictionary<string, string>? queryParams = null,
            string? dto = null
        )
        {
            var currentUrl = $"https://{this.Instance}{this.ApiPath}{path}";

            if(queryParams is not null)
            {
                currentUrl = this.AddQueryParams(currentUrl, queryParams);
            }
            Console.WriteLine($"Calling {currentUrl}");

            StringContent? body = this.ParseDto(dto);

            if(body is not null && method is null)
            {
                method = HttpMethod.Post;
            }

            HttpResponseMessage response;
            if(method == HttpMethod.Get || method == null)
            {
                response = await this.Client.GetAsync(currentUrl);
            }
            else if(method == HttpMethod.Post)
            {
                response = await this.Client.PostAsync(currentUrl, body);
            }
            else if(method == HttpMethod.Delete)
            {
                response = await this.Client.DeleteAsync(currentUrl);
            }
            else if(method == HttpMethod.Put)
            {
                response = await this.Client.PutAsync(currentUrl, body);
            }
            else
            {
                throw new Exception("Invalid HTTP method");
            }

            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine($"Connecting with {currentUrl} failed with response: {response.StatusCode}");
                return null;
            }
            var content = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine($"Response: {content}");
            // Maybe later add a header check on X-RateLimit-Limit,
            //X-RateLimit-LimitX-RateLimit-Remaining, and X-RateLimit-Reset.
            var headers = response.Headers;
            Console.WriteLine($"Headers: {headers}");

            return content;
        }

        internal StringContent? ParseDto(string? dto)
        {
            if(dto is not null)
            {
                return new StringContent(dto, Encoding.UTF8, "application/json");
            }
            return null;
        }

        private string AddQueryParams(string currentUrl, Dictionary<string, string> queryParams)
        {
            currentUrl = $"{currentUrl}?";
            foreach(KeyValuePair<string, string> parameter in queryParams)
            {
                currentUrl = $"{currentUrl}{parameter.Key}={parameter.Value}&";
            }
            return currentUrl.Remove(currentUrl.Length - 1);
        }

        public List<T>? ProcessResults<T>(Task<string?>? resp)
        {
            if(resp is null || resp.Result is null)
            {
                return null;
            }

            return JsonSerializer.Deserialize<List<T>>(resp.Result);
        }
    }
}