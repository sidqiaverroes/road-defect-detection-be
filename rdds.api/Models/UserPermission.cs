using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Models
{
    public class UserPermission
    {
        public string UserId { get; set; }
        public AppUser User { get; set; }

        public int PermissionId { get; set; }
        public Permission Permission { get; set; }

    }
}