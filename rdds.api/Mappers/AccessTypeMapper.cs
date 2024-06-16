using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Dtos.AccessType;
using rdds.api.Dtos.Account;
using rdds.api.Models;

namespace rdds.api.Mappers
{
    public static class AccessTypeMapper
    {
        public static AccessTypeDto ToAccessTypeDto(this AccessType accessTypeModel)
        {
            return new AccessTypeDto
            {
                Id = accessTypeModel.Id,
                Name = accessTypeModel.Name,
                Accesses = accessTypeModel.Accesses,
            };
        }

        public static AccessType ToAccessTypeFromCreate(this CreateAccessTypeDto newAccessTypeDto)
        {
            return new AccessType
            {
                Name = newAccessTypeDto.Name,
                Accesses = new List<string>(newAccessTypeDto.Accesses)
            };
        }

        public static AccessType ToAccessTypeFromUpdate(this UpdateAccessTypeDto accessTypeDto)
        {
            return new AccessType
            {
                Name = accessTypeDto.Name,
                Accesses = new List<string>(accessTypeDto.Accesses)
            };
        }
    }
}