using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Models
{
    public class InternationalRoughnessIndex
    {
        public float Roll { get; set; }
        public float Pitch { get; set; }
        public float Euclidean { get; set; }
        public float Average { get; set; }
        public string? RollProfile { get; set; }
        public string? PitchProfile { get; set; }
        public string? EuclideanProfile { get; set; }
        public string? AverageProfile { get; set; }
    }
}