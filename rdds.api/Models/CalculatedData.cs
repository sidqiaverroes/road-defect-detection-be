using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Models
{
    public class CalculatedData
    {
        [Key]
        public DateTimeOffset Timestamp { get; set; }
        public InternationalRoughnessIndex IRI { get; set; } = new InternationalRoughnessIndex();
        public float Velocity { get; set; }
        public Coordinate CoordinateStart { get; set; } = new Coordinate();
        public Coordinate CoordinateEnd { get; set; } = new Coordinate();
        public int? AttemptId {get; set;}
        public Attempt? Attempt { get; set; }

    }
}