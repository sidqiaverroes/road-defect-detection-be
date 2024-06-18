using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Models
{
    public class WebsocketPayload
    {
        public string? Roll { get; set; }
        public string? Pitch { get; set; }
        public string? Euclidean { get; set; }
        public InternationalRoughnessIndex IRI { get; set; }
        public string? Velocity { get; set; }
        public Coordinate Coordinate { get; set; }
        public string? Timestamp { get; set; }
        public string? AttemptId { get; set; }
        
    }
}