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
    public class AttemptSummaryDataRepository: IAttemptSummaryData
    {
        private readonly ApplicationDbContext _context;
        public AttemptSummaryDataRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AttemptSummaryData?> CreateAsync(AttemptSummaryData attemptSummaryData)
        {
            try
            {
                // Add AttemptSummaryData to context and save changes
                await _context.AttemptSummaryDatas.AddAsync(attemptSummaryData);
                await _context.SaveChangesAsync();

                return attemptSummaryData;
            }
            catch (Exception ex)
            {
                // Handle exception (log, throw, etc.)
                throw new Exception("Failed to create AttemptSummaryData", ex);
            }
        }

        public async Task<List<AttemptSummaryData>> GetAllAsync()
        {
            return await _context.AttemptSummaryDatas
                .Include(a => a.Attempt)
                .ToListAsync();
        }


        public async Task<AttemptSummaryData?> GetByAttemptIdAsync(int attemptId)
        {
            return await _context.AttemptSummaryDatas
                .FirstOrDefaultAsync(s => s.AttemptId == attemptId);
        }

        public async Task<List<AttemptSummaryData>> GetAllByDeviceIdAsync(string deviceId)
        {
            // Assuming Attempt has a property DeviceId and navigation property is configured correctly
            return await _context.AttemptSummaryDatas
                .Include(a => a.Attempt)
                .Where(a => a.Attempt.DeviceId == deviceId)
                .ToListAsync();
        }

        public async Task<AttemptSummaryData> UpdateAsync(AttemptSummaryData summaryData)
        {
            _context.AttemptSummaryDatas.Update(summaryData);
            await _context.SaveChangesAsync();
            return summaryData;
        }


    }
}