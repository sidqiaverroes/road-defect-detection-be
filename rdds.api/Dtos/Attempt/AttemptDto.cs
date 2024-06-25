using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Dtos.Attempt
{
    public class AttemptDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;
        public bool IsFinished { get; set; }
        public DateTime? FinishedOn { get; set; }
        public string? DeviceId {get; set;}
        public int? RoadCategoryId { get; set; }
        public string? RoadCategoryName { get; set; }
    }
}