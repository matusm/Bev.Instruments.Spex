using System;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace Bev.Instruments.Spex
{
    public class GpibPrologixHandler : IIeee488Handler
    {
        private readonly SerialPort serialPort;

        public GpibPrologixHandler(int deviceAddress, string serialPortname)
        {
            InitializeSerialPort(serialPortname);
            serialPort.Open();
            InitializePrologix(deviceAddress);
            DeviceAddress = deviceAddress;
        }

        public int DeviceAddress { get; }

        public byte ReadByte()
        {
            byte[] reply = ReadBytes();
            if (reply == null) return 0;
            if (reply.Length == 0) return 0;
            return reply[0];
        }

        public byte[] ReadBytes()
        {
            serialPort.DiscardInBuffer();
            serialPort.WriteLine("++read 256"); // request up to 256 bytes
            Thread.Sleep(50);
            byte[] buffer = new byte[1024];     // why not 256? 
            int read = serialPort.BaseStream.Read(buffer, 0, buffer.Length); // reads raw bytes
            byte[] result = new byte[read];
            Array.Copy(buffer, result, read);
            return result;
        }

        public void SendBytes(byte[] b)
        {
            serialPort.DiscardInBuffer();
            serialPort.Write(b, 0, b.Length);
            Thread.Sleep(50);
        }

        public void SendBytes(byte b) => SendBytes(new byte[] { b });

        private void InitializeSerialPort(string comPort)
        {
            // COM port parameters
            serialPort.PortName = comPort;
            serialPort.BaudRate = 115200;
            serialPort.Parity = Parity.None;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
            // RTS/CTS handshaking
            serialPort.Handshake = Handshake.RequestToSend;
            serialPort.DtrEnable = true;
            // Error handling
            serialPort.DiscardNull = false;
            serialPort.ParityReplace = 0;
            // additional settings from ChatGPT code example
            serialPort.ReadTimeout = 3000;
            serialPort.WriteTimeout = 2000;
            serialPort.NewLine = "\n";
            serialPort.Encoding = Encoding.ASCII;
        }

        private void InitializePrologix(int deviceAddress)
        {
            serialPort.WriteLine("++mode 1");    // controller
            serialPort.WriteLine($"++addr {deviceAddress}");    // instrument GPIB address
            serialPort.WriteLine("++auto 0");    // explicit reads
            serialPort.WriteLine("++eos 0");     // no EOS translation
            serialPort.WriteLine("++eoi 1");     // use EOI on GPIB writes
            Thread.Sleep(50);
        }

    }
}
