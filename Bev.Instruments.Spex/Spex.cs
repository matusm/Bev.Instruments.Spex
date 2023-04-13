using System.Threading;
using Bev.IO.RemoteInterface;

namespace Bev.Instruments.Spex
{
    public class Spex
    {
        #region Site specific values! Modify on use!
        /**********************************************/
        /* There is no documented way to obtain the   */
        /* instrument`s serial number automatically.  */
        /* This very site specific method infers the  */
        /* serial number from the GBIP address.       */
        /**********************************************/
        private string GetDeviceSerialNumberForBevLab()
        {
            switch (DeviceAddress)
            {
                case 1:
                    return "1930";
                case 3:
                    return "4867";
                default:
                    return "---";
            }
        }

        private string GetInstrumentTypeForBevLab()
        {
            switch (DeviceAddress)
            {
                case 1:
                    return "1680 B";
                case 3:
                    return "1681 B";
                default:
                    return "---";
            }
        }
        #endregion

        public Spex(int deviceAddress, IRemoteInterface remoteHandler)
        {
            DeviceAddress = deviceAddress;
            RemoteHandler = remoteHandler;
            wlConv = new WavelengthConverter();
            Initialize();
        }

        public IRemoteInterface RemoteHandler { get; }
        public int DeviceAddress { get; }
        public string InstrumentManufacturer => "Jobin-Yvon / SPEX";
        public string InstrumentType => GetInstrumentType();
        public string InstrumentSerialNumber => GetDeviceSerialNumber();
        public string InstrumentFirmwareVersion => GetDeviceFirmwareVersion();
        public string InstrumentID => $"{InstrumentType} SN:{InstrumentSerialNumber} {InstrumentFirmwareVersion} @ {DeviceAddress:D2}";

        public int GetPositionRaw()
        {
            string str = StripFirstChar(OutputEnter("H0"));
            return int.TryParse(str, out int value) ? value : -1; // good old C++ error return value
        }

        public void MoveRelativeRaw(int steps)
        {
            // TODO check for valid range
            if (steps == 0) return;
            if (steps > 0)
            {
                MoveRelativeGeneric(steps);
                ReturnOnHalt();
            }
            if (steps < 0)
            {
                MoveRelativeGeneric(steps - backlashSteps);
                ReturnOnHalt();
                MoveRelativeGeneric(backlashSteps);
                ReturnOnHalt();
            }
        }

        public void MoveAbsoluteRaw(int position)
        {
            int currentPos = GetPositionRaw();
            int steps = position - currentPos;
            MoveRelative(steps);
        }

        public double GetPosition() => wlConv.StepsToWavelength(GetPositionRaw());

        public void MoveRelative(double wavelength) => MoveRelativeRaw(wlConv.WavelengthToSteps(wavelength));

        public void MoveAbsolute(double wavelength) => MoveAbsoluteRaw(wlConv.WavelengthToSteps(wavelength));

        private string GetInstrumentType()
        {
            // there is no documented way to obtain the type
            // use a hard coded dicitonary
            return GetInstrumentTypeForBevLab();
        }

        private string GetDeviceSerialNumber()
        {
            // there is no documented way to obtain the serial number
            // use a hard coded dicitonary
            return GetDeviceSerialNumberForBevLab();
        }

        private string GetDeviceFirmwareVersion()
        {
            string zStr = StripFirstChar(OutputEnter("z"));
            string yStr = StripFirstChar(OutputEnter("y"));
            return $"{zStr} - {yStr}";
        }

        private void MoveRelativeGeneric(int steps) => OutputEnter($"F0,{steps}");

        private void ReturnOnHalt()
        {
            while (IsBusy()) { }
        }

        private bool IsBusy()
        {
            string str = OutputEnter("E");
            if (str.Contains("q")) return true;
            return false;
        }

        private void Initialize()
        {
            RemoteHandler.Remote(DeviceAddress);
            OutputEnter(" ", 200);
            StartMainProgram();
        }

        private void StartMainProgram()
        {
            char[] buff = { 'O', '0', '0', '0', (char)0 };
            OutputEnter(new string(buff), 500);
        }

        private string OutputEnter(string command, int delay)
        {
            RemoteHandler.Output(DeviceAddress, command);
            Thread.Sleep(delay);
            return RemoteHandler.Enter(DeviceAddress);
        }

        private string OutputEnter(string command) => OutputEnter(command, noDelay);

        private string StripFirstChar(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            if (str.Length == 1) return str;
            return str.Substring(1);
        }

        private WavelengthConverter wlConv;
        private const int noDelay = 0;
        private const int defaultDelay = 100;
        private const int backlashSteps = 500;
    }
}
