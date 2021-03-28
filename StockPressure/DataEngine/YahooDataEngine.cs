using Newtonsoft.Json;
using StockPressure.CacheClasses;
using StockPressure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StockPressure.DataEngine
{
    public class YahooDataEngine
    {
        public YahooFinanceStatistics GetYahooFinanceForSymbol(string symbol, bool recache)
        {
            YahooFinanceStatistics obj = null;
            string apiKey = AppConfig.ApiKey;
            string apiHost = AppConfig.ApiHost;
            if (YFCache.Has(symbol) && !recache)
            {
                obj = YFCache.Load(symbol);
            }
            else
            {
                int currentMonth = DateTime.Now.Month;
                string currentSection = (DateTime.Now.Day <= 15) ? "A" : "B";

                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://{apiHost}/stock/v2/get-statistics?symbol={symbol}&region=US"),
                    Headers =
                    {
                        { "x-rapidapi-key", apiKey },
                        { "x-rapidapi-host", apiHost },
                    },
                };

                using (var response = client.SendAsync(request).Result)
                {
                    response.EnsureSuccessStatusCode();
                    var body = response.Content.ReadAsStringAsync().Result;

                    obj = JsonConvert.DeserializeObject<YahooFinanceStatistics>(body);

                    if (obj?.defaultKeyStatistics?.dateShortInterest?.value == null)
                    {
                        Console.WriteLine($"Unable to pull [{symbol}] from Yahoo.");
                        return null;
                    }

                    if (obj.defaultKeyStatistics.dateShortInterest.value.AddDays(15).Month != currentMonth ||
                        (obj.defaultKeyStatistics.dateShortInterest.value.AddDays(15).Day  <= 15 && currentSection != "A") ||
                        (obj.defaultKeyStatistics.dateShortInterest.value.AddDays(15).Day  >  15 && currentSection != "B") 
                       )
                    {
                        Console.WriteLine("We are beyond the date to retrieve that information from Yahoo.");
                        return null;
                    }
                }

                YFCache.Save(symbol, obj);
            }
            return obj;
        }
    }
}
