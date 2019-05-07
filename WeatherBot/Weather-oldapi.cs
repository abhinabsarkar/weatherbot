using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.ApplicationInsights;

namespace WeatherBot
{
    public class Weather
    {
        static string weatherURI = ConfigurationManager.AppSettings["WeatherURI"];
        static TelemetryClient telemetry = new TelemetryClient();

        public static async Task<Object[]> GetWeatherDetails(string location)
        {
            LocationAPIModel multipleLocations = new LocationAPIModel();
            WeatherAPIModel weatherAPIModel = new WeatherAPIModel();
            Object[] multiDataTypes = new Object[2];
            try
            { 
                if (location.Contains(" "))
                {
                    location.Replace(" ", "_");
                }
                string weatherURL = weatherURI + location + ".json";
                //Deal with the corporate proxy by creating a proxy handler
                Proxy proxy = new Proxy();
                HttpClientHandler handler = new HttpClientHandler();
                handler = proxy.ProxyHandler();
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    //Track Weather API response time
                    DateTime startTime = DateTime.UtcNow;
                    //Get the weather details
                    string weatherDetails = await httpClient.GetStringAsync(weatherURL);
                    //Capture elapsed time
                    TimeSpan elapsedTime = DateTime.UtcNow - startTime;
                    //Send Dependency telemetry
                    telemetry.TrackDependency("WeatherAPI", "GetWeatherDetails", startTime, elapsedTime, true);
                    weatherAPIModel = JsonConvert.DeserializeObject<WeatherAPIModel>(weatherDetails);
                    multipleLocations = JsonConvert.DeserializeObject<LocationAPIModel>(weatherDetails);
                    multiDataTypes[0] = weatherAPIModel;
                    multiDataTypes[1] = multipleLocations;
                    return multiDataTypes;
                }                
            }
            catch (Exception ex)
            {
                //var telemetry = new TelemetryClient();
                telemetry.TrackException(ex.InnerException); //Sends stack traces
                throw ex;
            }
        }

        public static async Task<WeatherAPIModel> GetWeatherDetailsUSA(string city, string state)
        {
            WeatherAPIModel weatherAPIModel = new WeatherAPIModel();
            try
            {
                if (city.Contains(" "))
                {
                    city.Replace(" ", "_");
                }
                string weatherURL = weatherURI + state + "/" + city + ".json";
                //Deal with the corporate proxy by creating a proxy handler
                Proxy proxy = new Proxy();
                HttpClientHandler handler = new HttpClientHandler();
                handler = proxy.ProxyHandler();
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    //Track Weather API response time
                    DateTime startTime = DateTime.UtcNow;
                    //Get the weather details
                    string weatherDetails = await httpClient.GetStringAsync(weatherURL);
                    //Capture elapsed time
                    TimeSpan elapsedTime = DateTime.UtcNow - startTime;
                    //Send Dependency telemetry
                    telemetry.TrackDependency("WeatherAPI", "GetWeatherDetailsUSA", startTime, elapsedTime, true);
                    weatherAPIModel = JsonConvert.DeserializeObject<WeatherAPIModel>(weatherDetails);
                    return JsonConvert.DeserializeObject<WeatherAPIModel>(weatherDetails); ;
                }
            }
            catch (Exception ex)
            {
                //var telemetry = new TelemetryClient();
                telemetry.TrackException(ex.InnerException); //Sends stack traces
                throw ex;
            }
        }

        public static async Task<WeatherAPIModel> GetWeatherDetailsOthers(string city, string country)
        {
            WeatherAPIModel weatherAPIModel = new WeatherAPIModel();
            try
            {
                if (city.Contains(" "))
                {
                    city.Replace(" ", "_");
                }
                string weatherUrl = weatherURI + country + "/" + city + ".json";
                //Deal with the corporate proxy by creating a proxy handler
                Proxy proxy = new Proxy();
                HttpClientHandler handler = new HttpClientHandler();
                handler = proxy.ProxyHandler();
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    //Track Weather API response time
                    DateTime startTime = DateTime.UtcNow;
                    //Get the weather details
                    //TODO: Add logic to handle mutliple cities in different Country
                    //Ask for temperature in Paris and Select Paris, YT, Canada (last option) in the response
                    string weatherDetails = await httpClient.GetStringAsync(weatherUrl);
                    //Capture elapsed time
                    TimeSpan elapsedTime = DateTime.UtcNow - startTime;
                    //Send Dependency telemetry
                    telemetry.TrackDependency("WeatherAPI", "GetWeatherDetailsOthers", startTime, elapsedTime, true);
                    weatherAPIModel = JsonConvert.DeserializeObject<WeatherAPIModel>(weatherDetails);
                    return JsonConvert.DeserializeObject<WeatherAPIModel>(weatherDetails); ;
                }
            }
            catch (Exception ex)
            {
                //var telemetry = new TelemetryClient();
                telemetry.TrackException(ex.InnerException); //Sends stack traces
                throw ex;
            }
        }
    }
}