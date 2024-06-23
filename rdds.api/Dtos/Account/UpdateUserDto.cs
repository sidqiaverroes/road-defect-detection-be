using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Dtos.Account
{
    public class UpdateAdminDto
    {
        public string Username { get; set; }

        public string CurrentPassword { get; set; }

        public string NewPassword { get; set; }
    }

    public class UpdateUserDetailsDto
    {
        public string? NewPassword { get; set; }
        public string? NewUsername { get; set; }
        public string? NewEmail { get; set; }
        public List<int>? PermissionId { get; set; }
    }
}