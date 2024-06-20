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
        Task<List<CalculatedData>> GetAllByFilterAsync(int? attemptId, string startDate, string endDate);
        Task<bool> CreateAsync(IEnumerable<CreateCalculatedDataDto> calculatedDataDtos, int attemptId);
        Task<bool> DeleteAllAsync();
        Task CreateFromMqttAsync(List<SensorData> sensorDataList, InternationalRoughnessIndex IRI, int attemptId);
    }
}