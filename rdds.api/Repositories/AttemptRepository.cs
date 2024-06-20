using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using rdds.api.Data;
using rdds.api.Dtos.Attempt;
using rdds.api.Interfaces;
using rdds.api.Models;

namespace rdds.api.Repositories
{
    public class AttemptRepository : IAttemptRepository
    {
        private readonly ApplicationDbContext _context;
        public AttemptRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Attempt?> CreateAsync(Attempt attemptModel)
        {
            var lastAttempt = await _context.Attempts.OrderByDescending(a => a.CreatedOn).FirstOrDefaultAsync();

            if (lastAttempt != null && !lastAttempt.IsFinished)
            {
                return null;
            }

            await _context.Attempts.AddAsync(attemptModel);
            await _context.SaveChangesAsync();
            return attemptModel;
        }

        public async Task<Attempt?> DeleteAsync(int id)
        {
            var attempt = await _context.Attempts.FindAsync(id);

            if (attempt == null)
            {
                return null; // Attempt not found
            }

            _context.Attempts.Remove(attempt);
            await _context.SaveChangesAsync();

            return attempt;
        }

        public async Task<Attempt?> FinishAsync(int id)
        {
            var existingAttempt = await _context.Attempts.FindAsync(id);

            if(existingAttempt != null && existingAttempt.IsFinished)
            {
                return null;
            }

            if(existingAttempt != null &&  !existingAttempt.IsFinished)
            {
                existingAttempt.IsFinished = true;
                existingAttempt.FinishedOn = DateTime.Now;
                existingAttempt.LastModified = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return existingAttempt;
        }

        public async Task<List<Attempt>> GetAllAsync()
        {
            return await _context.Attempts.ToListAsync();
        }

        public async Task<Attempt?> GetByIdAsync(int id)
        {
            return await _context.Attempts.FindAsync(id);
        }

        public async Task<bool> IsExistedAsync(int id)
        {
            return await _context.Attempts.AnyAsync(a => a.Id == id);
        } 

        public async Task<Attempt?> UpdateAsync(int id, Attempt attemptModel)
        {
            var existingAttempt = await _context.Attempts.FindAsync(id);

            if(existingAttempt == null)
            {
                return null;
            }

            existingAttempt.Title = attemptModel.Title;
            existingAttempt.Description = attemptModel.Description;
            existingAttempt.RoadCategoryId = attemptModel.RoadCategoryId;
            existingAttempt.LastModified = DateTime.Now;

            await _context.SaveChangesAsync();

            return existingAttempt;
        }

        public async Task<bool> IsAttemptRelatedToDevice(int attemptId, string deviceMac)
        {
            // Check if there exists an attempt with the given attemptId related to the device with the specified deviceMac
            var isRelated = await _context.Attempts
                .AnyAsync(a => a.Id == attemptId && a.Device.MacAddress == deviceMac);

            return isRelated;
        }
    }
}