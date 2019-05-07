using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace WeatherBot
{
    public class WeatherResponse
    {
        public string FormattedResponse(WeatherAPIModel weatherAPIModel)
        {
            try
            {
                StringBuilder weatherResponse = new StringBuilder();
                if (weatherAPIModel.current_observation.relative_humidity == "" &&
                    weatherAPIModel.current_observation.weather == "")
                {
                    weatherResponse.Append("Sorry, the weather details for this location is not available.");
                }
                else
                {
                    weatherResponse.Append("The current temperature in " + weatherAPIModel.current_observation.display_location.city
                        + " is " + weatherAPIModel.current_observation.temperature_string);
                    weatherResponse.Append(" .Feels like " + weatherAPIModel.current_observation.feelslike_string);
                    weatherResponse.Append(" .Humidity is " + weatherAPIModel.current_observation.relative_humidity);
                    weatherResponse.Append(" .Today's forecast: " + weatherAPIModel.current_observation.weather);
                }
                return weatherResponse.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string FormattedResponse(WeatherAPIModelNew weatherAPIModelNew)
        {
            try
            {
                StringBuilder weatherResponse = new StringBuilder();
                weatherResponse.Append("The current temperature in " + weatherAPIModelNew.name + " (" + weatherAPIModelNew.sys.country
                    + ") is " + weatherAPIModelNew.main.temp + "°F.");                
                weatherResponse.Append(" Humidity is " + weatherAPIModelNew.main.humidity + "%.");
                weatherResponse.Append(" Today's forecast: " + weatherAPIModelNew.weather[0].description);
                
                var telemetry = new TelemetryClient();
                telemetry.TrackEvent("WeatherDetailed");

                return weatherResponse.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}