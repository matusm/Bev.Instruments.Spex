using Bev.IO.RemoteInterface;
using Bev.IO.Gpib.Keithley500Serial;
using Bev.Instruments.Spex;
using RS232Interface;
using System;

namespace TestSPEX
{
    class Program
    {
        static void Main(string[] args)
        {
            string comPort = "COM1";
            int gpibAddress = 3;

            SerialInterface remote = new SerialInterface(comPort);
            // Keithley500Serial remote = new Keithley500Serial(comPort);

            Spex spex = new Spex(gpibAddress, remote);

            Console.WriteLine(spex.InstrumentID);

            spex.MoveRelativeRaw(1000);
            Console.WriteLine(spex.GetPositionRaw());
            spex.MoveRelativeRaw(-1000);
            Console.WriteLine(spex.GetPositionRaw());

        }
    }
}
