using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rdds.api.Dtos.RoadData;
using rdds.api.Interfaces;
using rdds.api.Mappers;

namespace rdds.api.Controllers
{
    [Route("rddsapi/roaddata")]
    [ApiController]
    public class RoadDataController : ControllerBase
    {
        private readonly IRoadDataRepository _roadDataRepo;
        private readonly IDeviceRepository _deviceRepo;

        public RoadDataController(IRoadDataRepository roadDataRepo, IDeviceRepository deviceRepo)
        {
            _roadDataRepo = roadDataRepo;
            _deviceRepo = deviceRepo;
        }

        
        [HttpGet("{deviceMac}/{attemptId}")]
        public async Task<IActionResult> GetAllByFilter([FromRoute] string deviceMac, [FromRoute] int attemptId, [FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] float minVelocity, [FromQuery] float maxVelocity)
        {
            var roadDataModel = await _roadDataRepo.GetAllByFilterAsync(deviceMac, attemptId, startDate, endDate, minVelocity, maxVelocity);

            var roadDataDto = roadDataModel.Select(s => s.ToRoadDataDto());

            return Ok(roadDataDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var roadData = await _roadDataRepo.GetByIdAsync(id);

            if (roadData == null)
            {
                return NotFound();
            }

            return Ok(roadData.ToRoadDataDto());
        }

        [HttpPost("{attemptId}")]
        public async Task<ActionResult> CreateAsync([FromRoute] int attemptId, [FromBody] IEnumerable<CreateRoadDataDto> roadDataDtos)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var roadDataModels = roadDataDtos.Select(dto => dto.ToRoadDataFromCreate(attemptId)).ToList();
            var result = await _roadDataRepo.CreateAsync(roadDataModels);

            if (!result)
            {
                return StatusCode(500, "A problem happened while handling your request.");
            }
            
            return Ok("Success");
        }
    }
}