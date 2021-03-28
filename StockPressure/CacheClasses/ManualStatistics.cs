using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPressure.CacheClasses
{
    public class ManualStatistics
    {
        public DateTime StartingDate { get; set; }
        public Int64 OutstandingShares { get; set; }
        public Int64 FloatShares { get; set; }
        public Int64 ShortShares { get; set; }
    }
}
