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
    public class RoadCategoryRepository : IRoadCategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public RoadCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RoadCategory?> CreateAsync(RoadCategory roadCategoryModel)
        {
            await _context.RoadCategories.AddAsync(roadCategoryModel);
            await _context.SaveChangesAsync();
            return roadCategoryModel;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var roadCategory = await _context.RoadCategories.FindAsync(id);
            if (roadCategory == null)
            {
                return false;
            }

            _context.RoadCategories.Remove(roadCategory);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<RoadCategory>> GetAllAsync()
        {
            return await _context.RoadCategories.ToListAsync();
        }

        public async Task<RoadCategory?> GetByIdAsync(int id)
        {
            return await _context.RoadCategories.FindAsync(id);
        }

        public async Task<bool> IsExistedAsync(int id)
        {
            return await _context.RoadCategories.AnyAsync(rc => rc.Id == id);
        }

        public async Task<RoadCategory?> UpdateAsync(int id, RoadCategory roadCategoryModel)
        {
            var existingRoadCategory = await _context.RoadCategories.FindAsync(id);
            if (existingRoadCategory == null)
            {
                return null;
            }

            existingRoadCategory.Name = roadCategoryModel.Name;
            existingRoadCategory.TotalLength = roadCategoryModel.TotalLength;

            await _context.SaveChangesAsync();
            return existingRoadCategory;
        }
    }
}