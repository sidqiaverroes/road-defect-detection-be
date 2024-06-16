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

        [HttpGet("{deviceMac}/{attemptId}")]
        public async Task<IActionResult> GetAllByFilter([FromRoute] string deviceMac, [FromRoute] int attemptId, [FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] float minVelocity, [FromQuery] float maxVelocity)
        {
            // Check if the device exists
            var device = await _deviceRepo.GetByMacAddressAsync(deviceMac);
            if (device == null)
            {
                return BadRequest($"Device with MAC address '{deviceMac}' not found.");
            }

            // Check if the attempt exists
            var attemptExists = await _attemptRepo.IsExistedAsync(attemptId);
            if (!attemptExists)
            {
                return BadRequest($"Attempt with ID '{attemptId}' not found.");
            }

            // Check if the attempt is related to the device
            var isAttemptRelated = await _attemptRepo.IsAttemptRelatedToDevice(attemptId, deviceMac);
            if (!isAttemptRelated)
            {
                return BadRequest($"Attempt with ID '{attemptId}' is not related to device with MAC address '{deviceMac}'.");
            }

            // If both device and attempt exist, proceed to fetch road data
            var roadDataModel = await _roadDataRepo.GetAllByFilterAsync(deviceMac, attemptId, startDate, endDate, minVelocity, maxVelocity);

            var roadDataDto = roadDataModel.Select(s => s.ToRoadDataDto());

            return Ok(roadDataDto);
        }

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
    }
}
