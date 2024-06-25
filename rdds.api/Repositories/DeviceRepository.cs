using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using rdds.api.Data;
using rdds.api.Dtos.Device;
using rdds.api.Interfaces;
using rdds.api.Models;

namespace rdds.api.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly ApplicationDbContext _context;
        public DeviceRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Device> CreateAsync(Device deviceModel)
        {
            await _context.Devices.AddAsync(deviceModel);
            await _context.SaveChangesAsync();

            return deviceModel;
        }

        public async Task<Device?> DeleteAsync(string mac)
        {
            var deviceModel = await _context.Devices.FirstOrDefaultAsync(m => m.MacAddress == mac);

            if(deviceModel == null)
            {
                return null;
            }

            _context.Devices.Remove(deviceModel);
            await _context.SaveChangesAsync();

            return deviceModel;
        }

        public async Task<List<Device>> GetAllAsync(string userId)
        {
            return await _context.Devices.Where(u => u.AppUserId == userId)
                .Include(a => a.Attempts)
                .ThenInclude(a => a.RoadCategory)
                .Include(a => a.AppUser)
                .ToListAsync();
        }

        public async Task<Device?> GetByMacAddressAsync(string mac)
        {
            return await _context.Devices
                .Include(a => a.Attempts)
                .ThenInclude(a => a.RoadCategory)
                .Include(a => a.AppUser)
                .FirstOrDefaultAsync(m => m.MacAddress == mac);
        }

        public async Task<string?> IsExistedAsync(string? mac)
        {
            if (string.IsNullOrEmpty(mac))
            {
                return null;
            }

            var result = await _context.Devices.FirstOrDefaultAsync(m => m.MacAddress == mac);

            if (result == null)
            {
                mac = mac.Replace(":", "-");
                result = await _context.Devices.FirstOrDefaultAsync(m => m.MacAddress == mac);
            }
            if (result == null)
            {
                mac = mac.Replace("-", ":");
                result = await _context.Devices.FirstOrDefaultAsync(m => m.MacAddress == mac);
            }
            if (result == null)
            {
                return null;
            }

            return result.MacAddress;
        }

        public async Task<Device?> UpdateAsync(string mac, Device deviceModel)
        {
            var existingDevice = await _context.Devices.FirstOrDefaultAsync(m => m.MacAddress == mac);

            existingDevice.DeviceName = deviceModel.DeviceName;
            existingDevice.LastModified = deviceModel.LastModified;

            await _context.SaveChangesAsync();

            return existingDevice;
        }
    }
}