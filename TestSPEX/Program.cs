using System;
using Bev.Instruments.Spex;

namespace TestSPEX
{
    class Program
    {
        static void Main(string[] args)
        {

            int gpibAddress = 3;

            Spex spex = new Spex(gpibAddress);

            Console.WriteLine(spex.InstrumentID);

            Console.WriteLine($"steps: {spex.GetCurrentStepPosition()} -> {spex.GetCurrentWavelength()} nm");
            spex.MoveRelativeSteps(1000);
            Console.WriteLine($"steps: {spex.GetCurrentStepPosition()} -> {spex.GetCurrentWavelength()} nm");
            spex.MoveRelativeSteps(-1000);
            Console.WriteLine($"steps: {spex.GetCurrentStepPosition()} -> {spex.GetCurrentWavelength()} nm");

            Console.WriteLine();

            Console.Write("Input displayed wavelength (in nm): ");
            double displayWavelength = double.Parse(Console.ReadLine());
            Console.WriteLine($"your input: {displayWavelength} nm");

            int steps = spex.WavelengthCalibration.WavelengthToSteps(displayWavelength);
            spex.SetCurrentStepPosition(steps);

            Console.WriteLine($"steps: {spex.GetCurrentStepPosition()} -> {spex.GetCurrentWavelength()} nm");
            spex.MoveRelativeSteps(1000);
            Console.WriteLine($"steps: {spex.GetCurrentStepPosition()} -> {spex.GetCurrentWavelength()} nm");
            spex.MoveRelativeSteps(-1000);
            Console.WriteLine($"steps: {spex.GetCurrentStepPosition()} -> {spex.GetCurrentWavelength()} nm");
        }
    }
}
