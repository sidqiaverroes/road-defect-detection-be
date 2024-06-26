using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using rdds.api.Data;
using rdds.api.Dtos.RoadData;
using rdds.api.Interfaces;
using rdds.api.Mappers;
using rdds.api.Models;

namespace rdds.api.Repositories
{
    public class RoadDataRepository : IRoadDataRepository
    {
        private readonly ApplicationDbContext _context;

        public RoadDataRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateAsync(IEnumerable<CreateRoadDataDto> roadDataDtos, int attemptId)
        {
            try
            {
                // Convert CreateRoadDataDto to RoadData entities
                var roadDataModels = roadDataDtos.Select(dto => dto.ToRoadDataFromCreate(attemptId)).ToList();

                // Add range of roadDataModels to context and save changes
                await _context.RoadDatas.AddRangeAsync(roadDataModels);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (DbUpdateException ex)
            {
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> DeleteAllAsync()
        {
            try
            {
                var roadDataEntities = await _context.RoadDatas.ToListAsync();
                _context.RoadDatas.RemoveRange(roadDataEntities);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error deleting all RoadData: {ex.Message}");
                return false;
            }
        }

        public async Task<List<RoadData>> GetAllByFilterAsync(int? attemptId, string startDate = "", string endDate = "")
        {
            DateTime? startDateTime = null;
            DateTime? endDateTime = null;

            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedStartDate))
            {
                startDateTime = parsedStartDate.Date; // Extracts the date part without time
            }

            if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedEndDate))
            {
                endDateTime = parsedEndDate.Date; // Extracts the date part without time
            }

            if (startDateTime.HasValue && endDateTime.HasValue && endDateTime < startDateTime)
            {
                throw new ArgumentException("End date must be greater than start date.");
            }

            var query = _context.RoadDatas.AsQueryable();

            if (attemptId.HasValue)
            {
                query = query.Where(rd => rd.AttemptId == attemptId.Value);
            }

            if (startDateTime.HasValue)
            {
                query = query.Where(rd => rd.Timestamp.Date >= startDateTime.Value);
            }

            if (endDateTime.HasValue)
            {
                query = query.Where(rd => rd.Timestamp.Date <= endDateTime.Value);
            }

            query = query.OrderBy(cd => cd.Timestamp);

            var roadDataList = await query.ToListAsync();

            return roadDataList;
        }


        public async Task CreateFromMqttAsync(List<SensorData> sensorDataList, int attemptId)
        {
            try
            {
                var roadDataList = new List<CreateRoadDataDto>();

                foreach (var sensorData in sensorDataList)
                {
                    // Map SensorData to RoadData
                    var roadData = new CreateRoadDataDto
                    {
                        Roll = (float)sensorData.roll,
                        Pitch = (float)sensorData.pitch,
                        Euclidean = (float)sensorData.euclidean,
                        Velocity = (float)sensorData.velocity,
                        Timestamp = sensorData.timestamp,
                        Latitude = sensorData.latitude,
                        Longitude = sensorData.longitude
                    };

                    roadDataList.Add(roadData);
                }

                // Convert CreateRoadDataDto to RoadData entities
                var roadDataModels = roadDataList.Select(dto => dto.ToRoadDataFromCreate(attemptId)).ToList();

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    await _context.RoadDatas.AddRangeAsync(roadDataModels);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }

            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}