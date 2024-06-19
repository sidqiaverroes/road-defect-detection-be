using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Dtos.AccessType;
using rdds.api.Dtos.Account;
using rdds.api.Models;

namespace rdds.api.Mappers
{
    public static class AccountMapper
    {
        public static UserDto ToUserDto(this AppUser userModel)
        {
            return new UserDto
            {
                Id = userModel.Id,
                Username = userModel.UserName,
                Email = userModel.Email,
                AccessType = new AccessTypeDto
                {
                    Id = userModel.AccessType.Id,
                    Name = userModel.AccessType.Name,
                    Accesses = userModel.AccessType.Accesses
                }
            };
        }
    }
}