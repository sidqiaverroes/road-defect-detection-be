using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Dtos.Permission;
using rdds.api.Models;
namespace rdds.api.Dtos.Account
{
    public class UserDto
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public List<PermissionDto> Permissions { get; set; }
    }
}