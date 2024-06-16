// AccessTypesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rdds.api.Dtos.AccessType;
using rdds.api.Interfaces;
using rdds.api.Mappers;
using rdds.api.Models;
using rdds.api.Repositories;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace rdds.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccessTypeController : ControllerBase
    {
        private readonly IAccessTypeRepository _accessTypeRepository;

        public AccessTypeController(IAccessTypeRepository accessTypeRepository)
        {
            _accessTypeRepository = accessTypeRepository;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllAccessTypes()
        {
            var accessTypes = await _accessTypeRepository.GetAllAccessTypesAsync();

            var accessTypeDto = accessTypes.Select(s => s.ToAccessTypeDto());
            return Ok(accessTypeDto);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccessTypeById(int id)
        {
            var accessType = await _accessTypeRepository.GetAccessTypeByIdAsync(id);
            if (accessType == null)
            {
                return NotFound();
            }
            return Ok(accessType.ToAccessTypeDto());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateAccessType([FromBody] CreateAccessTypeDto newAccessTypeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate the DTO
            var validationResults = newAccessTypeDto.Validate(new ValidationContext(newAccessTypeDto));
            if (validationResults.Any())
            {
                foreach (var validationResult in validationResults)
                {
                    ModelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage);
                }
                return BadRequest(ModelState);
            }

            var newAccessTypeModel = newAccessTypeDto.ToAccessTypeFromCreate();
            var createdAccessType = await _accessTypeRepository.CreateAccessTypeAsync(newAccessTypeModel);
            return CreatedAtAction(nameof(GetAccessTypeById), new { id = createdAccessType.Id }, createdAccessType);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccessType([FromRoute] int id, [FromBody] UpdateAccessTypeDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id <= 0)
                return BadRequest("Invalid access type id");

            // Validate the DTO
            var validationResults = updateDto.Validate(new ValidationContext(updateDto));
            if (validationResults.Any())
            {
                foreach (var validationResult in validationResults)
                {
                    ModelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage);
                }
                return BadRequest(ModelState);
            }

            var accessTypeModel = updateDto.ToAccessTypeFromUpdate();
            var updatedAccessType = await _accessTypeRepository.UpdateAccessTypeAsync(id, accessTypeModel);
            if (updatedAccessType == null)
            {
                return NotFound();
            }

            return Ok(updatedAccessType.ToAccessTypeDto());
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccessType(int id)
        {
            var result = await _accessTypeRepository.DeleteAccessTypeAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
