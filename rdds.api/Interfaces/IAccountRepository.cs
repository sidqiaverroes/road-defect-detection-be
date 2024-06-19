using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Dtos.Account;
using rdds.api.Models;

namespace rdds.api.Interfaces
{
    public interface IAccountRepository
    {
        Task<List<UserDto>> GetAllAsync();
        Task UpdateUserAccessAsync(string userId, int accessTypeId);
        Task<AppUser> GetUserByIdAsync(string userId);

    }
}