using StockPressure.CacheClasses;
using StockPressure.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StockPressure.DataEngine
{
    public class FinraDataEngine
    {
        public FinraDataEngine()
        {

        }

        public FinraData GetFileDataForDate(DateTime startingDate, string symbol, bool recache)
        {
            FinraData data = null;

            if (FinraCache.Has(symbol, startingDate) && !recache)
            {
                data = FinraCache.Load(symbol, startingDate);
            }
            else
            {
                if (FinraCache.Has_Full(startingDate) && !recache)
                {
                    try
                    {
                        string fileData = FinraCache.Load_Full(startingDate);
                        data = ParseForSymbol(symbol, fileData);
                        FinraCache.Save(symbol, startingDate, data);
                    }
                    catch (Exception ex)
                    {
                        // Do nothing. Something went wrong, and the data is just missing or bad. 
                        // Either way, we want no part of that.
                    }
                }
                else
                { 
                    string dateFile = @"http://regsho.finra.org/" + $"CNMSshvol{startingDate.ToString("yyyyMMdd")}.txt";

                    var webRequest = WebRequest.Create(dateFile);
                    try
                    {
                        using (var response = webRequest.GetResponse())
                        using (var content = response.GetResponseStream())
                        using (var reader = new StreamReader(content))
                        {
                            var strContent = reader.ReadToEnd();

                            FinraCache.Save_Full(startingDate, strContent);

                            data = ParseForSymbol(symbol, strContent);
                        }

                        FinraCache.Save(symbol, startingDate, data);
                    }
                    catch (Exception ex)
                    {
                        // Do nothing. Something went wrong, and the data is just missing or bad. 
                        // Either way, we want no part of that.
                    }
                }
            }
            return data;
        }

        private static FinraData ParseForSymbol(string symbol, string strContent)
        {
            FinraData data = null;

            foreach (string l in strContent.Replace("\r\n", "\n").Split("\n".ToCharArray()))
            {
                string[] strArr = l.Split("|".ToCharArray()).ToArray();
                if (strArr.Length > 4 && strArr[1] == symbol)
                {
                    data = new FinraData();

                    data.Symbol = strArr[1];
                    data.ShortVolume = int.Parse(strArr[2]);
                    data.ShortExempt = int.Parse(strArr[3]);
                    data.TotalVolume = int.Parse(strArr[4]);

                    break;
                }
            }

            return data;
        }
    }
}
