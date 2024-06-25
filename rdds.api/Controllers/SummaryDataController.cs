using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using rdds.api.Data;
using rdds.api.Dtos.SummaryData;
using rdds.api.Extensions;
using rdds.api.Interfaces;
using rdds.api.Mappers;
using rdds.api.Models;


namespace rdds.api.Controllers
{
    [Route("rddsapi/summary")]
    [ApiController]
    public class SummaryDataController: ControllerBase
    {
        private readonly ICalculatedDataRepository _calculatedDataRepository;
        private readonly IAttemptSummaryData _attemptSumDataRepository;
        private readonly IRoadCategoryRepository _roadCategoryRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IAccountRepository _accountRepo;

        public SummaryDataController(ICalculatedDataRepository calculatedDataRepository, IAttemptSummaryData attemptSumDataRepository, IRoadCategoryRepository roadCategoryRepository, UserManager<AppUser> userManager, IAccountRepository accountRepo)
        {
            _calculatedDataRepository = calculatedDataRepository;
            _attemptSumDataRepository = attemptSumDataRepository;
            _roadCategoryRepository = roadCategoryRepository;
            _userManager = userManager;
            _accountRepo = accountRepo;
        }
        
        [EnableCors]
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetSummaries()
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
                var isAuthorized = permissions.Any(p => p == 701);
                if(!isAuthorized){
                    return Unauthorized("You don't have permission.");
                }
            }

            try
            {
                var summaries = await _attemptSumDataRepository.GetAllAsync();
                var summaresDto = summaries.Select(s => s.ToSummaryDataDto());
                return Ok(summaresDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving summaries: {ex.Message}");
            }
        }

        [EnableCors]
        [Authorize]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllSummaries()
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
                var isAuthorized = permissions.Any(p => p == 702);
                if(!isAuthorized){
                    return Unauthorized("You don't have permission.");
                }
            }

            try
            {
                // Retrieve all AttemptSummaryData
                var attemptSummaryDataList = await _attemptSumDataRepository.GetAllAsync();

                if (attemptSummaryDataList == null || !attemptSummaryDataList.Any())
                {
                    return NotFound("No AttemptSummaryData found.");
                }
                var attemptSummaryDataListDto = attemptSummaryDataList.Select(a => a.ToSummaryDataDto());
                // Filter out entries without RoadCategoryId
                var validAttemptSummaryDataList = attemptSummaryDataListDto
                    .Where(a => a.Attempt.RoadCategoryId.HasValue)
                    .ToList();

                if (!validAttemptSummaryDataList.Any())
                {
                    return NotFound("No valid AttemptSummaryData found with RoadCategoryId.");
                }

                // Group data by RoadCategoryId
                var groupedData = validAttemptSummaryDataList
                    .GroupBy(a => a.Attempt.RoadCategoryId.Value);

                var deviceSummaryDataDto = new DeviceSummaryDataDto
                {
                    TotalLength = validAttemptSummaryDataList.Sum(a => a.TotalLength),
                    DeviceDatas = new List<DeviceDataDto>()
                };

                foreach (var group in groupedData)
                {
                    var roadCategoryId = group.Key;
                    var dataList = group.ToList();

                    // Get the road category name by its ID
                    var roadCategory = await _roadCategoryRepository.GetByIdAsync(roadCategoryId);

                    // Check if the road category was found
                    if (roadCategory == null)
                    {
                        return NotFound($"Road category with ID {roadCategoryId} not found.");
                    }

                    var roadCategoryName = roadCategory.Name; // Assuming the name property

                    // Calculate total lengths and percentages for each category
                    var totalLength = dataList.Sum(a => a.TotalLength);
                    var baikLength = dataList.Sum(a => a.LengthData.Baik);
                    var sedangLength = dataList.Sum(a => a.LengthData.Sedang);
                    var rusakRinganLength = dataList.Sum(a => a.LengthData.RusakRingan);
                    var rusakBeratLength = dataList.Sum(a => a.LengthData.RusakBerat);

                    var lengthData = new LengthData
                    {
                        Baik = baikLength,
                        Sedang = sedangLength,
                        RusakRingan = rusakRinganLength,
                        RusakBerat = rusakBeratLength
                    };

                    var percentageData = new PercentageData
                    {
                        Baik = baikLength / totalLength * 100,
                        Sedang = sedangLength / totalLength * 100,
                        RusakRingan = rusakRinganLength / totalLength * 100,
                        RusakBerat = rusakBeratLength / totalLength * 100
                    };

                    deviceSummaryDataDto.DeviceDatas.Add(new DeviceDataDto
                    {
                        RoadCategory = roadCategoryName,
                        TotalLength = totalLength,
                        LengthData = lengthData,
                        PercentageData = percentageData
                    });
                }

                return Ok(deviceSummaryDataDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving summaries: {ex.Message}");
            }
        }

        [EnableCors]
        [Authorize]
        [HttpGet("device/{deviceId}")]
        public async Task<IActionResult> GetDeviceSummary([FromRoute] string deviceId)
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
                var isAuthorized = permissions.Any(p => p == 703);
                if(!isAuthorized){
                    return Unauthorized("You don't have permission.");
                }
            }

            try
            {
                // Retrieve all AttemptSummaryData for the given DeviceId
                var attemptSummaryDataList = await _attemptSumDataRepository.GetAllByDeviceIdAsync(deviceId);

                if (attemptSummaryDataList == null || attemptSummaryDataList.Count == 0)
                {
                    return NotFound("No AttemptSummaryData found for the provided DeviceId.");
                }

                // Filter out entries without RoadCategoryId
                var validAttemptSummaryDataList = attemptSummaryDataList
                    .Where(a => a.Attempt.RoadCategoryId.HasValue)
                    .ToList();

                if (validAttemptSummaryDataList.Count == 0)
                {
                    return NotFound("No valid AttemptSummaryData found with RoadCategoryId for the provided DeviceId.");
                }

                // Group data by RoadCategoryId
                var groupedData = validAttemptSummaryDataList
                    .GroupBy(a => a.Attempt.RoadCategoryId.Value);

                var deviceSummaryDataDto = new DeviceSummaryDataDto
                {
                    TotalLength = validAttemptSummaryDataList.Sum(a => a.TotalLength),
                    DeviceDatas = new List<DeviceDataDto>()
                };

                foreach (var group in groupedData)
                {
                    var roadCategoryId = group.Key;
                    var dataList = group.ToList();

                    // Assuming you have a method to get the road category name by its ID
                    var roadCategory = await _roadCategoryRepository.GetByIdAsync(roadCategoryId);

                    //ADD handler

                    // Calculate total lengths and percentages for each category
                    var totalLength = dataList.Sum(a => a.TotalLength);
                    var baikLength = dataList.Sum(a => a.LengthData.Baik);
                    var sedangLength = dataList.Sum(a => a.LengthData.Sedang);
                    var rusakRinganLength = dataList.Sum(a => a.LengthData.RusakRingan);
                    var rusakBeratLength = dataList.Sum(a => a.LengthData.RusakBerat);

                    var lengthData = new LengthData
                    {
                        Baik = baikLength,
                        Sedang = sedangLength,
                        RusakRingan = rusakRinganLength,
                        RusakBerat = rusakBeratLength
                    };

                    var percentageData = new PercentageData
                    {
                        Baik = baikLength / totalLength * 100,
                        Sedang = sedangLength / totalLength * 100,
                        RusakRingan = rusakRinganLength / totalLength * 100,
                        RusakBerat = rusakBeratLength / totalLength * 100
                    };

                    deviceSummaryDataDto.DeviceDatas.Add(new DeviceDataDto
                    {
                        RoadCategory = roadCategory.Name,
                        TotalLength = totalLength,
                        LengthData = lengthData,
                        PercentageData = percentageData
                    });
                }

                return Ok(deviceSummaryDataDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving device summary: {ex.Message}");
            }
        }

        [EnableCors]
        [Authorize]
        [HttpGet("attempt/{attemptId}")]
        public async Task<IActionResult> GetAttemptSummary(int attemptId)
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
                var isAuthorized = permissions.Any(p => p == 704);
                if(!isAuthorized){
                    return Unauthorized("You don't have permission.");
                }
            }

            try
            {
                var summary = await _attemptSumDataRepository.GetByAttemptIdAsync(attemptId);

                if (summary == null)
                {
                    return NotFound($"No summary data found for AttemptId: {attemptId}");
                }

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving summary: {ex.Message}");
            }
        }

        [EnableCors]
        [Authorize]
        [HttpPost("{attemptId}")]
        public async Task<IActionResult> CreateSummary([FromRoute] int attemptId)
        {
            var username = User.GetUsername();
            if (username == null)
            {
                return BadRequest("You are not authorized.");
            }

            var AppUser = await _userManager.FindByNameAsync(username);

            // Authorization of User Permission
            if (User.IsInRole("User") && AppUser != null)
            {
                var authUser = await _accountRepo.GetUserByIdAsync(AppUser.Id);
                var permissions = authUser.UserPermissions.Select(up => up.Permission.Id).ToList();
                var isAuthorized = permissions.Any(p => p == 705);
                if (!isAuthorized)
                {
                    return Unauthorized("You don't have permission.");
                }
            }

            try
            {
                // Retrieve all CalculatedData for the given AttemptId
                var calculatedDataList = await _calculatedDataRepository.GetAllByFilterAsync(attemptId, "", "");

                if (calculatedDataList == null || calculatedDataList.Count == 0)
                {
                    return NotFound("No CalculatedData found for the provided AttemptId.");
                }

                // Initialize summary data
                var summaryData = new AttemptSummaryData
                {
                    Id = Guid.NewGuid(), // Generate a new Id for summary entry
                    AttemptId = attemptId,
                };

                // Calculate total length and profile lengths
                double totalLength = 0;
                double baikLength = 0, sedangLength = 0, rusakRinganLength = 0, rusakBeratLength = 0;

                // Calculate lengths and total length based on IRI values
                foreach (var data in calculatedDataList)
                {
                    var distance = CalculateDistance(
                        data.CoordinateStart.Latitude,
                        data.CoordinateStart.Longitude,
                        data.CoordinateEnd.Latitude,
                        data.CoordinateEnd.Longitude);

                    totalLength += distance;

                    // Update profile lengths based on IRI values
                    switch (data.IRI.EuclideanProfile)
                    {
                        case "Rusak Berat":
                            rusakBeratLength += distance;
                            break;
                        case "Rusak Ringan":
                            rusakRinganLength += distance;
                            break;
                        case "Sedang":
                            sedangLength += distance;
                            break;
                        default:
                            baikLength += distance;
                            break;
                    }
                }

                // Set summary data
                summaryData.TotalLength = totalLength;
                summaryData.LengthData = new LengthData
                {
                    Baik = baikLength,
                    Sedang = sedangLength,
                    RusakRingan = rusakRinganLength,
                    RusakBerat = rusakBeratLength
                };

                // Calculate percentage data
                summaryData.PercentageData = new PercentageData
                {
                    Baik = totalLength == 0 ? 0 : baikLength / totalLength * 100,
                    Sedang = totalLength == 0 ? 0 : sedangLength / totalLength * 100,
                    RusakRingan = totalLength == 0 ? 0 : rusakRinganLength / totalLength * 100,
                    RusakBerat = totalLength == 0 ? 0 : rusakBeratLength / totalLength * 100
                };

                // Create the summary data in the repository
                var createdSumData = await _attemptSumDataRepository.CreateAsync(summaryData);

                return Ok(createdSumData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating summary: {ex.Message}");
            }
        }

        // Helper method to calculate distance between two coordinates
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371.0; // Earth radius in kilometers

            double dLat = Math.PI * (lat2 - lat1) / 180.0;
            double dLon = Math.PI * (lon2 - lon1) / 180.0;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(Math.PI * lat1 / 180.0) * Math.Cos(Math.PI * lat2 / 180.0) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double distance = R * c; // Distance in kilometers
            return distance * 1000; // Convert to meters
        }
    }
}