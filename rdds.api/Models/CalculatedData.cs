using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Models
{
    public class CalculatedData
    {
        public Guid Id { get; set; }
        public PowerSpectralDensity PSD { get; set; } = new PowerSpectralDensity();
        public InternationalRoughnessIndex IRI { get; set; } = new InternationalRoughnessIndex();
         public float Velocity { get; set; }
        public Coordinate Coordinate { get; set; } = new Coordinate();
        public DateTime Timestamp { get; set; }
        public int? AttemptId {get; set;}
        public Attempt? Attempt { get; set; }

    }
}