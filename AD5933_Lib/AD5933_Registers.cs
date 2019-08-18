using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AD5933_Lib
{
   //
   // The register values are specified in the datasheet: AD5933 Datasheet.pdf
   //
    static class AD5933_Control
    {
        // D0 to D2 not used and must be zero
        public const ushort ExternalClock = 0x0008;
        public const ushort Reset = 0x0010;
        // D5 to D7 not used and must be zero
        public const ushort PgaGain = 0x0100;   // 0 = x5, 1 = x1
        //   0000 0XX0 0000000
        public const ushort Range1 = 0x0000;    // 2.0V p-p
        public const ushort Range2 = 0x0600;    // 1V p-p
        public const ushort Range3 = 0x0400;    // 400mV p-p
        public const ushort Range4 = 0x0200;    // 200mV p-p

        public const ushort InitializeWithStartFreq = 0x1000;
        public const ushort StartFrequencySweep = 0x2000;
        public const ushort IncrementFrequency = 0x3000;
        public const ushort RepeatFrequency = 0x4000;
        public const ushort MeasureTemperature = 0x9000;
        public const ushort PowerDownMode = 0xA000;
        public const ushort StandbyMode = 0xB000;

    }

    static class AD5933_Status
    {
        public const byte TemperatureValid = 0x01;
        public const byte DataValid = 0x02;
        public const byte SweepComplete = 0x04;
       
    }

    static class AD5933_Registers
    {


        public const byte RegControlMSB = 0x80;
        public const byte RegControlLSB = 0x81;

        // Start Frequency
        public const byte RegStartFreqMSB = 0x82;
        public const byte RegStartFreq = 0x83;
        public const byte RegStartFreqLSB = 0x84;

        // Frequency increments
        public const byte RegFreqIncrMSB = 0x85;
        public const byte RegFreqIncr = 0x86;
        public const byte RegFreqIncrLSB = 0x87;

        // Number of increments
        public const byte RegIncrementsMSB = 0x88;
        public const byte RegIncrementsLSB = 0x89;

        //Settling time
        public const byte RegSettlingTimeMSB = 0x8A;
        public const byte RegSettlingTimeLSB = 0x8B;



        // Status Register
        public const byte RegStatus = 0x8F;


        // Temperature reading
        public const byte RegTempUpper = 0x92;
        public const byte RegTempLower = 0x93;

        // Real data
        public const byte RegRealDataMSB = 0x94;
        public const byte RegRealDataLSB = 0x95;

        // Imaginary data
        public const byte RegImaginaryDataMSB = 0x96;
        public const byte RegImaginaryDataLSB = 0x97;
    }
}
