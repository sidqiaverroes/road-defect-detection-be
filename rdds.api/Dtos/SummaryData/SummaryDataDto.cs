using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Dtos.Attempt;
using rdds.api.Models;

namespace rdds.api.Dtos.SummaryData
{
    public class SummaryDataDto
    {
        public double TotalLength { get; set; }
        public LengthData LengthData { get; set; }
        public PercentageData PercentageData { get; set; }
        public int AttemptId { get; set; }
        public AttemptDto Attempt { get; set; }
    }
}