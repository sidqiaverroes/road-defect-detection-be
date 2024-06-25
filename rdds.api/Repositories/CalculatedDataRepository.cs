using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using rdds.api.Data;
using rdds.api.Dtos.CalculatedData;
using rdds.api.Interfaces;
using rdds.api.Mappers;
using rdds.api.Models;

namespace rdds.api.Repositories
{
    public class CalculatedDataRepository : ICalculatedDataRepository
    {
        private readonly ApplicationDbContext _context;

        public CalculatedDataRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateAsync(IEnumerable<CreateCalculatedDataDto> calculatedDataDtos, int attemptId)
        {
            try
            {
                var calculatedDataModels = calculatedDataDtos.Select(dto => dto.ToCalculatedDataFromCreate(attemptId)).ToList();
                await _context.CalculatedDatas.AddRangeAsync(calculatedDataModels);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task CreateFromMqttAsync(List<SensorData> sensorDataList, InternationalRoughnessIndex IRI, int attemptId)
        {
            try
            {
                var calculatedDataList = new List<CreateCalculatedDataDto>();

                foreach (var sensorData in sensorDataList)
                {
                    // Map SensorData to RoadData
                    var calcualtedData = new CreateCalculatedDataDto
                    {
                        IRI = IRI,
                        Velocity = sensorData.velocity,
                        Coordinate = new Coordinate
                        {
                            Latitude = sensorData.latitude,
                            Longitude = sensorData.longitude,
                        },
                        Timestamp = sensorData.timestamp,
                        AttemptId = attemptId
                    };

                    calculatedDataList.Add(calcualtedData);
                }

                // Convert CreateRoadDataDto to RoadData entities
                var calculatedDataModels = calculatedDataList.Select(dto => dto.ToCalculatedDataFromCreate(attemptId)).ToList();

                // Add range of roadDataModels to context and save changes
                await _context.CalculatedDatas.AddRangeAsync(calculatedDataModels);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> DeleteAllAsync()
        {
            try
            {
                var calculatedDataEntities = await _context.CalculatedDatas.ToListAsync();
                _context.CalculatedDatas.RemoveRange(calculatedDataEntities);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting all CalculatedData: {ex.Message}");
                return false;
            }
        }

        public async Task<List<CalculatedData>> GetAllByFilterAsync(int? attemptId, string startDate = "", string endDate = "")
        {
            // Parse dates if provided
            DateTime? startDateTime = string.IsNullOrEmpty(startDate) ? (DateTime?)null : DateTime.Parse(startDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;
            DateTime? endDateTime = string.IsNullOrEmpty(endDate) ? (DateTime?)null : DateTime.Parse(endDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;

            if (startDateTime.HasValue && endDateTime.HasValue && endDateTime < startDateTime)
            {
                throw new ArgumentException("End date must be greater than start date.");
            }

            var query = _context.CalculatedDatas.AsNoTracking().AsQueryable();

            if (attemptId.HasValue)
            {
                query = query.Where(cd => cd.AttemptId == attemptId.Value);
            }

            if (startDateTime.HasValue)
            {
                query = query.Where(cd => cd.Timestamp.Date >= startDateTime.Value);
            }

            if (endDateTime.HasValue)
            {
                query = query.Where(cd => cd.Timestamp.Date <= endDateTime.Value);
            }

            // Apply pagination
            var calculatedDataList = await query.ToListAsync();

            return calculatedDataList;
        }

    }
}