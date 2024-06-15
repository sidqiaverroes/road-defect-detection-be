using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Data;
using rdds.api.Interfaces;
using rdds.api.Models;
using rdds.api.Dtos.Account;
using Microsoft.AspNetCore.Identity;
using rdds.api.Mappers;

namespace rdds.api.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public AccountRepository(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<UserDto>> GetAllAsync()
    {
        var usersInRole = await _userManager.GetUsersInRoleAsync("User");

        if (usersInRole == null || !usersInRole.Any())
        {
            return new List<UserDto>(); // Return an empty list if usersInRole is null or empty
        }

        var userIdsInRole = usersInRole.Select(u => u.Id).ToList();

        var usersWithAccessTypes = await _context.Users
            .Where(u => userIdsInRole.Contains(u.Id))
            .Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                UserAccesses = u.UserAccesses.Select(ua => new UserAccessDto
                {
                    AccessTypeId = ua.AccessTypeId,
                    AccessTypeName = ua.AccessType.Name
                }).ToList()
            })
            .ToListAsync();

        return usersWithAccessTypes;
    }


    }
}