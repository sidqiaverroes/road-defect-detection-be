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

            var usersWithPermissions = await _context.Users
                .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
                .Where(u => userIdsInRole.Contains(u.Id))
                .ToListAsync();

            var userDtos = usersWithPermissions.Select(u => u.ToUserDto()).ToList();

            return userDtos;
        }

        public async Task<AppUser> GetUserByIdAsync(string userId)
        {
            var user = await _context.Users
                .Include(u => u.UserPermissions) // Include UserPermissions navigation property
                    .ThenInclude(up => up.Permission) // Include Permission navigation property
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return null;
            }

            return user;
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            var user = await _context.Users
                .Include(u => u.UserPermissions) // Include UserPermissions navigation property
                .ThenInclude(up => up.Permission) // Include Permission navigation property
                .FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                return null;
            }

            return user;
        }

        public async Task UpdateUserPermissionsAsync(string userId, List<int> permissionIds)
        {
            var user = await _context.Users
                .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new ArgumentException("User not found", nameof(userId));
            }

            // Clear existing permissions
            user.UserPermissions.Clear();

            // Add new permissions based on permissionIds
            foreach (var permissionId in permissionIds)
            {
                var permission = await _context.Permissions.FindAsync(permissionId);
                if (permission != null)
                {
                    user.UserPermissions.Add(new UserPermission
                    {
                        UserId = userId,
                        PermissionId = permissionId
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

    }
}