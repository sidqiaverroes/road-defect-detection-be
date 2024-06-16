using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Dtos.RoadCategory;
using rdds.api.Models;

namespace rdds.api.Mappers
{
    public static class RoadCategoryMapper
    {
        public static RoadCategoryDto ToRoadCategoryDto(this RoadCategory model)
        {
            return new RoadCategoryDto
            {
                Id = model.Id,
                Name = model.Name,
                TotalLength = model.TotalLength
            };
        }

        public static RoadCategory ToRoadCategoryFromCreate(this CreateRoadCategoryDto dto)
        {
            return new RoadCategory
            {
                Name = dto.Name,
                TotalLength = dto.TotalLength
            };
        }

        public static RoadCategory ToRoadCategoryFromUpdate(this UpdateRoadCategoryDto dto)
        {
            return new RoadCategory
            {
                Name = dto.Name,
                TotalLength = dto.TotalLength
            };
        }

    }
}