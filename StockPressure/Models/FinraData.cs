namespace StockPressure.Models
{
    public class FinraData
    {
        public string Symbol { get; set; }
        public int ShortVolume { get; set; }
        public int ShortExempt { get; set; }
        public int TotalVolume { get; set; }

        public int MinimumShorted
        {
            get
            {
                return TotalVolume - ShortExempt;
            }
        }

        public override string ToString()
        {
            return Symbol + ":" + MinimumShorted.ToString("###,###,###,##0");
        }
    }
}
