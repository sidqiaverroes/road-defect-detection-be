using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Models
{
    public class Permission
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        public List<UserPermission> UserPermissions { get; set; }

    }
}