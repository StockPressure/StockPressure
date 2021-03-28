namespace StockPressure.Models
{
    /// <summary>
    /// A class for capturing the Json file data.
    /// </summary>
    public class YahooNumber
    {
        public string raw { get; set; }
        public string fmt { get; set; }
        public string longFmt { get; set; }

        public decimal value 
        { 
            get
            {
                decimal result = 0M;
                decimal.TryParse(raw, out result);
                return result;
            } 
        }
    }
}