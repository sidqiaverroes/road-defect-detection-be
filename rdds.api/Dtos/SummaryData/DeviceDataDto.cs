using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Models;

namespace rdds.api.Dtos.SummaryData
{
    public class DeviceDataDto
    {
        public string RoadCategory { get; set; }
        public double TotalLength { get; set; }
        public LengthData? LengthData { get; set; }
        public PercentageData? PercentageData { get; set; }
    }
}