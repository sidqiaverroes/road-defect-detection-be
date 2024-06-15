using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Models
{
    public class UserAccess
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public AppUser User { get; set; }

        [Required]
        public int AccessTypeId { get; set; }

        [ForeignKey("AccessTypeId")]
        public AccessType AccessType { get; set; }
    }
}