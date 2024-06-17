using System;
using System.Linq;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using Microsoft.AspNetCore.Mvc;

namespace rdds.api.Services.Calculation
{
    public class CalculationService
    {
        private const int Nperseg = 1024; // Number of points per segment

        public (double[] frequencies, double[] psd) CalculatePSD(double[] data, int fs)
        {
            var signal = data.Select(x => new Complex(x, 0)).ToArray();
            Fourier.Forward(signal, FourierOptions.NoScaling);

            var psd = signal.Select(x => x.MagnitudeSquared() / (fs * Nperseg)).ToArray();
            var frequencies = Fourier.FrequencyScale(fs, Nperseg);

            return (frequencies, psd);
        }

        public double CalculateIRI(double[] psd, double[] frequencies)
        {
            // Calculate the integral using the trapezoidal rule
            double integral = 0.0;
            for (int i = 1; i < psd.Length; i++)
            {
                double segment = (psd[i - 1] + psd[i]) * (frequencies[i] - frequencies[i - 1]) / 2.0;
                integral += segment;
            }

            // Integrate the PSD over the frequency range to get the roughness standard deviation
            var roughnessStd = Math.Sqrt(integral);

            // Use the regression equation to calculate IRI from roughness standard deviation
            var iri = 0.5926 * roughnessStd - 0.013;
            return iri;
        }
    }
}
