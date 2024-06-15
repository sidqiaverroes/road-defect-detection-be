using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace rdds.api.Models
{
    public class AppUser : IdentityUser
    {
        public ICollection<UserAccess> UserAccesses { get; set; }
    }
}