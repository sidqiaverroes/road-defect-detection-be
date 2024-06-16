using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rdds.api.Data;
using rdds.api.Dtos.RoadCategory;
using rdds.api.Interfaces;
using rdds.api.Mappers;

namespace rdds.api.Controllers
{
    [Route("rddsapi/roadcategory")]
    [ApiController]
    public class RoadCategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IRoadCategoryRepository _roadCategoryRepo;

        public RoadCategoryController(ApplicationDbContext context, IRoadCategoryRepository roadCategoryRepo)
        {
            _context = context;
            _roadCategoryRepo = roadCategoryRepo;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var roadCategories = await _roadCategoryRepo.GetAllAsync();

            var roadCategoryDtos = roadCategories.Select(rc => rc.ToRoadCategoryDto());

            return Ok(roadCategoryDtos);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var roadCategory = await _roadCategoryRepo.GetByIdAsync(id);

            if (roadCategory == null)
            {
                return NotFound();
            }

            return Ok(roadCategory.ToRoadCategoryDto());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoadCategoryDto newRoadCategoryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var roadCategoryModel = newRoadCategoryDto.ToRoadCategoryFromCreate();
            var createdRoadCategory = await _roadCategoryRepo.CreateAsync(roadCategoryModel);

            return CreatedAtAction(nameof(GetById), new { id = createdRoadCategory.Id }, createdRoadCategory.ToRoadCategoryDto());
        }

        [Authorize]
        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateRoadCategoryDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (id <= 0)
                return BadRequest("Invalid road category id");

            var roadCategoryModel = updateDto.ToRoadCategoryFromUpdate();
            var roadCategory = await _roadCategoryRepo.UpdateAsync(id, roadCategoryModel);

            if (roadCategory == null)
            {
                return NotFound("Road category not found");
            }

            return Ok(roadCategory.ToRoadCategoryDto());
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (id <= 0)
                return BadRequest("Invalid road category id");

            var success = await _roadCategoryRepo.DeleteAsync(id);
            if (!success)
            {
                return NotFound("Road category not found");
            }

            return NoContent();
        }
    }
}
