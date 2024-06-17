using System;
using System.Collections.Generic;
using System.Globalization;
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
                Timestamp = roadDataModel.Timestamp.ToString(),
                AttemptId = roadDataModel.AttemptId
            };
        }

        public static RoadData ToRoadDataFromCreate(this CreateRoadDataDto roadDataDto, int attemptId)
        {
            // Convert Timestamp string to DateTime
            DateTime timestamp;
            // Attempt to parse the timestamp string to DateTime using specific format
            if (!DateTime.TryParseExact(roadDataDto.Timestamp, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp))
            {
                throw new ArgumentException($"Invalid timestamp format: {roadDataDto.Timestamp}. Expected format: yyyy-MM-dd HH:mm:ss.fff");
            }


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
                Timestamp = timestamp,
                AttemptId = attemptId
            };
        }
    }
}