using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Dtos.SummaryData;
using rdds.api.Models;

namespace rdds.api.Mappers
{
    public static class SummaryMapper
    {
        public static SummaryDataDto ToSummaryDataDto(this AttemptSummaryData attemptSummaryData)
        {
            if (attemptSummaryData == null)
                return null;

            return new SummaryDataDto
            {
                TotalLength = attemptSummaryData.TotalLength,
                LengthData = new LengthData
                {
                    Baik = attemptSummaryData.LengthData.Baik,
                    Sedang = attemptSummaryData.LengthData.Sedang,
                    RusakRingan = attemptSummaryData.LengthData.RusakRingan,
                    RusakBerat = attemptSummaryData.LengthData.RusakBerat
                },
                PercentageData = new PercentageData
                {
                    Baik = attemptSummaryData.PercentageData.Baik,
                    Sedang = attemptSummaryData.PercentageData.Sedang,
                    RusakRingan = attemptSummaryData.PercentageData.RusakRingan,
                    RusakBerat = attemptSummaryData.PercentageData.RusakBerat
                },
                AttemptId = attemptSummaryData.AttemptId,
                Attempt = attemptSummaryData.Attempt.ToAttemptDto()
            };
        }
    }
}