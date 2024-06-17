using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            catch (ArgumentException ex)
            {
                throw;
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

        public async Task<List<RoadData>> GetAllByFilterAsync(string deviceMac, int attemptId, string startDate = "", string endDate = "", float minVelocity = 0, float maxVelocity = 0)
        {
            // Parse startDate and endDate strings to DateTime objects
            DateTime? startDateTime = null;
            DateTime? endDateTime = null;

            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParseExact(startDate, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedStartDate))
            {
                startDateTime = parsedStartDate;
            }

            if (!string.IsNullOrEmpty(endDate) && DateTime.TryParseExact(endDate, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedEndDate))
            {
                endDateTime = parsedEndDate;
            }

            // Validate end date is greater than start date
            if (startDateTime.HasValue && endDateTime.HasValue && endDateTime <= startDateTime)
            {
                throw new ArgumentException("End date must be greater than start date.");
            }

            var roadDataList = await _context.RoadDatas
                .Where(r => r.AttemptId == attemptId)
                .Where(r => r.Velocity >= minVelocity && r.Velocity <= maxVelocity)
                .ToListAsync();

            // Filter based on parsed startDateTime and endDateTime
            var filteredRoadData = roadDataList.Where(r =>
                (!startDateTime.HasValue || r.Timestamp >= startDateTime.Value) &&
                (!endDateTime.HasValue || r.Timestamp <= endDateTime.Value))
                .ToList();

            return filteredRoadData;
        }
    }
}