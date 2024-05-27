using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using rdds.api.Data;
using rdds.api.Interfaces;
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

        public async Task<bool> CreateAsync(List<RoadData> roadDataModel)
        {
            await _context.RoadDatas.AddRangeAsync(roadDataModel);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<RoadData>> GetAllByFilterAsync(string deviceMac, int attemptId, string startDate="", string endDate="", float minVelocity=0, float maxVelocity=0)
        {
            var query = _context.RoadDatas.Where(rd => rd.AttemptId == attemptId)
            .Include(rd => rd.Attempt).Where(a => a.Attempt.DeviceId == deviceMac);

            // Apply optional filters
            if (!string.IsNullOrEmpty(startDate))
            {
                DateTime parsedStartDate;
                if (DateTime.TryParseExact(startDate, "yy-MM-dd HH:mm:ss.ff", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedStartDate))
                {
                    query = query.Where(rd => DateTime.ParseExact(rd.Timestamp, "yy-MM-dd HH:mm:ss.ff", CultureInfo.InvariantCulture) >= parsedStartDate);
                }
                else
                {
                    // Handle invalid startDate format
                    return null; // Or throw an exception, return an empty list, etc.
                }
            }

            if (!string.IsNullOrEmpty(endDate))
            {
                DateTime parsedEndDate;
                if (DateTime.TryParseExact(endDate, "yy-MM-dd HH:mm:ss.ff", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedEndDate))
                {
                    query = query.Where(rd => DateTime.ParseExact(rd.Timestamp, "yy-MM-dd HH:mm:ss.ff", CultureInfo.InvariantCulture) <= parsedEndDate);
                }
                else
                {
                    // Handle invalid endDate format
                    return null; // Or throw an exception, return an empty list, etc.
                }
            }

            if (minVelocity > 0)
            {
                query = query.Where(rd => rd.Velocity >= minVelocity);
            }

            if (maxVelocity > 0)
            {
                query = query.Where(rd => rd.Velocity <= maxVelocity);
            }

            return await query.ToListAsync();
        }

        public async Task<RoadData?> GetByIdAsync(string id)
        {
            return await _context.RoadDatas.FindAsync(id);
        }
    }
}