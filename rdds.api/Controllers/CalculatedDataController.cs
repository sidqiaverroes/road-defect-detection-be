using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using rdds.api.Dtos.CalculatedData;
using rdds.api.Interfaces;
using rdds.api.Models;
using rdds.api.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using rdds.api.Extensions;

namespace rdds.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalculatedDataController : ControllerBase
    {
        private readonly ICalculatedDataRepository _calculatedDataRepository;
        private readonly IDeviceRepository _deviceRepo;
        private readonly IAttemptRepository _attemptRepo;
        private readonly UserManager<AppUser> _userManager;
        private readonly IAccountRepository _accountRepo;

        public CalculatedDataController(ICalculatedDataRepository calculatedDataRepository, IDeviceRepository deviceRepo, IAttemptRepository attemptRepo, UserManager<AppUser> userManager, IAccountRepository accountRepo)
        {
            _calculatedDataRepository = calculatedDataRepository;
            _deviceRepo = deviceRepo;
            _attemptRepo = attemptRepo;
            _userManager = userManager;
            _accountRepo = accountRepo;
        }

        [Authorize]
        [HttpGet("{deviceMac}")]
        public async Task<IActionResult> GetAllByFilter([FromRoute] string deviceMac, [FromQuery] int? attemptId = null, [FromQuery] string startDate = "", [FromQuery] string endDate = "", [FromQuery] float minVelocity = 0, [FromQuery] float maxVelocity = 0)
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
                var isAuthorized = permissions.Any(p => p == 501);
                if(!isAuthorized){
                    return Unauthorized("You don't have permission.");
                }
            }

            // Check if the device exists
            var device = await _deviceRepo.GetByMacAddressAsync(deviceMac);
            if (device == null)
            {
                return BadRequest($"Device with MAC address '{deviceMac}' not found.");
            }

            List<CalculatedDataDto> calculatedDataDtoList = new List<CalculatedDataDto>();

            if (attemptId.HasValue)
            {
                // Check if the attempt exists
                var attemptExists = await _attemptRepo.IsExistedAsync(attemptId.Value);
                if (!attemptExists)
                {
                    return BadRequest($"Attempt with ID '{attemptId}' not found.");
                }

                // Check if the attempt is related to the device
                var isAttemptRelated = await _attemptRepo.IsAttemptRelatedToDevice(attemptId.Value, deviceMac);
                if (!isAttemptRelated)
                {
                    return BadRequest($"Attempt with ID '{attemptId}' is not related to device with MAC address '{deviceMac}'.");
                }

                // Fetch road data for the specific attempt and filters
                var roadDataModels = await _calculatedDataRepository.GetAllByFilterAsync(attemptId.Value, startDate, endDate, minVelocity, maxVelocity);
                calculatedDataDtoList = roadDataModels.Select(rd => rd.ToCalculatedDataDto()).ToList();
            }
            else
            {
                foreach(var attempt in device.Attempts)
                {
                    // Fetch road data without filtering by attempt, but for the device
                    var roadDataModels = await _calculatedDataRepository.GetAllByFilterAsync(attempt.Id, startDate, endDate, minVelocity, maxVelocity);
                    calculatedDataDtoList.AddRange(roadDataModels.Select(rd => rd.ToCalculatedDataDto()));
                }
            }

            return Ok(calculatedDataDtoList);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] IEnumerable<CreateCalculatedDataDto> createCalculatedDataDtos, int attemptId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the attempt exists
            var attemptExists = await _attemptRepo.IsExistedAsync(attemptId);
            if (!attemptExists)
            {
                return BadRequest($"Attempt with ID '{attemptId}' not found.");
            }

            try
            {

                var result = await _calculatedDataRepository.CreateAsync(createCalculatedDataDtos, attemptId);
                if (!result)
                {
                    return StatusCode(500, "A problem happened while handling your request.");
                }
            }
            catch (Exception ex)
            {   
                return StatusCode(500, $"Error create new calculated data: {ex.Message}");
            }
            

            return Ok();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize]
        [HttpDelete]
        public async Task<ActionResult> DeleteAll()
        {
            var result = await _calculatedDataRepository.DeleteAllAsync();
            if (!result)
            {
                return StatusCode(500, "A problem happened while handling your request.");
            }

            return Ok();
        }
    }
}
