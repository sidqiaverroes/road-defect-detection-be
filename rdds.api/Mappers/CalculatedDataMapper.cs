using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Dtos.CalculatedData;
using rdds.api.Models;

namespace rdds.api.Mappers
{
    public static class CalculatedDataMapper
    {
        public static CalculatedDataDto ToCalculatedDataDto(this CalculatedData calculatedData)
        {
            return new CalculatedDataDto
            {
                IRI = new InternationalRoughnessIndex
                {
                    Roll = calculatedData.IRI.Roll,
                    Pitch = calculatedData.IRI.Pitch,
                    Euclidean = calculatedData.IRI.Euclidean,
                    Average = calculatedData.IRI.Average,
                    RollProfile = calculatedData.IRI.RollProfile,
                    PitchProfile = calculatedData.IRI.PitchProfile,
                    EuclideanProfile = calculatedData.IRI.EuclideanProfile,
                    AverageProfile = calculatedData.IRI.AverageProfile,
                },
                Velocity = calculatedData.Velocity,
                CoordinateStart = new Coordinate
                {
                    Latitude = calculatedData.CoordinateStart.Latitude,
                    Longitude = calculatedData.CoordinateStart.Longitude
                },
                CoordinateEnd = new Coordinate
                {
                    Latitude = calculatedData.CoordinateEnd.Latitude,
                    Longitude = calculatedData.CoordinateStart.Longitude
                },
                Timestamp = calculatedData.Timestamp.ToString(),
                AttemptId = calculatedData.AttemptId
            };
        }

        public static CalculatedData ToCalculatedDataFromCreate(this CreateCalculatedDataDto calculatedDataDto, int attemptId)
        {
            string[] formats = new string[] { 
                "yyyy-MM-dd HH:mm:ss.f",    // For milliseconds with one decimal place
                "yyyy-MM-dd HH:mm:ss.ff",   // For milliseconds with two decimal places
                "yyyy-MM-dd HH:mm:ss.fff"   // For milliseconds with three decimal places
            };

            // Attempt to parse the timestamp
            if (!DateTime.TryParseExact(calculatedDataDto.Timestamp, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime timestamp))
            {
                throw new ArgumentException($"Invalid timestamp format: {calculatedDataDto.Timestamp}. Expected format: yyyy-MM-dd HH:mm:ss.f, yyyy-MM-dd HH:mm:ss.ff, or yyyy-MM-dd HH:mm:ss.fff");
            }

            return new CalculatedData
            {
                IRI = new InternationalRoughnessIndex
                {
                    Roll = calculatedDataDto.IRI.Roll,
                    Pitch = calculatedDataDto.IRI.Pitch,
                    Euclidean = calculatedDataDto.IRI.Euclidean,
                    Average = calculatedDataDto.IRI.Average,
                    RollProfile = calculatedDataDto.IRI.RollProfile,
                    PitchProfile = calculatedDataDto.IRI.PitchProfile,
                    EuclideanProfile = calculatedDataDto.IRI.EuclideanProfile,
                    AverageProfile = calculatedDataDto.IRI.AverageProfile,
                },
                Velocity = calculatedDataDto.Velocity,
                CoordinateStart = new Coordinate
                {
                    Latitude = calculatedDataDto.CoordinateStart.Latitude,
                    Longitude = calculatedDataDto.CoordinateStart.Longitude
                },
                CoordinateEnd = new Coordinate
                {
                    Latitude = calculatedDataDto.CoordinateEnd.Latitude,
                    Longitude = calculatedDataDto.CoordinateStart.Longitude
                },
                Timestamp = timestamp,
                AttemptId = attemptId
            };
        }
    }
}