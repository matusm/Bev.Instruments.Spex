﻿using System;
using System.Text;
using System.Threading;

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

        /**********************************************/
        /* There is no documented way to obtain the   */
        /* instrument type automatically.             */
        /* This very site specific method infers the  */
        /* type from the GBIP address.                */
        /**********************************************/
        private string GetInstrumentTypeForBevLab()
        {
            switch (DeviceAddress)
            {
                case 1:
                    return "1680B";
                case 3:
                    return "1681B";
                default:
                    return "---";
            }
        }
        #endregion

        public Spex(int deviceAddress, IWavelengthConverter wavelengthConverter)
        {
            interpreter = new SpexCommandInterpreter(deviceAddress);
            waveConverter = wavelengthConverter;
            Initialize();
        }

        public int DeviceAddress => interpreter.DeviceAddress;
        public int CurrentPosition => GetCurrentStepPosition();
        public bool HitAnyLimitSwitch => hitLowerLimitSwitch || hitUpperLimitSwitch;
        public IWavelengthConverter WavelengthCalibration => waveConverter;
        public string InstrumentManufacturer => "Jobin-Yvon/SPEX";
        public string InstrumentType => GetInstrumentType();
        public string InstrumentSerialNumber => GetDeviceSerialNumber();
        public string InstrumentFirmwareVersion => GetDeviceFirmwareVersion();
        public string InstrumentID => $"{InstrumentManufacturer} {InstrumentType} SN:{InstrumentSerialNumber} {InstrumentFirmwareVersion} @ {DeviceAddress:D2}";

        public void SetCurrentStepPosition(int stepPosition)
        {
            interpreter.Send($"G0,{stepPosition}");
            interpreter.ReadSingleCharacter();
            ClearLimitSwitchFlag(); // really?
        }

        public int GetCurrentStepPosition()
        {
            // negative step positions are valid!
            string str = interpreter.Query("H0");
            return int.TryParse(str, out int value) ? value : int.MinValue;
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
                QueryLimitSwitchStatus();
                MoveRelativeGeneric(backlashSteps);
                ReturnOnHalt();
            }
            QueryLimitSwitchStatus();
        }

        public void MoveAbsoluteSteps(int position)
        {
            int currentPos = GetCurrentStepPosition();
            int steps = position - currentPos;
            MoveRelativeSteps(steps);
        }

        public double GetCurrentWavelength() => waveConverter.StepsToWavelength(GetCurrentStepPosition());

        public void MoveRelativeWavelength(double interval) => MoveAbsoluteWavelength(GetCurrentWavelength() + interval);

        public void MoveAbsoluteWavelength(double wavelength) => MoveAbsoluteSteps(waveConverter.WavelengthToSteps(wavelength));

        public void MotorInit()
        {
            interpreter.Send("A");
            Thread.Sleep(defaultDelay);
            interpreter.ReadSingleCharacter();
        }

        public void SetMotorSpeed(int minFrequency, int maxFrequency, int rampTime)
        {
            if (minFrequency < 100) return;
            if (maxFrequency < 100) return;
            if (rampTime < 100) return;
            if (minFrequency > 80000) return;
            if (maxFrequency > 80000) return;
            if (rampTime > 65535) return;
            if (minFrequency > maxFrequency) return;
            interpreter.Query($"B0,{minFrequency},{maxFrequency},{rampTime}");
        }

        public string GetMotorSpeed() => interpreter.Query("C0");

        public int GetMotorSpeed(MotorSpeedParameter parameter)
        {
            string str = GetMotorSpeed();
            string[] tokens = str.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length != 3)
                return -1;  // good old C++ error return value
            int value;
            switch (parameter)
            {
                case MotorSpeedParameter.MinFrequency:
                    return int.TryParse(tokens[0], out value) ? value : -1;
                case MotorSpeedParameter.MaxFrequency:
                    return int.TryParse(tokens[1], out value) ? value : -1;
                case MotorSpeedParameter.RampTime:
                    return int.TryParse(tokens[2], out value) ? value : -1;
                default:
                    return -1;
            }
        }

        public void ClearLimitSwitchFlag()
        {
            hitLowerLimitSwitch = false;
            hitUpperLimitSwitch = false;
        }

        private void QueryLimitSwitchStatus()
        {
            byte b = GetLimitSwitchStatus();
            InterpretStatusByte(b);
            //Console.WriteLine(LimitSwitchStatusToString(b)); // comment out for release
        }

        private string LimitSwitchStatusToString(byte b) => $"{Convert.ToString(b, toBase: 2).PadLeft(8, '0'),8}";

        private byte GetLimitSwitchStatus()
        {
            string str = interpreter.Query("K");
            byte[] buffer = Encoding.ASCII.GetBytes(str);
            if (buffer.Length == 1) 
                return buffer[0];
            return 0;
        }

        private void InterpretStatusByte(byte b)
        {
            if ((b & 0b00000001) != 0) hitLowerLimitSwitch = true;
            if ((b & 0b00000010) != 0) hitUpperLimitSwitch = true;
        }

        private string GetInstrumentType() => GetInstrumentTypeForBevLab();

        private string GetDeviceSerialNumber() => GetDeviceSerialNumberForBevLab();

        private string GetDeviceFirmwareVersion()
        {
            string zStr = interpreter.Query("z"); // main program version
            string yStr = interpreter.Query("y"); // boot program version
            return $"{zStr}"; // for the time being the main program is of interest
        }

        private void MoveRelativeGeneric(int steps) => interpreter.Query($"F0,{steps}");

        private void ReturnOnHalt()
        {
            while (IsBusy()) 
            {
                UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            // this is the place to raise an event
            double wl = GetCurrentWavelength();
            //Console.WriteLine($"   -> {wl:F2} nm"); // comment out for release
        }

        private bool IsBusy()
        {
            // the manual (p 53) is misleading!
            interpreter.Send("E");
            string str = interpreter.Read();
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
            Thread.Sleep(defaultDelay);
        }

        private string WhereAmI()
        {
            interpreter.Send(" ");
            return interpreter.ReadSingleCharacter();
        }

        private void StartUpControllerMainProgram()
        {
            interpreter.Send(new byte[] { 0x4F, 0x30, 0x30, 0x30, 0x00 });
            Thread.Sleep(defaultDelay);
            string response = interpreter.ReadSingleCharacter();
            if (response == "*")
                return;
            throw new Exception("SPEX controller did not enter main program");
        }

        private readonly SpexCommandInterpreter interpreter;
        private IWavelengthConverter waveConverter;
        private bool hitLowerLimitSwitch;
        private bool hitUpperLimitSwitch;
        private const int defaultDelay = 600;
        private const int backlashSteps = 500;
    }
}
