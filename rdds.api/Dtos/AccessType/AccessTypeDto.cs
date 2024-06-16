using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Dtos.AccessType
{
    public class AccessTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<string> Accesses { get; set; }
    }
}