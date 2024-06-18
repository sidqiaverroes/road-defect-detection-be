using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rdds.api.Dtos.RoadData;
using rdds.api.Interfaces;
using rdds.api.Mappers;
using rdds.api.Models;

namespace rdds.api.Controllers
{
    [Route("rddsapi/roaddata")]
    [ApiController]
    public class RoadDataController : ControllerBase
    {
        private readonly IRoadDataRepository _roadDataRepo;
        private readonly IDeviceRepository _deviceRepo;
        private readonly IAttemptRepository _attemptRepo;

        public RoadDataController(IRoadDataRepository roadDataRepo, IDeviceRepository deviceRepo, IAttemptRepository attemptRepo)
        {
            _roadDataRepo = roadDataRepo;
            _deviceRepo = deviceRepo;
            _attemptRepo = attemptRepo;
        }

        [HttpGet("{deviceMac}")]
        public async Task<IActionResult> GetAllByFilter([FromRoute] string deviceMac, [FromQuery] int? attemptId = null, [FromQuery] string startDate = "", [FromQuery] string endDate = "", [FromQuery] float minVelocity = 0, [FromQuery] float maxVelocity = 0)
        {
            // Check if the device exists
            var device = await _deviceRepo.GetByMacAddressAsync(deviceMac);
            if (device == null)
            {
                return BadRequest($"Device with MAC address '{deviceMac}' not found.");
            }

            List<RoadDataDto> roadDataDtoList = new List<RoadDataDto>();

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
                var roadDataModels = await _roadDataRepo.GetAllByFilterAsync(attemptId.Value, startDate, endDate, minVelocity, maxVelocity);
                roadDataDtoList = roadDataModels.Select(rd => rd.ToRoadDataDto()).ToList();
            }
            else
            {
                foreach(var attempt in device.Attempts)
                {
                    // Fetch road data without filtering by attempt, but for the device
                    var roadDataModels = await _roadDataRepo.GetAllByFilterAsync(attempt.Id, startDate, endDate, minVelocity, maxVelocity);
                    roadDataDtoList.AddRange(roadDataModels.Select(rd => rd.ToRoadDataDto()));
                }
            }

            return Ok(roadDataDtoList);
        }

        
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("{attemptId}")]
        public async Task<ActionResult> CreateAsync([FromRoute] int attemptId, [FromBody] IEnumerable<CreateRoadDataDto> roadDataDtos)
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
                var result = await _roadDataRepo.CreateAsync(roadDataDtos, attemptId);

                if (!result)
                {
                    return StatusCode(500, "Failed to create road data.");
                }
                
                return Ok("Success");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpDelete]
        public async Task<IActionResult> DeleteAllRoadData()
        {
            try
            {
                var success = await _roadDataRepo.DeleteAllAsync();
                if (success)
                {
                    return NoContent(); // 204 No Content
                }
                else
                {
                    return StatusCode(500); // Server error
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
