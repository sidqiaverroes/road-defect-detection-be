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
                Roll = roadDataModel.Roll,
                Pitch = roadDataModel.Pitch,
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
            string[] formats = new string[] { 
                "yyyy-MM-dd HH:mm:ss.f",    // For milliseconds with one decimal place
                "yyyy-MM-dd HH:mm:ss.ff",   // For milliseconds with two decimal places
                "yyyy-MM-dd HH:mm:ss.fff"   // For milliseconds with three decimal places
            };

            // Attempt to parse the timestamp
            if (!DateTime.TryParseExact(roadDataDto.Timestamp, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime timestamp))
            {
                throw new ArgumentException($"Invalid timestamp format: {roadDataDto.Timestamp}. Expected format: yyyy-MM-dd HH:mm:ss.f, yyyy-MM-dd HH:mm:ss.ff, or yyyy-MM-dd HH:mm:ss.fff");
            }


            return new RoadData
            {
                Roll = roadDataDto.Roll,
                Pitch = roadDataDto.Pitch,
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