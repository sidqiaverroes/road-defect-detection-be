using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Models;

namespace rdds.api.Interfaces
{
    public interface IAttemptSummaryData
    {
        Task<AttemptSummaryData?> CreateAsync(AttemptSummaryData sumModel);
        Task<List<AttemptSummaryData>> GetAllAsync();
        Task<AttemptSummaryData?> GetByAttemptIdAsync(int attemptId);
        Task<List<AttemptSummaryData>> GetAllByDeviceIdAsync(string deviceId);
        Task<AttemptSummaryData> UpdateAsync(AttemptSummaryData summaryData);

    }
}