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
                if (sensorDataList == null || sensorDataList.Count == 0)
                {
                    throw new ArgumentException("Sensor data list cannot be null or empty.");
                }

                // Calculate average velocity
                float totalVelocity = sensorDataList.Sum(sd => sd.velocity);
                float averageVelocity = totalVelocity / sensorDataList.Count;

                // Get the first and last sensor data to determine start and end coordinates
                var firstSensorData = sensorDataList.First();
                var lastSensorData = sensorDataList.Last();

                // Create CalculatedData entity with start and end coordinates
                var calculatedData = new CalculatedData
                {
                    IRI = IRI,
                    Velocity = averageVelocity,
                    CoordinateStart = new Coordinate
                    {
                        Latitude = firstSensorData.latitude,
                        Longitude = firstSensorData.longitude
                    },
                    CoordinateEnd = new Coordinate
                    {
                        Latitude = lastSensorData.latitude,
                        Longitude = lastSensorData.longitude
                    },
                    Timestamp = DateTime.Now, // Assuming the timestamp of the first sensor data
                    AttemptId = attemptId
                };

                // Add CalculatedData entity to context and save changes
                await _context.CalculatedDatas.AddAsync(calculatedData);
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

        public async Task<List<CalculatedData>> GetAllByFilterAsync(int? attemptId, string startDate, string endDate)
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

            var query = _context.CalculatedDatas.AsQueryable();

            query = query.Where(cd => cd.AttemptId == attemptId);


            if (startDateTime.HasValue)
            {
                query = query.Where(cd => cd.Timestamp.Date >= startDateTime.Value);
            }

            if (endDateTime.HasValue)
            {
                query = query.Where(cd => cd.Timestamp.Date <= endDateTime.Value);
            }

            var calculatedDataList = await query.ToListAsync();

            return calculatedDataList;
        }
    }
}