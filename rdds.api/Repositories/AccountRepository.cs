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
using rdds.api.Dtos.AccessType;

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

        public async Task UpdateUserAccessAsync(string userId, int accessTypeId)
        {
           
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                throw new ArgumentException("User not found", nameof(userId));
            }

            var accessType = await _context.AccessTypes.FindAsync(accessTypeId);

            if (accessType == null)
            {
                throw new ArgumentException("AccessType not found", nameof(accessTypeId));
            }

            user.AccessTypeId = accessTypeId;

            await _context.SaveChangesAsync();
        
        }

    }
}