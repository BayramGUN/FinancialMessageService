using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public interface IMessageAvailable
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal LowPrice { get; set; }
        public decimal HighPrice { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal Difference { get; set; }
        public decimal PreClose { get; set; }
        public long Volume { get; set; }
        public decimal DifferencePercent { get; set; }
        public string Time { get; set; }
        public char DifferenceArrow { get; set; }
    }
}
