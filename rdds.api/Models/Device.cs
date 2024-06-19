using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.CustomValidation;

namespace rdds.api.Models
{
    public class Device
    {
        [Key]
        [MacAddress(ErrorMessage = "Invalid MAC address format.")]
        [Required(ErrorMessage = "Mac Address is required.")]
        public string MacAddress {get; set;} = string.Empty;
        [Required(ErrorMessage = "Device name is required.")]
        public string DeviceName { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public DateTime LastModified { get; set; } = DateTime.Now;
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        public List<Attempt> Attempts {get; set;} = new List<Attempt>();
    }
}