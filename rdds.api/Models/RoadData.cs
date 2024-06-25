using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Models
{
    public class RoadData
    {
        [Key]
        public DateTimeOffset Timestamp { get; set; }
        public float Roll { get; set; }
        public float Pitch { get; set; }
        public float Euclidean { get; set; }
        public float Velocity { get; set; }
        public Coordinate Coordinate { get; set; } = new Coordinate();
        public int? AttemptId { get; set; }
        public Attempt Attempt { get; set; } 
    }
}