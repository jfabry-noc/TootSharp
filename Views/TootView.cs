namespace TootSharp
{
    public class TootView
    {
        public string Username
        {
            get { return username; }
            set { username = value; }
        }
        private string username = "";

        public string Content
        {
            get { return content; }
            set { content = value; }
        }
        private string content = "";

        public TootView(string username, string content)
        {
            this.Username = username;
            this.Content = content;
        }
    }
}