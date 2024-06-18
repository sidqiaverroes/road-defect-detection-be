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

        public Task CreateFromMqttAsync(string payload, int attemptId)
        {
            throw new NotImplementedException();
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

        public async Task<List<CalculatedData>> GetAllByFilterAsync(string deviceMac, int attemptId, string startDate, string endDate, float minVelocity, float maxVelocity)
        {
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

            if (startDateTime.HasValue && endDateTime.HasValue && endDateTime <= startDateTime)
            {
                throw new ArgumentException("End date must be greater than start date.");
            }

            var calculatedDataList = await _context.CalculatedDatas
                .Where(cd => cd.AttemptId == attemptId)
                .Where(cd => cd.Velocity >= minVelocity && cd.Velocity <= maxVelocity)
                .ToListAsync();

            var filteredCalculatedData = calculatedDataList.Where(cd =>
                (!startDateTime.HasValue || cd.Timestamp >= startDateTime.Value) &&
                (!endDateTime.HasValue || cd.Timestamp <= endDateTime.Value))
                .ToList();

            return filteredCalculatedData;
        }
    }
}