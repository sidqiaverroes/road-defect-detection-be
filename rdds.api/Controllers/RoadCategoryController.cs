using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using rdds.api.Data;
using rdds.api.Dtos.RoadCategory;
using rdds.api.Extensions;
using rdds.api.Interfaces;
using rdds.api.Mappers;
using rdds.api.Models;

namespace rdds.api.Controllers
{
    [Route("rddsapi/roadcategory")]
    [ApiController]
    public class RoadCategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IRoadCategoryRepository _roadCategoryRepo;
        private readonly UserManager<AppUser> _userManager;
        private readonly IAccountRepository _accountRepo;

        public RoadCategoryController(ApplicationDbContext context, IRoadCategoryRepository roadCategoryRepo, UserManager<AppUser> userManager, IAccountRepository accountRepo)
        {
            _context = context;
            _roadCategoryRepo = roadCategoryRepo;
            _userManager = userManager;
            _accountRepo = accountRepo;
        }

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
                var isAuthorized = permissions.Any(p => p == 601);
                if(!isAuthorized){
                    return Unauthorized("You don't have permission.");
                }
            }

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
                var isAuthorized = permissions.Any(p => p == 602);
                if(!isAuthorized){
                    return Unauthorized("You don't have permission.");
                }
            }

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
                var isAuthorized = permissions.Any(p => p == 603);
                if(!isAuthorized){
                    return Unauthorized("You don't have permission.");
                }
            }

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
                var isAuthorized = permissions.Any(p => p == 604);
                if(!isAuthorized){
                    return Unauthorized("You don't have permission.");
                }
            }
            
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
                var isAuthorized = permissions.Any(p => p == 605);
                if(!isAuthorized){
                    return Unauthorized("You don't have permission.");
                }
            }

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
