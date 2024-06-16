using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using rdds.api.Data;
using rdds.api.Interfaces;
using rdds.api.Models;

namespace rdds.api.Repositories
{
    public class AccessTypeRepository : IAccessTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public AccessTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AccessType> CreateAccessTypeAsync(AccessType accessType)
        {
             _context.AccessTypes.Add(accessType);
            await _context.SaveChangesAsync();
            return accessType;
        }

        public async Task<bool> DeleteAccessTypeAsync(int id)
        {
            var accessType = await _context.AccessTypes.FindAsync(id);
            if (accessType != null)
            {
                _context.AccessTypes.Remove(accessType);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<AccessType> GetAccessTypeByIdAsync(int id)
        {
            return await _context.AccessTypes.FindAsync(id);
        }

        public async Task<AccessType> GetAccessTypeByNameAsync(string name)
        {
            return await _context.AccessTypes.FirstOrDefaultAsync(at => at.Name == name);
        }

        public async Task<List<AccessType>> GetAllAccessTypesAsync()
        {
            return await _context.AccessTypes.ToListAsync();
        }

        public async Task<AccessType> UpdateAccessTypeAsync(int id, AccessType accessType)
        {
            var existingAccessType = await _context.AccessTypes.FindAsync(id);
            if (existingAccessType != null)
            {
                existingAccessType.Name = accessType.Name;
                existingAccessType.Accesses = accessType.Accesses;
                await _context.SaveChangesAsync();
            }
            return existingAccessType;
        }
    }
}