using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Models
{
    public class Attempt
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;
        public bool IsFinished { get; set; } = false;
        public DateTime? FinishedOn { get; set; }
        public string? DeviceId {get; set;}
        public Device? Device { get; set; }
        public int? RoadCategoryId { get; set; }
        public RoadCategory RoadCategory { get; set; }

        public List<RoadData> RoadDatas { get; set; } = new List<RoadData>();
        public List<CalculatedData> CalculatedData { get; set; } = new List<CalculatedData>();
    }
}