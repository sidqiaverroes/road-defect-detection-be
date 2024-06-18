using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Dtos.CalculatedData
{
    public class CreateCalculatedDataDto
    {
        public float PSDRoll { get; set; }
        public float PSDPitch { get; set; }
        public float PSDEuclidean { get; set; }
        public float IRIRoll { get; set; }
        public float IRIPitch { get; set; }
        public float IRIEuclidean { get; set; }
        public float Velocity { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string Timestamp { get; set; }
    }
}