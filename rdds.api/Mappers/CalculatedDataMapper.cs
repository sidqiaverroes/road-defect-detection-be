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
                Id = calculatedData.Id,
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
                Coordinate = new Coordinate
                {
                    Latitude = calculatedData.Coordinate.Latitude,
                    Longitude = calculatedData.Coordinate.Longitude
                },
                Timestamp = calculatedData.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                AttemptId = calculatedData.AttemptId
            };
        }

        public static CalculatedData ToCalculatedDataFromCreate(this CreateCalculatedDataDto calculatedDataDto, int attemptId)
        {
            DateTime timestamp;
            if (!DateTime.TryParseExact(calculatedDataDto.Timestamp, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp))
            {
                throw new ArgumentException($"Invalid timestamp format: {calculatedDataDto.Timestamp}. Expected format: yyyy-MM-dd HH:mm:ss.fff");
            }

            return new CalculatedData
            {
                IRI = new InternationalRoughnessIndex
                {
                    Roll = calculatedDataDto.IRIRoll,
                    Pitch = calculatedDataDto.IRIPitch,
                    Euclidean = calculatedDataDto.IRIEuclidean
                },
                Velocity = calculatedDataDto.Velocity,
                Coordinate = new Coordinate
                {
                    Latitude = calculatedDataDto.Latitude,
                    Longitude = calculatedDataDto.Longitude
                },
                Timestamp = timestamp,
                AttemptId = attemptId
            };
        }
    }
}