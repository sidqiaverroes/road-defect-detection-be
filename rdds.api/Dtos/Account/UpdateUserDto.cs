using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Dtos.Account
{
    public class UpdateUserDto
    {
        public string Username { get; set; }

        public string CurrentPassword { get; set; }

        public string NewPassword { get; set; }
    }
}