using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Models;

namespace rdds.api.Interfaces
{
    public interface IRoadDataRepository
    {
        Task<List<RoadData>> GetAllByFilterAsync(string deviceMac, int attemptId);
        Task<bool> CreateAsync(List<RoadData> roadDataModel);
        Task<RoadData?> GetByIdAsync(string id);
    }
}