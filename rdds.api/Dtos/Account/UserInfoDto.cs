using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Dtos.Account
{
    public class UserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public AccessTypeDto AccessType { get; set; }
    }

    public class AccessTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<string> Accesses { get; set; }
    }
}