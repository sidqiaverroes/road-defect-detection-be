using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Models
{
    public class AccessType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; } 
        
        public List<string>? Accesses { get; set; }// Example: "read", "write", "update", "delete"

        public List<AppUser> Users { get; set; } = new List<AppUser>();
    }
}