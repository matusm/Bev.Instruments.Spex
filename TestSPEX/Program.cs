using System;
using System.Globalization;
using System.Threading;
using Bev.Instruments.Spex;

namespace TestSPEX
{
    class Program
    {
        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            IWavelengthConverter waveConverter = new WavelengthConverter(50.0, 0.0);

            int gpibAddress = 3;
            Spex spex = new Spex(gpibAddress, waveConverter);

            Console.WriteLine(spex.InstrumentID);
            Console.WriteLine($"steps: {spex.GetCurrentStepPosition()} -> {spex.GetCurrentWavelength()} nm");
            spex.MoveRelativeSteps(-1500); //Thread.Sleep(10000);
            Console.WriteLine($"steps: {spex.GetCurrentStepPosition()} -> {spex.GetCurrentWavelength()} nm");
            spex.MoveRelativeSteps(1500); //Thread.Sleep(10000);
            Console.WriteLine($"steps: {spex.GetCurrentStepPosition()} -> {spex.GetCurrentWavelength()} nm");
            Console.WriteLine();
            Console.WriteLine();

            Console.Write("Input displayed wavelength (in nm): ");
            double displayWavelength = double.Parse(Console.ReadLine());
            Console.WriteLine($"your input: {displayWavelength} nm");
            int steps = spex.WavelengthCalibration.WavelengthToSteps(displayWavelength);
            Console.WriteLine($"steps to set: {steps}");
            spex.SetCurrentStepPosition(steps);
            Console.WriteLine($"steps: {spex.GetCurrentStepPosition()} -> {spex.GetCurrentWavelength()} nm");

            double wavelength = 0;

            while (true)
            {
                Console.Write("Drive to wavelength (in nm): ");
                wavelength = double.Parse(Console.ReadLine());
                if (wavelength < -100) break;
                Console.WriteLine($"moving to {wavelength} nm");
                spex.MoveAbsoluteWavelength(wavelength);
                Console.WriteLine($"steps: {spex.GetCurrentStepPosition()} -> {spex.GetCurrentWavelength()} nm");
                Console.WriteLine();
            }

            wavelength = 600;
            Console.WriteLine($"moving to {wavelength} nm");
            spex.MoveAbsoluteWavelength(wavelength);
            Console.WriteLine($"steps: {spex.GetCurrentStepPosition()} -> {spex.GetCurrentWavelength()} nm");

        }
    }
}
