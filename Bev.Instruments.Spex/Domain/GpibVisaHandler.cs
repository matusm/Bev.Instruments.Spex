using System.Threading;
using NationalInstruments.Visa;

namespace Bev.Instruments.Spex
{
    public class GpibVisaHandler : IIeee488Handler
    {
        private const int delay = 100;
        private readonly ResourceManager resourceManager;
        private readonly GpibSession gpibSession;

        public GpibVisaHandler(int deviceAddress)
        {
            resourceManager = new ResourceManager();
            gpibSession = (GpibSession)resourceManager.Open($"GPIB0::{deviceAddress}::INSTR");
            gpibSession.TerminationCharacterEnabled = false;
            gpibSession.TimeoutMilliseconds = 10000;
            gpibSession.TerminationCharacter = 0x0D;
        }

        public int DeviceAddress => gpibSession.PrimaryAddress;

        public void SendBytes(byte[] b)
        {
            gpibSession.RawIO.Write(b);
            Thread.Sleep(delay);
        }

        public void SendBytes(byte b) => SendBytes(new byte[] { b });

        public byte ReadByte()
        {
            byte[] reply = gpibSession.RawIO.Read(1);
            if (reply == null) return 0;
            if (reply.Length == 0) return 0;
            return reply[0];
        }

        public byte[] ReadBytes() => gpibSession.RawIO.Read();

    }
}
