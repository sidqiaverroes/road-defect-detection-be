using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using rdds.api.Data;
using rdds.api.Dtos.Attempt;
using rdds.api.Extensions;
using rdds.api.Interfaces;
using rdds.api.Mappers;
using rdds.api.Models;

namespace rdds.api.Controllers
{
    [Route("rddsapi/attempt")]
    [ApiController]
    public class AttemptController: ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IAttemptRepository _attemptRepo;
        private readonly IDeviceRepository _deviceRepo;
        private readonly IAccountRepository _accountRepo;
        public AttemptController(ApplicationDbContext context, IAttemptRepository attemptRepo, IDeviceRepository deviceRepo, UserManager<AppUser> userManager, IAccountRepository accountRepo)
        {
            _context = context;
            _attemptRepo = attemptRepo;
            _deviceRepo = deviceRepo;
            _userManager = userManager;
            _accountRepo = accountRepo;
        }

        [EnableCors]
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var username = User.GetUsername();
            if (username == null)
            {
                return BadRequest("You are not authorized.");
            }
            var AppUser = await _userManager.FindByNameAsync(username);

            // Authorization of User Permission
            if(User.IsInRole("User") && AppUser != null)
            {
                var authUser = await _accountRepo.GetUserByIdAsync(AppUser.Id);
                var permissions = authUser.UserPermissions.Select(up => up.Permission.Id).ToList();
                var isAuthorized = permissions.Any(p => p == 301);
                if(!isAuthorized){
                    return Unauthorized("You don't have permission.");
                }
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var attempts = await _attemptRepo.GetAllAsync();

            var attemptDto = attempts.Select(s => s.ToAttemptDto());

            return Ok(attemptDto);
        }

        [EnableCors]
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var username = User.GetUsername();
            if (username == null)
            {
                return BadRequest("You are not authorized.");
            }
            var AppUser = await _userManager.FindByNameAsync(username);

            // Authorization of User Permission
            if(User.IsInRole("User") && AppUser != null)
            {
                var authUser = await _accountRepo.GetUserByIdAsync(AppUser.Id);
                var permissions = authUser.UserPermissions.Select(up => up.Permission.Id).ToList();
                var isAuthorized = permissions.Any(p => p == 302);
                if(!isAuthorized){
                    return Unauthorized("You don't have permission.");
                }
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var attempt = await _attemptRepo.GetByIdAsync(id);

            if (attempt == null)
            {
                return NotFound();
            }

            return Ok(attempt.ToAttemptDto());
        }

        [EnableCors]
        [Authorize]
        [HttpPost("{deviceMac}")]
        public async Task<IActionResult> Create([FromRoute] string deviceMac, CreateAttemptDto attemptDto)
        {
            var username = User.GetUsername();
            if (username == null)
            {
                return BadRequest("You are not authorized.");
            }
            var AppUser = await _userManager.FindByNameAsync(username);

            // Authorization of User Permission
            if(User.IsInRole("User") && AppUser != null)
            {
                var authUser = await _accountRepo.GetUserByIdAsync(AppUser.Id);
                var permissions = authUser.UserPermissions.Select(up => up.Permission.Id).ToList();
                var isAuthorized = permissions.Any(p => p == 303);
                if(!isAuthorized){
                    return Unauthorized("You don't have permission.");
                }
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existedMacAddress = await _deviceRepo.IsExistedAsync(deviceMac);

            if(existedMacAddress == null)
                return BadRequest("Device does not exist.");

            var attemptModel = attemptDto.ToAttemptFromCreate(existedMacAddress);

            var createdAttempt = await _attemptRepo.CreateAsync(attemptModel);
            
            if(createdAttempt == null)
            {
                return BadRequest("There is still ongoing attempt");
            }

            return CreatedAtAction(nameof(GetById), new {id = attemptModel}, attemptModel.ToAttemptDto());
        }

        [EnableCors]
        [Authorize]
        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateAttemptDto updateDto)
        {
            var username = User.GetUsername();
            if (username == null)
            {
                return BadRequest("You are not authorized.");
            }
            var AppUser = await _userManager.FindByNameAsync(username);

            // Authorization of User Permission
            if(User.IsInRole("User") && AppUser != null)
            {
                var authUser = await _accountRepo.GetUserByIdAsync(AppUser.Id);
                var permissions = authUser.UserPermissions.Select(up => up.Permission.Id).ToList();
                var isAuthorized = permissions.Any(p => p == 304);
                if(!isAuthorized){
                    return Unauthorized("You don't have permission.");
                }
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (id <= 0)
                return BadRequest("Invalid attempt id");
            
            var attemptModel = updateDto.ToAttemptFromUpdate();
            var attempt = await _attemptRepo.UpdateAsync(id, attemptModel);

            if(attempt == null)
            {
                return NotFound("Attempt not found");
            }

            return Ok(attempt.ToAttemptDto());
        }

        [EnableCors]
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var username = User.GetUsername();
            if (username == null)
            {
                return BadRequest("You are not authorized.");
            }
            var appUser = await _userManager.FindByNameAsync(username);

            // Authorization of User Permission
            if (User.IsInRole("User") && appUser != null)
            {
                var authUser = await _accountRepo.GetUserByIdAsync(appUser.Id);
                var permissions = authUser.UserPermissions.Select(up => up.Permission.Id).ToList();
                var isAuthorized = permissions.Any(p => p == 305);
                if (!isAuthorized)
                {
                    return Unauthorized("You don't have permission.");
                }
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (id <= 0)
                return BadRequest("Invalid attempt id");

            var attempt = await _attemptRepo.GetByIdAsync(id);
            if (attempt == null)
            {
                return NotFound("Attempt not found");
            }

            var result = await _attemptRepo.DeleteAsync(id);
            if (result == null)
            {
                return StatusCode(500, "A problem happened while handling your request.");
            }

            return Ok("Attempt deleted successfully");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize]
        [HttpPut("finish/{id}")]
        public async Task<IActionResult> Finish([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (id <= 0)
                return BadRequest("Invalid attempt id");
            if(!await _attemptRepo.IsExistedAsync(id))
            {
                return NotFound("Attempt not found");
            }
        
            var attempt = await _attemptRepo.FinishAsync(id);

            if(attempt == null)
            {
                return BadRequest("Attempt already finished");
            }

            return Ok(attempt.ToAttemptDto());
        }
    }
}