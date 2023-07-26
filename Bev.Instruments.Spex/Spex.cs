using System;
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

        public Spex(int deviceAddress)
        {
            interpreter = new SpexCommandInterpreter(deviceAddress);
            wlConv = new WavelengthConverter();
            Initialize();
        }

        public int DeviceAddress => interpreter.DeviceAddress;
        public int CurrentPosition => GetCurrentStepPosition();
        public WavelengthConverter WavelengthCalibration => wlConv;
        public string InstrumentManufacturer => "Jobin-Yvon / SPEX";
        public string InstrumentType => GetInstrumentType();
        public string InstrumentSerialNumber => GetDeviceSerialNumber();
        public string InstrumentFirmwareVersion => GetDeviceFirmwareVersion();
        public string InstrumentID => $"{InstrumentType} SN:{InstrumentSerialNumber} {InstrumentFirmwareVersion} @ {DeviceAddress:D2}";

        public void SetCurrentStepPosition(int stepPosition)
        {
            interpreter.Send($"F0,{stepPosition}");
            interpreter.ReadSingleCharacter();
        }

        public int GetCurrentStepPosition()
        {
            string str = interpreter.Query("H0");
            return int.TryParse(str, out int value) ? value : -1; // good old C++ error return value
        }

        public void MoveRelativeSteps(int steps)
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

        public void MoveAbsoluteSteps(int position)
        {
            int currentPos = GetCurrentStepPosition();
            int steps = position - currentPos;
            MoveRelativeWavelength(steps);
        }

        public double GetCurrentWavelength() => wlConv.StepsToWavelength(GetCurrentStepPosition());

        public void MoveRelativeWavelength(double wavelength) => MoveRelativeSteps(wlConv.WavelengthToSteps(wavelength)); // this is wrong! (offset)

        public void MoveAbsoluteWavelength(double wavelength) => MoveAbsoluteSteps(wlConv.WavelengthToSteps(wavelength));

        public void MotorInit()
        {
            interpreter.Send("A");
            interpreter.ReadSingleCharacter();
        }

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
            string zStr = interpreter.Query("z");
            string yStr = interpreter.Query("y");
            return $"{zStr} - {yStr}";
        }

        private void MoveRelativeGeneric(int steps)
        {
            interpreter.Query($"F0,{steps}");
        }

        private void ReturnOnHalt()
        {
            while (IsBusy()) { }
        }

        private bool IsBusy()
        {
            interpreter.Send("E");
            string str = interpreter.ReadSingleCharacter();
            if (str.Contains("q")) return true;
            return false;
        }

        private void Initialize()
        {
            // Manual p 26-27
            ReBootIfHung();
            //FlushInputBuffer() ?? how ??
            string response = WhereAmI();
            if (response == "F")
            {
                return;
            }
            if (response == "B")
            {
                StartUpControllerMainProgram();
                MotorInit();
            }
        }

        private void ReBootIfHung()
        {
            interpreter.Send((byte)222);
            Thread.Sleep(500);
        }

        private string WhereAmI()
        {
            interpreter.Send(" ");
            return interpreter.ReadSingleCharacter();
        }

        private void StartUpControllerMainProgram()
        {
            byte[] buffer = { 0x4F, 0x30, 0x30, 0x30, 0x00 };
            interpreter.Send(buffer);
            Thread.Sleep(500);
            string response = interpreter.ReadSingleCharacter();
            if (response == "*")
                return;
            throw new Exception("SPEX controller did not enter main program");
        }

        private string StripFirstChar(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            if (str.Length == 1) return str;
            return str.Substring(1);
        }

        private readonly SpexCommandInterpreter interpreter;
        private WavelengthConverter wlConv;
        private const int noDelay = 0;
        private const int defaultDelay = 100;
        private const int backlashSteps = 500;
    }
}
