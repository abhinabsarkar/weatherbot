using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using System.Configuration;

namespace WeatherBot
{
    public class WeatherEntityLUIS
    {
        /// <summary>
        /// This method will return Entity & Intent in JSON format 
        /// (deserialized to .net class object) from LUIS
        /// </summary>
        public static async Task<WeatherLUISModel> GetEntityFromLUIS(string queryWeatherDetails)
        {
            queryWeatherDetails = Uri.EscapeDataString(queryWeatherDetails);
            WeatherLUISModel weatherModelLUISData = new WeatherLUISModel();
            var telemetry = new TelemetryClient();

            //Deal with the corporate proxy by creating a proxy handler
            Proxy proxy = new Proxy();
            HttpClientHandler handler = new HttpClientHandler();
            handler = proxy.ProxyHandler();
            using (HttpClient httpClient = new HttpClient(handler))
            {
                try
                {
                    string LUISURI = ConfigurationManager.AppSettings["LUISURI"];
                    //Invoke the LUIS application and fetch the intents and entities
                    string requestUri = LUISURI + "&q=" +  queryWeatherDetails;

                    //Track LUIS response time
                    DateTime startTime = DateTime.UtcNow;   
                    //Invoke call to LUIS                 
                    HttpResponseMessage msg = await httpClient.GetAsync(requestUri);
                    //Capture elapsed time
                    TimeSpan elapsedTime = DateTime.UtcNow - startTime;
                    //Send Dependency telemetry
                    telemetry.TrackDependency("LUIS", "GetEntityFromLUIS", startTime, elapsedTime, true);
                    if (msg.IsSuccessStatusCode)
                    {
                        var jsonDataResponse = await msg.Content.ReadAsStringAsync();
                        weatherModelLUISData = JsonConvert.DeserializeObject<WeatherLUISModel>(jsonDataResponse);
                    }
                    return weatherModelLUISData;
                }
                catch (Exception ex)
                {                    
                    telemetry.TrackException(ex.InnerException); //Sends stack traces
                    throw ex;
                }
            }

        }
    }
}