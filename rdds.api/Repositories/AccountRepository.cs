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
                return new List<UserDto>();
            }

            var userIdsInRole = usersInRole.Select(u => u.Id).ToList();

            var usersWithAccessTypes = await _context.Users
                .Where(u => userIdsInRole.Contains(u.Id))
                .Include(u => u.AccessType)
                .ToListAsync();

            var userDtos = usersWithAccessTypes.Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                AccessType = new AccessTypeDto
                {
                    Id = u.AccessType.Id,
                    Name = u.AccessType.Name,
                    Accesses = u.AccessType.Accesses
                }
            }).ToList();

            return userDtos;
        }

        public Task UpdateUserAccessesAsync(string userId, List<int> accessTypeIds)
        {
            throw new NotImplementedException();
        }
    }
}