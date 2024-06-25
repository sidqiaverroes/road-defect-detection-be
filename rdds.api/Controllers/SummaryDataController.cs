using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using rdds.api.Data;
using rdds.api.Dtos.SummaryData;
using rdds.api.Interfaces;
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

        public SummaryDataController(ICalculatedDataRepository calculatedDataRepository, IAttemptSummaryData attemptSumDataRepository, IRoadCategoryRepository roadCategoryRepository)
        {
            _calculatedDataRepository = calculatedDataRepository;
            _attemptSumDataRepository = attemptSumDataRepository;
            _roadCategoryRepository = roadCategoryRepository;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllSummaries()
        {
            try
            {
                var summaries = await _attemptSumDataRepository.GetAllAsync();
                return Ok(summaries);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving summaries: {ex.Message}");
            }
        }

        [HttpGet("device/{deviceId}")]
        public async Task<IActionResult> GetDeviceSummary([FromRoute] string deviceId)
        {
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

        [HttpGet("attempt/{attemptId}")]
        public async Task<IActionResult> GetSummaryByAttemptId(int attemptId)
        {
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

        [HttpPost("{attemptId}")]
        public async Task<IActionResult> CreateSummary([FromRoute] int attemptId)
        {
            try
            {
                // Retrieve all CalculatedData for the given AttemptId
                var calculatedDataList = await _calculatedDataRepository.GetAllByFilterAsync(attemptId, "" , "");

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

                for (int i = 0; i < calculatedDataList.Count - 1; i++)
                {
                    var current = calculatedDataList[i];
                    var next = calculatedDataList[i + 1];

                    // Calculate distance between consecutive coordinates
                    double distance = CalculateDistance(
                        current.Coordinate.Latitude,
                        current.Coordinate.Longitude,
                        next.Coordinate.Latitude,
                        next.Coordinate.Longitude);

                    // Update total length
                    totalLength += distance;

                    // Update profile lengths based on IRI values
                    if (current.IRI.EuclideanProfile == "Rusak Berat")
                    {
                        rusakBeratLength += distance;
                    }
                    else if (current.IRI.EuclideanProfile == "Rusak Ringan")
                    {
                        rusakRinganLength += distance;
                    }
                    else if (current.IRI.EuclideanProfile == "Sedang")
                    {
                        sedangLength += distance;
                    }
                    else
                    {
                        baikLength += distance;
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
                    Baik = baikLength / totalLength * 100,
                    Sedang = sedangLength / totalLength * 100,
                    RusakRingan = rusakRinganLength / totalLength * 100,
                    RusakBerat = rusakBeratLength / totalLength * 100
                };

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