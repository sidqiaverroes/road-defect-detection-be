using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using rdds.api.Dtos.Account;
using rdds.api.Extensions;
using rdds.api.Interfaces;
using rdds.api.Mappers;
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
        private readonly IAuthenticationService _authenticationService;
        private readonly ITokenBlacklistService _tokenBlacklistService;
        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, SignInManager<AppUser> signInManager, IAccountRepository accountRepo, IAuthenticationService authenticationService, ITokenBlacklistService blacklistService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signinManager = signInManager;
            _accountRepo = accountRepo;
            _authenticationService = authenticationService;
            _tokenBlacklistService = blacklistService;
        }
        
        [EnableCors]
        [Authorize("Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {

            var usersWithRole = await _accountRepo.GetAllAsync();

            if (usersWithRole == null || !usersWithRole.Any())
            {
                return NotFound("No users found with the specified role.");
            }

            return Ok(usersWithRole);
        }

        [EnableCors]
        [Authorize("Admin")]
        [HttpGet("get-admin-profile")]
        public async Task<IActionResult> GetAdminProfile()
        {
            var username = User.GetUsername();
            var adminUser = await _userManager.FindByNameAsync(username);

            var adminProfile = new AdminProfileDto
            {
                Username = adminUser.UserName,
                Email = adminUser.Email,
            };

            return Ok(adminProfile);
        }

        [EnableCors]
        [Authorize]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var user = await _accountRepo.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user.ToUserDto());
        }

        [EnableCors]
        [Authorize("Admin")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newUser = await _userManager.FindByNameAsync(registerDto.Username);
            if(newUser != null)
            {
                return BadRequest("Username already taken.");
            }

            var appUser = new AppUser
            {
                UserName = registerDto.Username,
                Email = registerDto.Email,
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
                            Token = await _tokenService.CreateToken(appUser)
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

        [EnableCors]
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
                    Token = await _tokenService.CreateToken(user)
                }
            );
        }

        [EnableCors]
        [Authorize("Admin")]
        [HttpPut("update-admin")]
        public async Task<IActionResult> UpdateAdmin([FromBody] UpdateAdminDto model)
        {
            var username = User.GetUsername();
            var authuser = await _userManager.FindByNameAsync(username);
            if (authuser == null)
            {
                return Unauthorized("You are not registered.");
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

        [EnableCors]
        [Authorize("Admin")]
        [HttpPut("update-user-details/{userId}")]
        public async Task<IActionResult> UpdateUserDetails(string userId, [FromBody] UpdateUserDetailsDto model)
        {

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Update Password
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (!User.IsInRole("Admin"))
                {
                    return BadRequest("This user's password cannot be changed this way.");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

                if (!result.Succeeded)
                {
                    return StatusCode(500, result.Errors);
                }
            }

            // Update Username
            if (!string.IsNullOrEmpty(model.NewUsername))
            {
                var result = await _userManager.SetUserNameAsync(user, model.NewUsername);

                if (!result.Succeeded)
                {
                    return StatusCode(500, result.Errors);
                }
            }

            // Update Permissions based on PermissionId list
            try
            {
                var permissionIds = model.PermissionId ?? new List<int>();
                await _accountRepo.UpdateUserPermissionsAsync(userId, permissionIds);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
            

            return Ok("User details updated successfully");
        }

        [EnableCors]
        [Authorize("Admin")]
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

        [Authorize]
        [HttpPost("logout")]
        [EnableCors]
        public IActionResult Logout()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            _tokenBlacklistService.AddTokenToBlacklist(token);
            Console.WriteLine(token);

            return Ok(new { message = "Successfully logged out" });
        }
    }
}