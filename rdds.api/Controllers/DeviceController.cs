using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using rdds.api.CustomValidation;
using rdds.api.Data;
using rdds.api.Dtos.Device;
using rdds.api.Extensions;
using rdds.api.Interfaces;
using rdds.api.Mappers;
using rdds.api.Models;

namespace rdds.api.Controllers
{
    [Route("rddsapi/device")]
    [ApiController]
    public class DeviceController: ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IDeviceRepository _deviceRepo;
        private readonly UserManager<AppUser> _userManager;
        private readonly IAccountRepository _accountRepo;
        public DeviceController(ApplicationDbContext context, IDeviceRepository deviceRepo, UserManager<AppUser> userManager, IAccountRepository accountRepo)
        {
            _deviceRepo = deviceRepo;
            _context = context;
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
                var isAuthorized = permissions.Any(p => p == 201);
                if(!isAuthorized){
                    return Unauthorized("You don't have permission.");
                }
            }
            
            var devices = await _deviceRepo.GetAllAsync();
            var deviceDto = devices.Select(s => s.ToDeviceDto());

            return Ok(deviceDto);
        }

        [EnableCors]
        [Authorize]
        [HttpGet("{mac}")]
        public async Task<IActionResult> GetByMacAddress([FromRoute] string mac)
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
                var isAuthorized = permissions.Any(p => p == 202);
                if(!isAuthorized){
                    return Unauthorized("You don't have permission.");
                }
            }
            
            var device = await _deviceRepo.GetByMacAddressAsync(mac);
            
            if(device == null)
            {
                return NotFound();
            }

            return Ok(device.ToDeviceDto());
        }

        [EnableCors]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDeviceDto deviceDto)
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
                var isAuthorized = permissions.Any(p => p == 203);
                if(!isAuthorized){
                    return Unauthorized("You don't have permission.");
                }
            }

            //Model Validation
            if(deviceDto == null)
            {
                return BadRequest();
            }
            if(!MacAddressValidator.IsMacAddressValid(deviceDto.MacAddress))
            {
                return BadRequest("The Mac Address format is invalid.");
            }
            if(deviceDto.MacAddress.IsNullOrEmpty())
            {
                return BadRequest("Mac Address cannot be empty.");
            }
            if(deviceDto.DeviceName.IsNullOrEmpty())
            {
                return BadRequest("Device Name cannot be empty.");
            }

            //Check is existed
            var existedMacAddress = await _deviceRepo.IsExistedAsync(deviceDto.MacAddress);
            if(existedMacAddress != null)
            {
                return BadRequest("Device with the corresponding mac address is already existed.");
            }

            //Write function to db
            var deviceModel = deviceDto.ToDeviceFromCreateDto();
            deviceModel.AppUserId = AppUser.Id;
            await _deviceRepo.CreateAsync(deviceModel);
            return CreatedAtAction(nameof(GetByMacAddress), new {mac = deviceModel.MacAddress}, deviceModel.ToDeviceDto());
        }

        [EnableCors]
        [Authorize]
        [HttpPut("{mac}")]
        public async Task<IActionResult> Update([FromRoute] string mac, [FromBody] UpdateDeviceDto updateDeviceDto)
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
                var isAuthorized = permissions.Any(p => p == 204);
                if(!isAuthorized){
                    return Unauthorized("You don't have permission.");
                }
            }

            var existingDevice = await _deviceRepo.GetByMacAddressAsync(mac);
            if (existingDevice == null)
            {
                return NotFound();
            }
            
            var deviceModel = updateDeviceDto.ToDeviceFromUpdate();
            // Save changes
            var device = await _deviceRepo.UpdateAsync(mac, deviceModel);

            return Ok(device.ToDeviceDto());
        }

        [EnableCors]
        [Authorize]
        [HttpDelete("{mac}")]
        public async Task<IActionResult> Delete([FromRoute] string mac)
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
                var isAuthorized = permissions.Any(p => p == 205);
                if(!isAuthorized){
                    return Unauthorized("You don't have permission.");
                }
            }

            var existingDevice = await _deviceRepo.GetByMacAddressAsync(mac);
            if (existingDevice == null)
            {
                return NotFound();
            }

            // Delete the device
            await _deviceRepo.DeleteAsync(mac);

            return NoContent();
        }
    }
}