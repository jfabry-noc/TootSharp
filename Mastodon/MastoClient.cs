namespace TootSharp
{
    public class MastoClient
    {
        private string _token;
        public string Instance;

        public MastoClient(string instance, string token)
        {
            this.Instance = instance;
            this._token = token;
        }
    }
}