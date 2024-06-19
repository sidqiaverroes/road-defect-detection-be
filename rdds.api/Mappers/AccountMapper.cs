using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Dtos.Account;
using rdds.api.Dtos.Permission;
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
                Permissions = userModel.UserPermissions
                    .Select(up => new PermissionDto
                    {
                        Id = up.Permission.Id,
                        Name = up.Permission.Name
                    })
                    .ToList()
            };
        }
    }
}