using System;
using System.Text;
using StockPressure.CacheClasses;
using StockPressure.DataEngine;
using StockPressure.Models;

namespace StockPressure
{
    /// <summary>
    /// TO USE THIS PROGRAM, YOU MUST GET YOUR OWN ACCOUNT WITH https://rapidapi.com/apidojo/api/yahoo-finance1
    /// </summary>
    class Program
    {
        public static ManualStatistics ManualStatistics { get; private set; }

        static void Main(string[] args)
        {
            //Console.SetWindowSize(100, Console.WindowHeight);
            string symbol = "GME";
            int pressureLine = 70;

            bool showVolumes = false;
            string reCacheInformation = "N";

            if (AppConfig.ApiKey == null || AppConfig.ApiHost == null)
            {
                Console.WriteLine("To use this program, you must have an API account with https://rapidapi.com/apidojo/api/yahoo-finance1");
                Console.WriteLine("You should only have to do this once.");
                Console.Write("Enter your API Key here :");
                string apiKey = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(apiKey)) AppConfig.ApiKey = apiKey.Trim();

                Console.Write("Write your API Host here :");
                string apiHost = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(apiKey)) AppConfig.ApiHost = apiHost.Trim();
            }

            YahooDataEngine yahooEngine = new YahooDataEngine();
            YahooFinanceStatistics yfData = yahooEngine.GetYahooFinanceForSymbol(symbol, false);

            while (true)
            {
                GetConfiguration(ref symbol, ref pressureLine, ref showVolumes, ref reCacheInformation);

                Console.WriteLine("------------------------------------------------------------------------------");

                // reset Yahoo data from Cache or query...
                yfData = yahooEngine.GetYahooFinanceForSymbol(symbol, reCacheInformation == "Y");

                if (yfData?.defaultKeyStatistics?.sharesOutstanding == null)
                {
                    Console.WriteLine($"ERROR: Unable to find data for {symbol}.\r\n\r\n");
                    reCacheInformation = "N";
                    continue;
                }

                DateTime DateToUse = yfData.defaultKeyStatistics.dateShortInterest.value;
                Int64 InitialSharesShort = yfData.defaultKeyStatistics.sharesShort.value;
                Int64 InitialFloat = yfData.defaultKeyStatistics.floatShares.value;

                GetShareData(ref DateToUse, ref InitialSharesShort, ref InitialFloat, symbol);

                if (yfData.defaultKeyStatistics.sharesOutstanding.value > 0)
                {
                    GetShortDataSinceDate(
                        InitialFloat,
                        InitialSharesShort,
                        DateToUse,
                        symbol,
                        pressureLine,
                        showVolumes,
                        reCacheInformation == "Y"
                    );
                }

                Console.WriteLine("------------------------------------------------------------------------------");
                reCacheInformation = "N";
            }
        }

        private static void GetShareData(ref DateTime startDate, ref Int64 initialShort, ref long initialFloat, string symbol)
        {
            Console.WriteLine("");
            Console.Write("How do you want to pull FINRA data? [C]urrent, [M]anual with Cache, or [F]orce manual override? (C) : ");
            string specifyStartingDataManually = GetString("C");

            if (specifyStartingDataManually.Trim().ToUpper() == "F" ||
                specifyStartingDataManually.Trim().ToUpper() == "M")
            {
                Console.WriteLine("------------------------------------------------------------------------------");

                Console.Write($"What date is this starting from? ({startDate:yyyy-MM-dd}) : ");
                startDate = DateTime.Parse(GetString($"{startDate:yyyy-MM-dd}"));

                if (YFCache.Has(symbol, startDate) && specifyStartingDataManually.Trim().ToUpper() != "F")
                {
                    Console.WriteLine("You've entered this date before for this symbol.");
                    ManualStatistics data = YFCache.Load(symbol, startDate);
                    initialFloat = data.FloatShares;
                    initialShort = data.ShortShares;
                    Console.WriteLine($"Initial Float : {initialFloat}");
                    Console.WriteLine($"Initial Short : {initialShort}");
                }
                else
                {
                    Console.Write($"What value do you want to use for the number of shares in the float? ({initialFloat}) : ");
                    initialFloat = Int64.Parse(GetString($"{initialFloat}"));

                    Console.Write($"What value do you want to use for the number of shares shorted? ({initialShort}) : ");
                    initialShort = Int64.Parse(GetString($"{initialShort}"));

                    ManualStatistics data = new ManualStatistics()
                    {
                        StartingDate = startDate,
                        FloatShares = initialFloat,
                        ShortShares = initialShort
                    };
                    YFCache.Save(symbol, startDate, data);
                }
            }

            Console.WriteLine("------------------------------------------------------------------------------");
        }

        private static void GetConfiguration(ref string symbol, ref int pressureLine, ref bool showVolumes, ref string recache)
        {
            Console.Write($"Symbol ({symbol}): ");
            symbol = GetString(symbol);

            Console.Write($"At what percent do you think the short-ers will be stuck? ({pressureLine}) : ");
            pressureLine = int.Parse(GetString(pressureLine.ToString()));

            Console.Write($"Show underlying volumes? ({showVolumes}) : ");
            showVolumes = bool.Parse(GetString(showVolumes.ToString()));

            if (YFCache.Has(symbol))
            {
                Console.WriteLine($"It appears this has been cached before. Do you want to refresh the cache?");
                Console.Write($"(use sparingly or you could lock yourself out.) ({recache}) : ");
                recache = GetString(recache);
            }
        }

        private static void ShowStatBlock(string symbol, YahooFinanceStatistics yfData, DateTime DateToUse, Int64 InitialSharesShort)
        {
            Int64 sharesHeldByInsiders = (Int64)(
                yfData.defaultKeyStatistics.sharesOutstanding.value *
                yfData.defaultKeyStatistics.heldPercentInsiders.value
            );

            Int64 sharesHeldByInstitutions = (Int64)(
                yfData.defaultKeyStatistics.sharesOutstanding.value *
                yfData.defaultKeyStatistics.heldPercentInstitutions.value
            );

            decimal calcFloat = yfData.defaultKeyStatistics.sharesOutstanding.value - sharesHeldByInsiders;
            decimal calcShort = yfData.defaultKeyStatistics.shortPercentOfFloat.value * calcFloat;
            decimal calcShortRatio = (decimal)InitialSharesShort / calcFloat;

            Console.WriteLine("");
            Console.WriteLine($"According to Yahoo Finance, on {DateToUse:yyyy-MM-dd} " +
                $"the outstanding shorts for {symbol} were ***{InitialSharesShort:###,###,###,##0}***, or " +
                $"***{calcShortRatio * 100m:0.0000}%*** of float.");
            Console.WriteLine("");
            Console.WriteLine($"Total outstanding shares   : ***.{yfData.defaultKeyStatistics.sharesOutstanding.value,15:###,###,###,##0}*** ");
            Console.WriteLine($"Held by insiders           : ***.{yfData.defaultKeyStatistics.heldPercentInsiders.value * 100.0m,14:0.000}%*** ( approx. ***.{sharesHeldByInsiders,14:###,###,###,##0}*** )");
            Console.WriteLine($"Held by institutions       : ***.{yfData.defaultKeyStatistics.heldPercentInstitutions.value * 100.0m,14:0.000}%*** ( approx. ***.{sharesHeldByInstitutions,14:###,###,###,##0}*** )");
            Console.WriteLine($"Reported Float             : ***.{yfData.defaultKeyStatistics.floatShares.value,15:###,###,###,##0}***");
            Console.WriteLine($"Reported Short of Float    : ***.{yfData.defaultKeyStatistics.shortPercentOfFloat.value * 100m,14:0.0000}%*** " +
                $"( approx. ***.{yfData.defaultKeyStatistics.shortPercentOfFloat.value * yfData.defaultKeyStatistics.floatShares.value,14:###,###,###,##0}*** )");
            Console.WriteLine($"Reported Short Ratio       : ***.{yfData.defaultKeyStatistics.shortRatio.value * 100m,14}%***");
            Console.WriteLine("");
            Console.WriteLine($"Shares Short Prev. Month   : ***.{yfData.defaultKeyStatistics.sharesShortPriorMonth.value,14:###,###,###,##0}*** " +
                $"( approx. ***.{((decimal)yfData.defaultKeyStatistics.sharesShortPriorMonth.value / (decimal)yfData.defaultKeyStatistics.floatShares.value) * 100m,14:0.00}%*** )");
            Console.WriteLine($"Shares Short Current Month : ***.{yfData.defaultKeyStatistics.sharesShort.value,14:###,###,###,##0}*** " +
                $"( approx. ***.{((decimal)yfData.defaultKeyStatistics.sharesShort.value / (decimal)yfData.defaultKeyStatistics.floatShares.value) * 100m,14:0.00}%*** )");
            Console.WriteLine("");
            Console.WriteLine($"Calculated Float           : ***.{calcFloat,15:###,###,###,##0}***");
            Console.WriteLine($"Calculated Short of Float  : ***.{yfData.defaultKeyStatistics.shortPercentOfFloat.value * 100m,14:0.0000}%*** ( approx. ***.{calcShort,14:###,###,###,##0}*** )");
            Console.WriteLine($"Calculated Short Ratio     : ***.{calcShortRatio * 100m,14:0.00}%***");
            Console.WriteLine("");
        }

        private static string GetString(string start)
        {
            string tmpStr = Console.ReadLine();
            if (tmpStr.Trim() == "") return start;

            return tmpStr;
        }

        private static void GetShortDataSinceDate(Int64 initialFloat, Int64 initialShort,  DateTime startingDate, string symbol, 
            int pressureLine, bool showVolumes, bool recache)
        {
            Int64 cumulativeShort = initialShort;

            DateTime curr = startingDate;


            Console.WriteLine($"Symbol           : {symbol}");
            Console.WriteLine($"Starting Date    : {startingDate:yyyy-MM-dd} ");
            Console.WriteLine($"Float            : ***{initialFloat:###,###,###,##0}***");
            Console.WriteLine($"Starting Short   : ***{initialShort:###,###,###,##0}***");
            Console.WriteLine($"Starting Short % : ***{(initialShort / initialFloat) * 100m:0.0000}%*** of float."); ;
            Console.WriteLine("");

            // I'll remove this chunk of expository once I know the calculations are correct and just let the numbers be the numbers.
            // ------------------------------------------------------------------------------------------------------------------------
            Console.WriteLine($"You can find the link to this code at: https://github.com/jerryhanel/StockPressure/ Take a look at the thesis and " +
                $"let me know if you can determine if this DD has somehow gone off the rails. " +
                $"\r\n\r\n" +
                $"***Keep an eye on the second column, Pressure #. That's the key factor here.***");
            Console.WriteLine("");

            // Create Markdown Table            
            Console.Write("| Date       | Pressure # | +/- | Uncovered Volume (     %) |");
            if (showVolumes)
            {
                Console.Write(" Volume          | Short Volume    | Potential Short % |");
            }
            Console.WriteLine("");

            Console.Write("|:----------:|-----------:|:---:|--------------------------:|");
            if (showVolumes)
            {
                Console.Write("----------------:|----------------:|------------------:|");
            }
            Console.WriteLine("");

            FinraDataEngine finraEngine = new FinraDataEngine();

            bool todaysFile = AppConfig.CheckForToday ?
                    finraEngine.GetFileDataForDate(DateTime.Now.Date, symbol, recache) != null :
                    true;

            if (todaysFile)
            {
                decimal prevSqueezeFactor = 0;
                int decimals = 5;

                while (curr < DateTime.Now.Date)
                {
                    curr = curr.AddDays(1);

                    if (curr.DayOfWeek == DayOfWeek.Sunday || curr.DayOfWeek == DayOfWeek.Saturday) continue;

                    FinraData data = finraEngine.GetFileDataForDate(curr, symbol, recache);

                    // Keep a cumulative count of short volumes. This is not EXACT, but we'll use this 
                    // to determine how much PRESSURE I think the hedge funds might be under. This assumes
                    // that ALL non-short volumes go to cover the short volumes.
                    if (data != null && initialFloat != 0)
                    {
                        // If I look at ALL possible trades (that I know of) as having the POTENTIAL to cover shorts, then
                        // the potential short coverage for the day is TotalVolume - (ShortVolume * 2). This is *2, because the first
                        // piece of volume was to transact the "short". The second one is to "cover" the short. The pressure is then
                        // the difference of what remains of that equation. If the stock has ANY positive volume left over, it could cover 
                        // previous shorts, decreasing the value. If negative, it INCREASES previous shorts, then it starts this all 
                        // over again the next day.
                        // 
                        // That makes the equation something like:
                        //     cumulative shorts = cumulative shorts - (Sales Today - (Shorts Today * 2)).

                        Int64 coveredShorts = data.TotalVolume - (data.ShortVolume * 2);
                        Int64 unCoveredShorts = (data.ShortVolume * 2) - data.TotalVolume;
                        cumulativeShort = cumulativeShort + unCoveredShorts;

                        // If they've already potentially cleared their position, set it to 0. We're done. There is no squeeze for
                        // this day.
                        if (cumulativeShort < 0) { cumulativeShort = 0; }

                        // How does this cumulative Shortage compare to the shares outstanding (by percent).
                        // This is the POTENTIAL Short Percent. This is NOT the actual Short Percent.
                        decimal shortPercent = Math.Round((decimal)cumulativeShort / (decimal)initialFloat, decimals);

                        // I need shortPercent and pressureLine to be in the same units, so divide pressure by 100. 
                        decimal squeezeFactor = shortPercent / (pressureLine / 100m);

                        string incdec = " ";
                        if (squeezeFactor > prevSqueezeFactor) incdec = "+";
                        if (squeezeFactor < prevSqueezeFactor) incdec = "-";
                        prevSqueezeFactor = squeezeFactor;

                        // At this point, the squeeze factor is just a number. It should not be mentally coupled with the squeeze percent since I don't
                        // know what that true number is. But what I DO know is that at this level the Hedge funds should be feeling quite a bit of 
                        // pressure, which could drive the price up. And that's what I am trying to find. Potential PRESSURE to facilitate a squeeze.
                        Console.Write($"| {curr:yyyy-MM-dd} | {(squeezeFactor * 100m),10:0.0} |  {incdec}  | {unCoveredShorts,15:###,###,###,##0} (" +
                                $"{((decimal)unCoveredShorts / (decimal)initialFloat) * 100m,6:0.00}%) ");
                        if (showVolumes)
                        {
                            Console.Write($"| " +
                                $"{data.TotalVolume,15:###,###,###,##0} | {data.ShortVolume,15:###,###,###,##0} | {shortPercent * 100,15:0.0000} % |");
                        }
                        Console.WriteLine("");
                    }
                    else
                    {
                        Console.WriteLine($"| {curr.Date:yyyy-MM-dd} | has no data.");
                    }
                }
            }
            else
            {
                Console.WriteLine("| No data for today, yet.");
            }
        }
    }
}
