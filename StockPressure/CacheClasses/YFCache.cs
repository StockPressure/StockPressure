using Newtonsoft.Json;
using StockPressure.Models;
using System;
using System.IO;

namespace StockPressure.CacheClasses
{
    /// <summary>
    /// I can only hit this account SO MANY times before I get shut down for a month. As such, I want to
    /// cache as MUCH of this information as possible.
    /// </summary>
    public class YFCache
    {
        public static bool Has(string symbol)
        {
            CreateCacheDir();

            string fn = GetFileName(symbol);

            if (File.Exists(fn))
            {
                string data = File.ReadAllText(fn);
                YFCache cache = JsonConvert.DeserializeObject<YFCache>(data);
                if (cache.Expires < DateTime.Now)
                {
                    // It has expired. Delete it.
                    File.Delete(fn);
                    return false;
                }
                return true;
            }

            return false;
        }

        public static bool Has(string symbol, DateTime startingDate)
        {
            CreateCacheDir();

            string fn = GetFileName(symbol, startingDate);

            if (File.Exists(fn))
            {
                return true;
            }

            return false;
        }

        private static void CreateCacheDir()
        {
            if (!Directory.Exists(@"Cache\Yahoo"))
            {
                Directory.CreateDirectory(@"Cache\Yahoo");
            }
        }

        private static string GetFileName(string symbol)
        {
            return $@"Cache\Yahoo\{symbol}.txt";
        }

        private static string GetFileName(string symbol, DateTime startingDate)
        {
            return $@"Cache\Yahoo\Manual_{symbol}_{startingDate:yyyy-MM-dd}.txt";
        }

        public static YahooFinanceStatistics Load(string symbol)
        {
            string fn = GetFileName(symbol);

            string data = File.ReadAllText(fn);
            YFCache cache = JsonConvert.DeserializeObject<YFCache>(data);

            return cache.Statistics;
        }

        public static ManualStatistics Load(string symbol, DateTime startingDate)
        {
            string fn = GetFileName(symbol, startingDate);

            string data = File.ReadAllText(fn);
            ManualStatistics cache = JsonConvert.DeserializeObject<ManualStatistics>(data);

            return cache;
        }

        public DateTime Expires { get; set; }
        public YahooFinanceStatistics Statistics { get; set; }

        public static void Save(string symbol, YahooFinanceStatistics obj)
        {
            string fn = GetFileName(symbol);
            YFCache cache = new YFCache();

            cache.Expires = DateTime.Now.AddDays(7);
            cache.Statistics = obj;
            string json = JsonConvert.SerializeObject(cache);
            
            File.WriteAllText(fn, json);
        }

        public static void Save(string symbol, DateTime startingDate, ManualStatistics data)
        {
            string fn = GetFileName(symbol, startingDate);
            string json = JsonConvert.SerializeObject(data);

            File.WriteAllText(fn, json);
        }
    }
}