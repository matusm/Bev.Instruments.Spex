﻿namespace Bev.Instruments.Spex
{
    public class WavelengthConverter : IWavelengthConverter
    {
        public WavelengthConverter() : this(50.0, 0.0) { }
        public WavelengthConverter(double stepsPerNm) : this(stepsPerNm, 0.0) { }
        public WavelengthConverter(double stepsPerNm, double offset)
        {
            StepsPerNanometer = stepsPerNm;
            StepsOffset = offset;
        }

        public double StepsPerNanometer { get; }
        public double StepsOffset { get; }

        public double StepsToWavelength(int steps) => steps / StepsPerNanometer;

        public int WavelengthToSteps(double wavelength) => (int)(wavelength * StepsPerNanometer);
    }
}
