namespace blackbox.utils
{
    public static class ConfigurationProvider
    {
        static ConfigurationProvider() {
            if (System.Environment.GetEnvironmentVariable("API_URL") != null) {
                ServiceUrl = System.Environment.GetEnvironmentVariable("API_URL");
            } else {
                ServiceUrl = "http://localhost:9999";
            }
        }
        public static string ServiceUrl { get; set; }
    }
}