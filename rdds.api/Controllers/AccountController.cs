using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using rdds.api.Dtos.Account;
using rdds.api.Extensions;
using rdds.api.Interfaces;
using rdds.api.Models;

namespace rdds.api.Controllers
{
    [Route("rddsapi/auth")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<AppUser> _signinManager;
        private readonly IAccountRepository _accountRepo;
        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, SignInManager<AppUser> signInManager, IAccountRepository accountRepo)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signinManager = signInManager;
            _accountRepo = accountRepo;
        }

        [Authorize]
        [HttpGet("get-all-users")]
        public async Task<IActionResult> GetAll()
        {
            var username = User.GetUsername();
            var authuser = await _userManager.FindByNameAsync(username);
            if (authuser == null)
            {
                return Unauthorized("You are not registered.");
            }
            if (!await _userManager.IsInRoleAsync(authuser, "Admin"))
            {
                return Unauthorized("You are not Admin.");
            }

            var userList = await _accountRepo.GetAllAsync();

            return Ok(userList);
        }

        [Authorize]
        [HttpGet("get-admin-profile")]
        public async Task<IActionResult> GetAdminProfile()
        {
            var username = User.GetUsername();
            var adminUser = await _userManager.FindByNameAsync(username);
            if (adminUser == null)
            {
                return NotFound("Admin user not found");
            }
            if(!await _userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                return Unauthorized("You are not Admin.");
            }

            var adminProfile = new AdminProfileDto
            {
                Username = adminUser.UserName,
                Email = adminUser.Email,
            };

            return Ok(adminProfile);
        }

        [Authorize]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var username = User.GetUsername();
            var authuser = await _userManager.FindByNameAsync(username);
            if (authuser == null)
            {
                return Unauthorized("You are not registered.");
            }
            if(!await _userManager.IsInRoleAsync(authuser, "Admin"))
            {
                return Unauthorized("You are not Admin.");
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var appUser = new AppUser
            {
                UserName = registerDto.Username,
                Email = registerDto.Email
            };

            var createdUser = await _userManager.CreateAsync(appUser, registerDto.Password);

            if (createdUser.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(appUser, "User");
                if (roleResult.Succeeded)
                {
                    return Ok(
                        new NewUserDto
                        {
                            UserName = appUser.UserName,
                            Email = appUser.Email,
                            Token = _tokenService.CreateToken(appUser)
                        }
                    );
                }
                else
                {
                    return StatusCode(500, roleResult.Errors);
                }
            }
            else
            {
                return StatusCode(500, createdUser.Errors);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());

            if (user == null) return Unauthorized("Invalid username!");

            var result = await _signinManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded) return Unauthorized("Username not found and/or password incorrect");

            return Ok(
                new NewUserDto
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    Token = _tokenService.CreateToken(user)
                }
            );
        }

        [Authorize]
        [HttpPut("update-username/{userId}")]
        public async Task<IActionResult> UpdateUsername(string userId, [FromBody] UpdateUserDto model)
        {
            var username = User.GetUsername();
            var authuser = await _userManager.FindByNameAsync(username);
            if (authuser == null)
            {
                return Unauthorized("You are not registered.");
            }
            if(!await _userManager.IsInRoleAsync(authuser, "Admin"))
            {
                return Unauthorized("You are not Admin.");
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound("User not found");

            var result = await _userManager.SetUserNameAsync(user, model.Username);

            if (!result.Succeeded)
                return StatusCode(500, result.Errors);

            return Ok("Username updated successfully");
        }

        [Authorize]
        [HttpPut("update-admin")]
        public async Task<IActionResult> UpdateAdmin([FromBody] UpdateUserDto model)
        {
            var username = User.GetUsername();
            var authuser = await _userManager.FindByNameAsync(username);
            if (authuser == null)
            {
                return Unauthorized("You are not registered.");
            }
            if(!await _userManager.IsInRoleAsync(authuser, "Admin"))
            {
                return Unauthorized("You are not Admin.");
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verify Current Password
            var passwordValid = await _userManager.CheckPasswordAsync(authuser, model.CurrentPassword);
            if (!passwordValid)
            {
                return BadRequest("Current password is incorrect.");
            }

            // Update Username
            var setUsernameResult = await _userManager.SetUserNameAsync(authuser, model.Username);
            if (!setUsernameResult.Succeeded)
            {
                return StatusCode(500, setUsernameResult.Errors);
            }

            // Change Password
            var changePasswordResult = await _userManager.ChangePasswordAsync(authuser, model.CurrentPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                return StatusCode(500, changePasswordResult.Errors);
            }

            return Ok("Admin profile updated successfully");
        }

        [Authorize]
        [HttpPut("update-password/{userId}")]
        public async Task<IActionResult> UpdateUserPassword(string userId, [FromBody] UpdateUserDto model)
        {
            var username = User.GetUsername();
            var authuser = await _userManager.FindByNameAsync(username);
            if (authuser == null)
            {
                return Unauthorized("You are not registered.");
            }
            if(!await _userManager.IsInRoleAsync(authuser, "Admin"))
            {
                return Unauthorized("You are not Admin.");
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound("User not found");
            
            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return BadRequest("This user's password cannot be changed this way.");

            // Generate a reset token for the user
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Reset the user's password without requiring the current password
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (!result.Succeeded)
                return StatusCode(500, result.Errors);

            return Ok("Password updated successfully");
        }

        [Authorize]
        [HttpDelete("delete/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound("User not found");

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return StatusCode(500, result.Errors);

            return Ok("User deleted successfully");
        }
    }
}