using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Dtos.Account
{
    public class UserDto
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public List<UserAccessDto>? UserAccesses { get; set; }
    }

    public class UserAccessDto
    {
        public int AccessTypeId { get; set; }
        public string? AccessTypeName { get; set; }
    }
}