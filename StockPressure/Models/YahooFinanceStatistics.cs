namespace StockPressure.Models
{
    /// <summary>
    /// A class for capturing the Json file data.
    /// </summary>
    public class YahooFinanceStatistics
    {
        public string symbol { get; set; }
        public YahooQuoteKeyStatistics defaultKeyStatistics { get; set; }
    }
}