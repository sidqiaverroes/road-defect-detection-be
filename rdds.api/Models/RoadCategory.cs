using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Models
{
    public class RoadCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float TotalLength { get; set; }

        public ICollection<Attempt> Attempts { get; set; }
    }
}