using System.Configuration;
using System.Net;
using System.Net.Http;

namespace WeatherBot
{
    public class Proxy
    {
        public HttpClientHandler ProxyHandler()
        {
            string userID = ConfigurationManager.AppSettings["UserID"];
            string password = ConfigurationManager.AppSettings["Password"];
            var defaultProxy = WebRequest.DefaultWebProxy;
            defaultProxy.Credentials = new NetworkCredential(userID, password);
            var handler = new HttpClientHandler { Proxy = defaultProxy };
            return handler;
        }
    }
}