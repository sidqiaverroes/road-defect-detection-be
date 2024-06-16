using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Models
{
    public class RoadData
    {
        public Guid Id { get; set; }
        public float Roll { get; set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; } 
        public float Euclidean { get; set; }
        public float Velocity { get; set; }
        public Coordinate Coordinate { get; set; } = new Coordinate();
        public DateTime Timestamp { get; set; }
        public int? AttemptId {get; set;}
        public Attempt? Attempt { get; set; }
    }
}