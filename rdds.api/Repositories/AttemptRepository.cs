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
        public async Task<Attempt?> CreateAsync(Attempt attemptModel, string deviceMac)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var lastAttempt = await GetLastAttempt(deviceMac);

                    // If there is no last attempt or the last attempt is finished, create a new attempt
                    if (lastAttempt == null || lastAttempt.IsFinished)
                    {
                        await _context.Attempts.AddAsync(attemptModel);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return attemptModel;
                    }

                    // If the last attempt is not finished, return null
                    await transaction.RollbackAsync();
                    return null;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("An error occurred while creating a new attempt", ex);
                }
            }
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
            return await _context.Attempts.Include(a => a.RoadCategory).ToListAsync();
        }

        public async Task<Attempt?> GetByIdAsync(int id)
        {
            return await _context.Attempts
            .Include(a => a.RoadCategory)
            .FirstOrDefaultAsync(a => a.Id == id);
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

        public async Task<Attempt?> GetLastAttempt(string deviceMac)
        {
            return await _context.Attempts
            .Where(a => a.DeviceId == deviceMac)
            .OrderByDescending(a => a.Id)
            .FirstOrDefaultAsync();
        }
    }
}