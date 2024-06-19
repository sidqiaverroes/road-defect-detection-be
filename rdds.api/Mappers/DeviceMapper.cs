using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Dtos.Device;
using rdds.api.Models;

namespace rdds.api.Mappers
{
    public static class DeviceMapper
    {
        public static DeviceDto ToDeviceDto(this Device deviceModel)
        {
            return new DeviceDto
            {
                MacAddress = deviceModel.MacAddress,
                DeviceName = deviceModel.DeviceName,
                CreatedOn = deviceModel.CreatedOn,
                LastModified = deviceModel.LastModified,
                Attempts = deviceModel.Attempts.Select(a => a.ToAttemptDto()).ToList()
            };
        }

        public static Device ToDeviceFromCreateDto(this CreateDeviceDto deviceDto)
        {
            return new Device
            {
                MacAddress = deviceDto.MacAddress,
                DeviceName = deviceDto.DeviceName,
                CreatedOn = DateTime.Now,
            };
        }

        public static Device ToDeviceFromUpdate(this UpdateDeviceDto deviceDto)
        {
            return new Device
            {
                DeviceName = deviceDto.DeviceName,
                LastModified = DateTime.Now,
            };
        }
    }
}