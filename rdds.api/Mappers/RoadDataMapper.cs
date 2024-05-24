using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Dtos.RoadData;
using rdds.api.Models;

namespace rdds.api.Mappers
{
    public static class RoadDataMapper
    {
        public static RoadDataDto ToRoadDataDto(this RoadData roadDataModel)
        {
            return new RoadDataDto
            {
                Id = roadDataModel.Id,
                Roll = roadDataModel.Roll,
                Pitch = roadDataModel.Pitch,
                Yaw = roadDataModel.Yaw,
                Euclidean = roadDataModel.Euclidean,
                Velocity = roadDataModel.Velocity,
                Coordinate = new Coordinate
                {
                    Latitude = roadDataModel.Coordinate.Latitude,
                    Longitude = roadDataModel.Coordinate.Longitude
                },
                Timestamp = roadDataModel.Timestamp,
                AttemptId = roadDataModel.AttemptId
            };
        }

        public static RoadData ToRoadDataFromCreate(this CreateRoadDataDto roadDataDto, int attemptId)
        {
            return new RoadData
            {
                Roll = roadDataDto.Roll,
                Pitch = roadDataDto.Pitch,
                Yaw = roadDataDto.Yaw,
                Euclidean = roadDataDto.Euclidean,
                Velocity = roadDataDto.Velocity,
                Coordinate = new Coordinate
                {
                    Latitude = roadDataDto.Latitude,
                    Longitude = roadDataDto.Longitude
                },
                Timestamp = roadDataDto.Timestamp,
                AttemptId = attemptId
            };
        }
    }
}