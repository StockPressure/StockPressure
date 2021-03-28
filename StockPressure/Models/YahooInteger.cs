using System;

namespace StockPressure.Models
{
    /// <summary>
    /// A class for capturing the Json file data.
    /// </summary>
    public class YahooInteger
    {
        public string raw { get; set; }
        public string fmt { get; set; }
        public string longFmt { get; set; }

        public Int64 value 
        { 
            get
            {
                Int64 result = 0;
                Int64.TryParse(raw, out result);
                return result;
            } 
        }
    }
}