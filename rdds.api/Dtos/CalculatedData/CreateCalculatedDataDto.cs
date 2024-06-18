using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Models;

namespace rdds.api.Dtos.CalculatedData
{
    public class CreateCalculatedDataDto
    {
        public InternationalRoughnessIndex IRI { get; set; }
        public float Velocity { get; set; }
        public Coordinate Coordinate { get; set; }
        public string? Timestamp { get; set; }
        public int? AttemptId { get; set; }
    }
}