using Newtonsoft.Json;
using StockPressure.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace StockPressure.CacheClasses
{
    /// <summary>
    /// I can only hit this account SO MANY times before I get shut down for a month. As such, I want to
    /// cache as MUCH of this information as possible.
    /// </summary>
    public class FinraCache
    {
        public static bool Has(string symbol, DateTime dt)
        {
            if (!Directory.Exists(@"Cache\FINRA"))
            {
                Directory.CreateDirectory(@"Cache\FINRA");
            }

            string fn = GetFileName(symbol, dt);

            if (File.Exists(fn))
            {
                string data = File.ReadAllText(fn);
                FinraCache cache = JsonConvert.DeserializeObject<FinraCache>(data);
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

        public static bool Has_Full(DateTime dt)
        {
            if (!Directory.Exists(@"Cache\FINRA_FILE"))
            {
                Directory.CreateDirectory(@"Cache\FINRA_FILE");
            }

            string fn = GetFileName_Full(dt);

            if (File.Exists(fn))
            {
                return true;
            }

            return false;
        }

        public static string Load_Full(DateTime dt)
        {
            string fn = GetFileName_Full(dt);
            string ret = File.ReadAllText(fn);

            return ret;
        }

        public static FinraData Load(string symbol, DateTime dt)
        {
            string fn = GetFileName(symbol,dt);

            string data = File.ReadAllText(fn);
            FinraCache cache = JsonConvert.DeserializeObject<FinraCache>(data);

            return cache.Data;
        }

        private static string GetFileName_Full(DateTime dt)
        {
            return $@"Cache\FINRA_FILE\{dt:yyyyMMdd}.txt";
        }

        private static string GetFileName(string symbol, DateTime dt)
        {
            return $@"Cache\FINRA\FINRA_{symbol}{dt:yyyyMMdd}.txt";
        }

        public DateTime Expires { get; set; }
        public FinraData Data { get; set; }

        public static void Save(string symbol, DateTime dt, FinraData obj)
        {
            string fn = GetFileName(symbol, dt);
            FinraCache cache = new FinraCache();

            cache.Expires = DateTime.Now.AddDays(7);
            cache.Data = obj;
            string json = JsonConvert.SerializeObject(cache);
            
            File.WriteAllText(fn, json);
        }

        public static void Save_Full(DateTime dt, string fileData)
        {
            if (!string.IsNullOrWhiteSpace(fileData))
            {
                string fn = GetFileName_Full(dt);
                File.WriteAllText(fn, fileData);
            }
        }
    }
}