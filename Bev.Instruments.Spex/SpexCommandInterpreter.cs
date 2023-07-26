using System.Text;

namespace Bev.Instruments.Spex
{
    internal class SpexCommandInterpreter
    {

        private readonly Ieee488Handler ieee;

        internal SpexCommandInterpreter(int deviceAddress)
        {
            ieee = new Ieee488Handler(deviceAddress);
        }

        internal int DeviceAddress => ieee.DeviceAddress;

        internal void Send(byte b) => ieee.SendBytes(b);

        internal void Send(byte[] buffer) => ieee.SendBytes(buffer);

        internal void Send(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return;
            if (command.Length > 1)
            {
                command = command + "\r";
            }
            byte[] buffer = Encoding.ASCII.GetBytes(command);
            ieee.SendBytes(buffer);
        }

        internal string Read()
        {
            byte[] buffer = ieee.ReadBytes();
            string answer = Encoding.ASCII.GetString(buffer);
            string firstLetter = answer.Substring(0, 1);
            answer = Clean(answer);
            return answer;
        }

        internal string ReadSingleCharacter()
        {
            byte b = ieee.ReadByte();
            string answer = Encoding.ASCII.GetString(new byte[] { b });
            return answer;
        }

        internal string Query(string command)
        {
            Send(command);
            return Read();
        }


        private string CleanFromNewline(string original) => original.TrimEnd('\r', '\n');

        private string RemoveConfirmation(string original) => original.TrimStart('o');

        private string Clean(string original) => RemoveConfirmation(CleanFromNewline(original));

    }
}
