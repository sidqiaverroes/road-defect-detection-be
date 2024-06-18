using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Models;

namespace rdds.api.Dtos.RoadData
{
    public class CreateRoadDataDto
    {
        public float Roll { get; set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public float Euclidean { get; set; }
        public float Velocity { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string Timestamp { get; set;}
    }
}