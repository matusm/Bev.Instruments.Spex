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
            // 1681 B SN:4867 V3.6  Maple Leaf - V3.6  Maple Leaf @ 03
            // 1200 nm -> limit switch at 1199 nm
            // -90 nm  -> limit switch at 9933.5 nm

            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            IWavelengthConverter waveConverter = new WavelengthConverter(50.0, 0.0);

            int gpibAddress = 3;
            Spex spex = new Spex(gpibAddress, waveConverter);

            Console.WriteLine(spex.InstrumentID);
            Console.WriteLine($"steps: {spex.GetCurrentStepPosition()} -> {spex.GetCurrentWavelength()} nm");
            spex.MoveRelativeSteps(-1000); 
            Console.WriteLine($"steps: {spex.GetCurrentStepPosition()} -> {spex.GetCurrentWavelength()} nm");
            Console.WriteLine(spex.HitAnyLimitSwitch);
            spex.MoveRelativeSteps(100); 
            Console.WriteLine($"steps: {spex.GetCurrentStepPosition()} -> {spex.GetCurrentWavelength()} nm");
            Console.WriteLine(spex.HitAnyLimitSwitch);
            Console.WriteLine();
            Console.WriteLine();

            Console.Write("Input displayed wavelength (in nm): ");
            double displayWavelength = double.Parse(Console.ReadLine());
            Console.WriteLine($"your input: {displayWavelength} nm");
            int steps = spex.WavelengthCalibration.WavelengthToSteps(displayWavelength);
            Console.WriteLine($"steps to set: {steps}");
            spex.SetCurrentStepPosition(steps);
            Console.WriteLine($"steps: {spex.GetCurrentStepPosition()} -> {spex.GetCurrentWavelength()} nm");
            Console.WriteLine();

            //for (int i = 400; i <= 800; i++)
            //{
            //    spex.MoveAbsoluteWavelength(i);
            //    Console.WriteLine($"wavelength {i} nm");
            //    Thread.Sleep(200);
            //}

            double wavelength;
            while (true)
            {
                Console.Write("Drive to wavelength (in nm): ");
                wavelength = double.Parse(Console.ReadLine());
                if (wavelength < -100) break;
                Console.WriteLine($"moving to {wavelength} nm");
                spex.MoveAbsoluteWavelength(wavelength);
                Console.WriteLine($"steps: {spex.GetCurrentStepPosition()} -> {spex.GetCurrentWavelength()} nm");
                Console.WriteLine(spex.HitAnyLimitSwitch);
                Console.WriteLine();
            }

            wavelength = 400;
            Console.WriteLine($"moving to {wavelength} nm");
            spex.MoveAbsoluteWavelength(wavelength);
            Console.WriteLine($"steps: {spex.GetCurrentStepPosition()} -> {spex.GetCurrentWavelength()} nm");
            Console.WriteLine(spex.HitAnyLimitSwitch);

        }
    }
}
