using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using rdds.api.Dtos.CalculatedData;
using rdds.api.Interfaces;
using rdds.api.Models;
using rdds.api.Mappers;

namespace rdds.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalculatedDataController : ControllerBase
    {
        private readonly ICalculatedDataRepository _calculatedDataRepository;
        private readonly IDeviceRepository _deviceRepo;
        private readonly IAttemptRepository _attemptRepo;

        public CalculatedDataController(ICalculatedDataRepository calculatedDataRepository, IDeviceRepository deviceRepo, IAttemptRepository attemptRepo)
        {
            _calculatedDataRepository = calculatedDataRepository;
            _deviceRepo = deviceRepo;
            _attemptRepo = attemptRepo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CalculatedDataDto>>> GetAllByFilter([FromRoute] string deviceMac, [FromRoute] int attemptId, string startDate = "", string endDate = "", float minVelocity = 0, float maxVelocity = 0)
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

            var calculatedData = await _calculatedDataRepository.GetAllByFilterAsync(deviceMac, attemptId, startDate, endDate, minVelocity, maxVelocity);
            var calculatedDataDtos = calculatedData.Select(cd => cd.ToCalculatedDataDto());
            return Ok(calculatedDataDtos);
        }

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

            try{

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
