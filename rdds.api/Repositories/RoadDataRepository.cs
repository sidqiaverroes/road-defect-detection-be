using System;
using System.Collections.Generic;
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

        public async Task<List<RoadData>> GetAllByFilterAsync(string deviceMac, int attemptId)
        {
            var query = _context.RoadDatas.Where(rd => rd.AttemptId == attemptId)
            .Include(rd => rd.Attempt).Where(a => a.Attempt.DeviceId == deviceMac);

            return await query.ToListAsync();
        }

        public async Task<RoadData?> GetByIdAsync(string id)
        {
            return await _context.RoadDatas.FindAsync(id);
        }
    }
}