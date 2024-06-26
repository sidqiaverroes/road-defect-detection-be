using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Dtos.Device;
using rdds.api.Models;

namespace rdds.api.Interfaces
{
    public interface IDeviceRepository
    {
        Task<List<Device>> GetAllAsync();
        Task<Device?> GetByMacAddressAsync(string mac);
        Task<Device> CreateAsync(Device deviceModel);
        Task<Device?> UpdateAsync(string mac, Device deviceDto);
        Task<Device?> DeleteAsync(string mac);
        Task<string?> IsExistedAsync(string mac);

    }
}