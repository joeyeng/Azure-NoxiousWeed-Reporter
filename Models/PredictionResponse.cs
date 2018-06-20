using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NoxiousWeedsWebApp.Models
{
    public class PredictionResponse
    {
        public string ImageUrl { get; set; }
        public string Tag { get; set; }
        public double Probability { get; set; }
        public string Message { get; internal set; }
    }
}
