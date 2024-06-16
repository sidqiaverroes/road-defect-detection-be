using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Models;

namespace rdds.api.Interfaces
{
    public interface IAccessTypeRepository
    {
        Task<List<AccessType>> GetAllAccessTypesAsync();
        Task<AccessType> GetAccessTypeByIdAsync(int id);
        Task<AccessType> CreateAccessTypeAsync(AccessType accessType);
        Task<AccessType> UpdateAccessTypeAsync(int id, AccessType accessType);
        Task<bool> DeleteAccessTypeAsync(int id);
        Task<AccessType> GetAccessTypeByNameAsync(string name);
    }
}