using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Dtos.CalculatedData;
using rdds.api.Models;

namespace rdds.api.Interfaces
{
    public interface ICalculatedDataRepository
    {
        Task<List<CalculatedData>> GetAllByFilterAsync(string deviceMac, int attemptId, string startDate, string endDate, float minVelocity, float maxVelocity);
        Task<bool> CreateAsync(IEnumerable<CreateCalculatedDataDto> calculatedDataDtos, int attemptId);
        Task<bool> DeleteAllAsync();
        Task CreateFromMqttAsync(string payload, int attemptId);
    }
}