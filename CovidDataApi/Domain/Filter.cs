using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CovidDataApi.Domain
{
    public class Filter
    {
        [Required]
        public string Location { get; set; }//also need regex for allowed data 
        public string FromDate { get; set; }
        public string ToDate { get; set; }
    }
}
