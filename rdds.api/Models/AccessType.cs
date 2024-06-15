using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Models
{
    public class AccessType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // Example: "read", "write", "update", "delete"

        // Navigation property to represent the many-to-many relationship
        public ICollection<UserAccess> UserAccesses { get; set; }
    }
}