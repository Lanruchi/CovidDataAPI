using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CovidDataApi.Domain
{
    public class GrowthRate
    {
        public string Date { get; set; }
        public int Cases { get; set; }
        public int Increase { get; set; }
    }
}
