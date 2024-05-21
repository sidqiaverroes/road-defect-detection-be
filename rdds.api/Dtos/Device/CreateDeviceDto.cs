using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Dtos.Device
{
    public class CreateDeviceDto
    {
        public string? MacAddress { get; set; }
        public string? DeviceName { get; set; }
    }
}