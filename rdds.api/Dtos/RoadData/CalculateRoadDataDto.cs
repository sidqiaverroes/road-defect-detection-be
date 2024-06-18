using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Dtos.RoadData
{
    public class CalculateRoadDataDto
    {
        public float Roll { get; set; }
        public float Pitch { get; set; }
        public float Euclidean { get; set; }
    }
}