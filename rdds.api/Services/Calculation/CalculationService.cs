using System;
using System.Linq;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using Microsoft.AspNetCore.Mvc;
using rdds.api.Interfaces;

namespace rdds.api.Services.Calculation
{
    public class CalculationService
    {
        private const int nperseg = 128; // Number of points per segment

        public (double[] f, double[] psd) CalculatePSD(double[] data, int fs)
        {
            int n = data.Length;
            
            // Use MathNet.Numerics for calculating Welch's method
            var psd = new double[nperseg / 2 + 1];
            var freqs = new double[nperseg / 2 + 1];
            double[] window = Window.Hamming(nperseg);
            var segmentCount = (n - nperseg) / (nperseg / 2) + 1;
            
            for (int i = 0; i < segmentCount; i++)
            {
                var segment = data.Skip(i * nperseg / 2).Take(nperseg).ToArray();
                var fft = new Complex[nperseg];
                for (int j = 0; j < nperseg; j++)
                {
                    fft[j] = segment[j] * window[j];
                }
                
                Fourier.Forward(fft, FourierOptions.Matlab);
                for (int j = 0; j < nperseg / 2 + 1; j++)
                {
                    var power = fft[j].MagnitudeSquared() / (nperseg * fs);
                    if (i == 0)
                    {
                        psd[j] = power;
                        freqs[j] = (double)j * fs / nperseg;
                    }
                    else
                    {
                        psd[j] = (psd[j] + power) / 2.0;
                    }
                }
            }

            return (freqs, psd);
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
