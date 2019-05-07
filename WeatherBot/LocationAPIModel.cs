namespace WeatherBot
{

    public class LocationAPIModel
    {
        public LocationResponse response { get; set; }
    }

    public class LocationResponse
    {
        public string version { get; set; }
        public string termsofService { get; set; }
        public Features features { get; set; }
        public LocationResult[] results { get; set; }
    }

    public class LocationFeatures
    {
        public int conditions { get; set; }
    }

    public class LocationResult
    {
        public string name { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string country_iso3166 { get; set; }
        public string country_name { get; set; }
        public string zmw { get; set; }
        public string l { get; set; }
    }

}