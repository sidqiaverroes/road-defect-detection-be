using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Dtos.RoadCategory
{
    public class RoadCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float TotalLength { get; set; }
    }
}