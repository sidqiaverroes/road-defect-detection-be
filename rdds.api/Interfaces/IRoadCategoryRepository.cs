using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Dtos.RoadCategory;
using rdds.api.Models;

namespace rdds.api.Interfaces
{
    public interface IRoadCategoryRepository
    {
        Task<List<RoadCategory>> GetAllAsync();
        Task<RoadCategory?> GetByIdAsync(int id);
        Task<RoadCategory?> CreateAsync(RoadCategory roadCategoryModel);
        Task<RoadCategory?> UpdateAsync(int id, RoadCategory roadCategoryModel);
        Task<bool> DeleteAsync(int id);
        Task<bool> IsExistedAsync(int id);
    }
}