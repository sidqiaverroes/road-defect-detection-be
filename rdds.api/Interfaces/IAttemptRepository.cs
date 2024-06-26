using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Dtos.Attempt;
using rdds.api.Models;

namespace rdds.api.Interfaces
{
    public interface IAttemptRepository
    {
        Task<List<Attempt>> GetAllAsync();
        Task<Attempt?> GetByIdAsync(int id);
        Task<Attempt?> CreateAsync(Attempt attemptModel, string deviceMac);
        Task<Attempt?> UpdateAsync(int id, Attempt attemptModel);
        Task<Attempt?> FinishAsync(int id);
        Task<Attempt?> DeleteAsync(int id);
        Task<bool> IsExistedAsync(int id);
        Task<bool> IsAttemptRelatedToDevice(int attemptId, string deviceMac);
        Task<Attempt?> GetLastAttempt(string deviceMac);
    }
}