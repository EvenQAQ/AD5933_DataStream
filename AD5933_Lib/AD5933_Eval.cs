using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace AD5933_Lib
{
    //
    // see: https://www.researchgate.net/publication/292976112_Analog_Front-End_for_the_Integrated_Circuit_AD5933_Used_in_Electrical_Bioimpedance_Measurements
    //
    public class AD5933_Eval : IDisposable
    {
        public enum PgaGain {x1, x5}
        public enum OutputRange { Range1, Range2, Range3, Range4 }

        class PointPair
        {
            public ushort Real;
            public ushort Imaginary;
        }

        const uint VID_ANALOG = 0x0456;
        const uint PID_EVAL_AD5933EBZ = 0xB203;
        const byte RW_REQUEST = 0xDE;
        const byte DIR_READ = 1;
        const byte DIR_WRITE = 0;
        const int TIMEOUT_MAX = 100;
       
      


        private UInt16 m_handle = 0;
        private bool disposedValue = false; // To detect redundant calls
        private ushort pgaGain = AD5933_Control.PgaGain;    // if flag is set, gain is x5
        private ushort outputVoltage = AD5933_Control.Range1;
        private double gainFactorL = 1.0;
        private double gainFactorH = 1.0;

        public AD5933_Eval (byte part)
        {
            connect(part);
            // Download firmware after connected
            downloadFirmware("AD5933_34FW.hex");
        }

        ~AD5933_Eval()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        public static IEnumerable<byte> Boards
        {
            get
            {
                IntPtr numBoards = Marshal.AllocHGlobal(4);
                IntPtr partPath = Marshal.AllocHGlobal(255);

                try
                {
                    if (AD_CYUSB_USB4.Search_For_Boards(VID_ANALOG, PID_EVAL_AD5933EBZ, numBoards, partPath) == 0)
                    {
                        int numberOfBoards = Marshal.ReadInt32(numBoards);
                        // the function returns a number of pointers.
                        for (int i = 0; i < numberOfBoards; i++)
                        {
                            byte b = Marshal.ReadByte(partPath, i);
                            yield return b;
                        }
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(partPath);
                    Marshal.FreeHGlobal(numBoards);
                }

            }
        }

        #region Private 

        private bool connect(byte PartPath)
        {
            IntPtr handlePtr = Marshal.AllocHGlobal(sizeof(UInt16));
            try
            {
                if (AD_CYUSB_USB4.Connect(VID_ANALOG, PID_EVAL_AD5933EBZ, PartPath, handlePtr) == 0)
                {
                    m_handle = (UInt16)Marshal.ReadInt16(handlePtr, 0);
                    return true;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handlePtr);
            }
            return false;
        }

        private bool disconnect()
        {
            if (m_handle != 0)
            {

                if (0 == AD_CYUSB_USB4.Disconnect(m_handle))
                {
                    m_handle = 0;
                    return true;
                }
            }
            return false;
        }

        private bool downloadFirmware(string FirmwareFile)
        {

            if (m_handle == 0) throw new Exception("Invalid Handle");
            if (!File.Exists(FirmwareFile)) throw new Exception("Firmware file does not exist");


            IntPtr filePtr = Marshal.StringToHGlobalAnsi(FirmwareFile);
            try
            {

                if (0 == AD_CYUSB_USB4.Download_Firmware(m_handle, filePtr))
                    return true;
                else
                    return false;
            }
            finally
            {

                Marshal.FreeHGlobal(filePtr);
            }
        }

        private int regToFrequency(uint register)
        {
            double temp = register * (this.ClockFrequency / Math.Pow(2,29));
            return (int) (temp + 0.5);  // Round to the closest, not just down
        }

        private int freqToRegister(int frequency)
        {
            double temp = (long)frequency << 29;
            return (int)((temp / this.ClockFrequency) + 0.5);
        }

        private void writeToPort(byte Address, byte Value)
        {
            if (m_handle == 0) throw new Exception("Invalid Handle");

            ushort addrval = (ushort)((Value << 8) | Address);

            if (0 != AD_CYUSB_USB4.Vendor_Request(m_handle, RW_REQUEST, 0x0D, addrval, DIR_WRITE, 0, IntPtr.Zero))
                throw new Exception("WriteToPort: Vendor_Request");
        }

        private byte readFromPort(byte Address)
        {
            if (m_handle == 0) throw new Exception("Invalid Handle");

            IntPtr valPtr = Marshal.AllocHGlobal(1);
            try
            {

                if (0 != AD_CYUSB_USB4.Vendor_Request(m_handle, RW_REQUEST, 0x0D, Address, DIR_READ, 1, valPtr))
                    throw new Exception("ReadFromPort: Vendor_Request");
                byte result = Marshal.ReadByte(valPtr);
                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(valPtr);
            }
        }

        private ushort readRealData()
        {
            ushort temp = this.readFromPort(AD5933_Registers.RegRealDataMSB);
            temp <<= 8;
            temp |= this.readFromPort(AD5933_Registers.RegRealDataLSB);
            return temp;
        }

        private ushort readImaginaryData()
        {
            ushort temp = this.readFromPort(AD5933_Registers.RegImaginaryDataMSB);
            temp <<= 8;
            temp |= this.readFromPort(AD5933_Registers.RegImaginaryDataLSB);
            return temp;
        }

        private void writeControl (ushort Value)
        {
            Value = (ushort)(Value | pgaGain | outputVoltage);

            writeToPort(AD5933_Registers.RegControlLSB, (byte)(Value & 0xFF));
            Value >>= 8;
            writeToPort(AD5933_Registers.RegControlMSB, (byte)(Value & 0xFF));
        }

        private double calcGainFactor (double Resistance, double Magnitude)
        {
            return 1.0 / (Resistance * Magnitude);
        }

        private void enterStandbyMode()
        {
            writeControl(AD5933_Control.StandbyMode);
        }

        private void enterPowerDownMode()
        {
            writeControl(AD5933_Control.PowerDownMode);
        }

        private IEnumerable<PointPair> doSweep()
        {
            bool busy = true;
            bool first = true;
            int timeout = TIMEOUT_MAX;

            enterStandbyMode();
            writeControl(AD5933_Control.ExternalClock);
            writeControl(AD5933_Control.InitializeWithStartFreq);
            writeControl(AD5933_Control.StartFrequencySweep);

            while (busy)
            {
                byte status = readFromPort(AD5933_Registers.RegStatus);

                if ((status & AD5933_Status.DataValid) != 0)
                {
                    timeout = TIMEOUT_MAX;

                    yield return (new PointPair()
                    {
                        Real = this.readRealData(),
                        Imaginary = this.readImaginaryData()
                    });

                    if ((status & AD5933_Status.SweepComplete) != 0)
                        busy = false;
                    else
                        writeControl(AD5933_Control.IncrementFrequency);
                }
                else if (timeout > 0)
                {
                    if (first)
                    {
                   //     writeControl(AD5933_Control.RepeatFrequency);
                        first = false;
                    }
                    Thread.Sleep(5);
                    timeout--;
                }
            }
            enterStandbyMode();

        }

        


        #endregion

        public double ReadTemperature()
        {
            byte status;
            byte upper, lower;

            if (m_handle == 0) throw new Exception("Invalid Handle");

            writeControl(AD5933_Control.MeasureTemperature);  

            do
            {
                Thread.Sleep(5);
                status = readFromPort(AD5933_Registers.RegStatus);

            }
            while ((status & AD5933_Status.TemperatureValid) == 0);

            upper = readFromPort(AD5933_Registers.RegTempUpper);
            lower = readFromPort(AD5933_Registers.RegTempLower);

            int temperature = (upper << 8) | lower;
            if (temperature <= 0x1FFF)
                return (temperature / 32.0);
            else
                return (temperature - 16384) / 32.0;
        }

        public IObservable<Snapshot> DoSweep()
        {
            return Observable.Create<Snapshot>((obs) =>
            {
                try
                {
                    foreach (var snap in SweepMeasure())
                    {
                        obs.OnNext(snap);
                    }
                    obs.OnCompleted();
                }
                catch (Exception e)
                {
                    obs.OnError(e);
                }
                return Disposable.Create(() => { });
            });
        }

        public IEnumerable<Snapshot> SweepMeasure()
        {
            int currentFreq = this.StartFrequency;
            int stepFreq = this.IncFrequency;
            double deltaFactor = this.gainFactorH - this.gainFactorL;
            double deltaFrequency = 0.0;

            foreach (var point in doSweep())
            {
                deltaFrequency += stepFreq;
                //     double calibrationFactor = (this.GainFactorL + deltaFrequency * deltaFactor);
                double calibrationFactor = this.GainFactor;


                   yield return (new Snapshot(currentFreq,
                        this.readRealData(),
                        this.readImaginaryData(),
                        calibrationFactor
                        ));

                currentFreq += stepFreq;
            }
        }


        //
        // Refer to Form1.frm
        //
        public bool CalibrateMultipoint()
        {
            Snapshot[] snaps = new Snapshot[2];
            
            int count = 0;

            var saveIncFrequency = this.IncFrequency;
            var saveSteps = this.Steps;
            try
            {
                int totalRange = this.IncFrequency * this.Steps;

                this.Steps = 1;
                this.IncFrequency = totalRange;
                Thread.Sleep(100);

                foreach (var snap in SweepMeasure())
                {
                    if (count < 2)
                        snaps[count] = snap;
                    count++;
                }
                if (count == 2)
                {
                    this.gainFactorL = calcGainFactor(this.CalibrationResistor, snaps[0].Magnitude);
                    this.gainFactorH = calcGainFactor(this.CalibrationResistor, snaps[1].Magnitude);
                    this.GainFactor = (gainFactorL + gainFactorH) / 2.0;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
            finally
            {
                this.Steps = saveSteps;
                this.IncFrequency = saveIncFrequency;
            }
        }

        public int StartFrequency
        {
            get
            {
                uint temp = this.readFromPort(AD5933_Registers.RegStartFreqMSB);
                temp <<= 8;
                temp |= this.readFromPort(AD5933_Registers.RegStartFreq);
                temp <<= 8;
                temp |= this.readFromPort(AD5933_Registers.RegStartFreqLSB);
                return regToFrequency(temp);
            }
            set
            {
                int register = this.freqToRegister(value);

                this.writeToPort(AD5933_Registers.RegStartFreqLSB, (byte)(register & 0xFF));
                register >>= 8;
                this.writeToPort(AD5933_Registers.RegStartFreq, (byte)(register & 0xFF));
                register >>= 8;
                this.writeToPort(AD5933_Registers.RegStartFreqMSB, (byte)(register & 0xFF));
            }
        }
        public ushort Steps
        {
            get
            {
                ushort temp = this.readFromPort(AD5933_Registers.RegIncrementsMSB);
                temp <<= 8;
                temp |= this.readFromPort(AD5933_Registers.RegIncrementsLSB);
                return temp;
            }
            set
            {
                this.writeToPort(AD5933_Registers.RegIncrementsLSB, (byte)(value & 0xFF));
                value >>= 8;
                this.writeToPort(AD5933_Registers.RegIncrementsMSB, (byte)(value & 0xFF));
            }
        }
        public int IncFrequency
        {
            get
            {
                uint temp = this.readFromPort(AD5933_Registers.RegFreqIncrMSB);
                temp <<= 8;
                temp |= this.readFromPort(AD5933_Registers.RegFreqIncr);
                temp <<= 8;
                temp |= this.readFromPort(AD5933_Registers.RegFreqIncrLSB);

                return regToFrequency(temp);
            }
            set
            {
                int register = this.freqToRegister(value);

                this.writeToPort(AD5933_Registers.RegFreqIncrLSB, (byte)(register & 0xFF));
                register >>= 8;
                this.writeToPort(AD5933_Registers.RegFreqIncr, (byte)(register & 0xFF));
                register >>= 8;
                this.writeToPort(AD5933_Registers.RegFreqIncrMSB, (byte)(register & 0xFF));
            }
        }
        public ushort SettlingCycles
        {
            get
            {
                ushort temp = this.readFromPort(AD5933_Registers.RegSettlingTimeMSB);
                temp <<= 8;
                temp |= this.readFromPort(AD5933_Registers.RegSettlingTimeLSB);
                return temp;
            }
            set
            {
                this.writeToPort(AD5933_Registers.RegSettlingTimeLSB, (byte)(value & 0xFF));
                value >>= 8;
                this.writeToPort(AD5933_Registers.RegSettlingTimeMSB, (byte)(value & 0xFF));
            }
        }
        // public double DataStreamT { get; set; } = 1.0; 
//        public double GainFactorL { get; private set; } = 1.0;
//        public double GainFactorH { get; private set; } = 1.0;
        public double GainFactor { get; private set; } = 1.0;
        public double CalibrationResistor { get; set; } = 330E3;
        public UInt32 ClockFrequency { get; set; } = 16000000;
        public PgaGain PGAControl
        {
            get
            {
                if (pgaGain == 0)
                    return PgaGain.x5;
                else
                    return PgaGain.x1;
            }
            set
            {
                if (value == PgaGain.x1)
                    pgaGain = AD5933_Control.PgaGain;
                else
                    pgaGain = 0;


                writeControl(0); // Write Control will update these values
            }
        }

        public OutputRange ExcitationVoltage
        {
            get
            {
                switch (outputVoltage)
                {
                    case AD5933_Control.Range1:
                        return AD5933_Eval.OutputRange.Range1;
                    case AD5933_Control.Range2:
                        return AD5933_Eval.OutputRange.Range2;
                    case AD5933_Control.Range3:
                        return AD5933_Eval.OutputRange.Range3;
                    case AD5933_Control.Range4:
                        return AD5933_Eval.OutputRange.Range4;
                    default:
                        throw new Exception("Unexpected in OutputRange");

                }
            }
            set
            {
                switch (value)
                {
                    case AD5933_Eval.OutputRange.Range1:
                        outputVoltage = AD5933_Control.Range1;
                        break;
                    case AD5933_Eval.OutputRange.Range2:
                        outputVoltage = AD5933_Control.Range2;
                        break;
                    case AD5933_Eval.OutputRange.Range3:
                        outputVoltage = AD5933_Control.Range3;
                        break;
                    case AD5933_Eval.OutputRange.Range4:
                        outputVoltage = AD5933_Control.Range4;
                        break;
                    default:
                        throw new Exception("Unexpected in OutputRange");

                }
                writeControl(0); // Write Control will update these values
            }
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose objects held by this.
                }

                if (m_handle != 0)
                {
                    disconnect();
                    m_handle = 0;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        
        
        #endregion
    }
}
