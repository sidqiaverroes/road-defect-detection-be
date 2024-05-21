using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Dtos.Attempt;
using rdds.api.Models;

namespace rdds.api.Mappers
{
    public static class AttemptMapper
    {
        public static AttemptDto ToAttemptDto(this Attempt attemptModel)
        {
            return new AttemptDto
            {
                Id = attemptModel.Id,
                Title = attemptModel.Title,
                Description = attemptModel.Description,
                CreatedOn = attemptModel.CreatedOn,
                LastModified = attemptModel.LastModified,
                IsFinished = attemptModel.IsFinished,
                FinishedOn = attemptModel.FinishedOn,
                DeviceId = attemptModel.DeviceId
            };
        }

        public static Attempt ToAttemptFromCreate(this CreateAttemptDto attemptDto, string deviceMac)
        {
            return new Attempt
            {
                Title = attemptDto.Title,
                Description = attemptDto.Description,
                DeviceId = deviceMac
            };
        }

        public static Attempt ToAttemptFromUpdate(this UpdateAttemptDto attemptDto)
        {
            return new Attempt
            {
                Title = attemptDto.Title,
                Description = attemptDto.Description,
            };
        }

        public static Attempt ToAttemptFromFinish(this FinishAttemptDto attemptDto)
        {
            return new Attempt
            {
                IsFinished = attemptDto.IsFinished
            };
        }
    }
}