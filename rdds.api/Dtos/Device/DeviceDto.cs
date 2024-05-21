using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.CustomValidation;
using rdds.api.Dtos.Attempt;
using rdds.api.Models;

namespace rdds.api.Dtos.Device
{
    public class DeviceDto
    {
        public string MacAddress {get; set;} = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;

        public List<AttemptDto> Attempts {get; set;}
    }
}