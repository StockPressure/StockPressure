using System.Configuration;
using System.Reflection;
using System.Text;

namespace StockPressure
{
    public static class AppConfig
    {
        public static bool CheckForToday { get { return bool.Parse(ConfigurationManager.AppSettings["checkForToday"]); } }

        private static string _apiKey = ConfigurationManager.AppSettings["apiKey"];
        public static string ApiKey
        {
            get { return _apiKey; }
            set
            {
                _apiKey = value;
                Configuration config = ConfigurationManager.OpenExeConfiguration(Assembly.GetEntryAssembly().Location);
                config.AppSettings.Settings.Add("ApiKey", value);
                config.Save();
            }
        }

        private static string _apiHost = ConfigurationManager.AppSettings["apiHost"];
        public static string ApiHost
        {
            get { return _apiHost; }
            set
            {
                _apiHost = value;
                Configuration config = ConfigurationManager.OpenExeConfiguration(Assembly.GetEntryAssembly().Location);
                config.AppSettings.Settings.Add("ApiHost", value);
                config.Save();
            }
        }

    }
}
