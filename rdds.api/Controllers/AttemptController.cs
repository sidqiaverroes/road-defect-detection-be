using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rdds.api.Data;
using rdds.api.Dtos.Attempt;
using rdds.api.Interfaces;
using rdds.api.Mappers;

namespace rdds.api.Controllers
{
    [Route("rddsapi/attempt")]
    [ApiController]
    public class AttemptController: ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAttemptRepository _attemptRepo;
        private readonly IDeviceRepository _deviceRepo;
        public AttemptController(ApplicationDbContext context, IAttemptRepository attemptRepo, IDeviceRepository deviceRepo)
        {
            _context = context;
            _attemptRepo = attemptRepo;
            _deviceRepo = deviceRepo;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var attempts = await _attemptRepo.GetAllAsync();

            var attemptDto = attempts.Select(s => s.ToAttemptDto());

            return Ok(attemptDto);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var attempt = await _attemptRepo.GetByIdAsync(id);

            if (attempt == null)
            {
                return NotFound();
            }

            return Ok(attempt.ToAttemptDto());
        }

        [Authorize]
        [HttpPost("{deviceMac}")]
        public async Task<IActionResult> Create([FromRoute] string deviceMac, CreateAttemptDto attemptDto)
        {
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

        [Authorize]
        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateAttemptDto updateDto)
        {
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