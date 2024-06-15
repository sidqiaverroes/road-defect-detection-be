using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Dtos.Account;

namespace rdds.api.Interfaces
{
    public interface IAccountRepository
    {
        Task<List<UserDto>> GetAllAsync();
    }
}