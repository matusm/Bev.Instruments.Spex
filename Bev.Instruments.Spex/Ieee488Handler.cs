using System.Threading;
using NationalInstruments.Visa;

namespace Bev.Instruments.Spex
{
    internal class Ieee488Handler
    {
        private const int delay = 100;
        private readonly ResourceManager resourceManager;
        private readonly GpibSession gpibSession;

        internal Ieee488Handler(int deviceAddress)
        {
            resourceManager = new ResourceManager();
            gpibSession = (GpibSession)resourceManager.Open($"GPIB0::{deviceAddress}::INSTR");
            gpibSession.TerminationCharacterEnabled = false;
            gpibSession.TimeoutMilliseconds = 10000;
            gpibSession.TerminationCharacter = 0x0D;
        }

        internal int DeviceAddress => gpibSession.PrimaryAddress;

        internal void SendBytes(byte[] b)
        {
            gpibSession.RawIO.Write(b);
            Thread.Sleep(delay);
        }

        internal void SendBytes(byte b) => SendBytes(new byte[] { b });

        internal byte ReadByte() => gpibSession.RawIO.Read(1)[0];

        internal byte[] ReadBytes() => gpibSession.RawIO.Read();

    }
}
