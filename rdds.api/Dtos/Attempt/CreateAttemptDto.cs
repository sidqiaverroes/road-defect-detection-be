using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Dtos.Attempt
{
    public class CreateAttemptDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}