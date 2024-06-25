using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Dtos.SummaryData
{
    public class DeviceSummaryDataDto
    {
        public double TotalLength { get; set; }
        public List<DeviceDataDto>? DeviceDatas { get; set; }

    }
}