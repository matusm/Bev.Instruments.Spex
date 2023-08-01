using System;
using System.Globalization;
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
            Console.WriteLine("Motor speed parameters: " + spex.GetMotorSpeed());

            spex.MoveRelativeSteps(+2000); 
            spex.MoveRelativeSteps(-2000);
            OutputPosition();
            Console.WriteLine();

            Console.Write("Input displayed wavelength (in nm): ");
            double displayWavelength = double.Parse(Console.ReadLine());
            Console.WriteLine($"your input: {displayWavelength} nm");
            int steps = spex.WavelengthCalibration.WavelengthToSteps(displayWavelength);
            Console.WriteLine($"steps to set: {steps}");
            spex.SetCurrentStepPosition(steps);
            OutputPosition();

            double wavelength;
            while (true)
            {
                Console.Write("Drive to wavelength (in nm): ");
                wavelength = double.Parse(Console.ReadLine());
                if (wavelength < -200) break;
                Console.WriteLine($"moving to {wavelength} nm");
                spex.MoveAbsoluteWavelength(wavelength);
                OutputPosition();
            }

            Console.WriteLine();
            wavelength = 000;
            Console.WriteLine($"moving to {wavelength} nm");
            spex.MoveAbsoluteWavelength(wavelength);
            OutputPosition();




            void OutputPosition()
            {
                Console.WriteLine($"steps: {spex.GetCurrentStepPosition()} -> {spex.GetCurrentWavelength():F2} nm");
                Console.WriteLine(spex.HitAnyLimitSwitch);
                Console.WriteLine();
            }

        }
    }
}
