using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Dtos.Device
{
    public class UpdateDeviceDto
    {
        public string? DeviceName { get; set; }
        public DateTime LastModified { get; set; } = DateTime.Now;
    }
}