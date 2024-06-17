using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Models
{
    public class SensorData
    {
        public string timestamp { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }
        public float velocity { get; set; }
        public float roll { get; set; }
        public float pitch { get; set; }
        public float euclidean { get; set; }
    }
}