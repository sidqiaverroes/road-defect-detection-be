using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Models
{
    public class AttemptSummaryData
    {
        [Key]
        public Guid Id { get; set; }
        public double TotalLength { get; set; }
        public LengthData? LengthData { get; set; }
        public PercentageData? PercentageData { get; set; }
        public int AttemptId { get; set; }
        public Attempt Attempt { get; set; }

    }
}