using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Dtos.RoadData;
using rdds.api.Models;

namespace rdds.api.Interfaces
{
    public interface IRoadDataRepository
    {
        Task<List<RoadData>> GetAllByFilterAsync(string deviceMac, int attemptId, string startDate, string endDate, float minVelocity, float maxVelocity);
        Task<bool> CreateAsync(IEnumerable<CreateRoadDataDto> roadDataModels, int attemptId);
        Task<bool> DeleteAllAsync();
        Task CreateFromMqttAsync(string payload, int attemptId);
    }
}