using Bev.IO.RemoteInterface;
using System;
using System.IO.Ports;
using System.Threading;

namespace RS232Interface
{
    public class SerialInterface : IRemoteInterface
    {

        public SerialInterface(string portName)
        {
            comPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
            comPort.Handshake = Handshake.None;
            //comPort.NewLine = "\r";
            comPort.ReadTimeout = hugeDelay;
            comPort.WriteTimeout = hugeDelay;
            comPort.DtrEnable = true;
            comPort.RtsEnable = false;
            OpenPort();
        }

        public string Enter(int address)
        {
            try
            {
                char[] buffer = new char[512];
                comPort.Read(buffer, 0, buffer.Length);
                char[] charsToTrim = { (char)0, '\r', '\n' };
                return new string(buffer).TrimEnd(charsToTrim);
            }
            catch (TimeoutException)
            {
                Console.WriteLine(">read timeout<");
                return string.Empty;
            }
        }

        public void Output(int address, string command)
        {
            try
            {
                comPort.Write(command);
                Thread.Sleep(defaultDelay);
            }
            catch (TimeoutException)
            {
                Console.WriteLine(">write timeout<");
            }
        }

        public void Local(int address) => NoOp();
        public void Remote(int address) => NoOp();
        public void Trigger(int address) => NoOp();

        private void OpenPort()
        {
            try
            {
                if (!comPort.IsOpen)
                    comPort.Open();
            }
            catch (Exception)
            { }
        }

        private void ClosePort()
        {
            try
            {
                if (comPort.IsOpen)
                    comPort.Close();
            }
            catch (Exception)
            { }
        }

        private void NoOp() { }

        private readonly SerialPort comPort;
        private const int defaultDelay = 500;   // in ms
        private const int hugeDelay = 8000;     // in ms

    }
}
