using System;

namespace AD5933_Lib
{
    public class Snapshot
    {
        private ushort realPart;
        private ushort imaginaryPart;
        private double gainFactor;
        

        public Snapshot(int Frequency, ushort RealPart, ushort ImaginaryPart, double GainFactor)
        {
            this.Frequency = Frequency;
            this.gainFactor = GainFactor;
            this.realPart = RealPart;
            this.imaginaryPart = ImaginaryPart;
        }

        private int twosComplement(ushort Val)
        {
            if (Val <= 0x7FFF)
                return Val;
            else
                return Val - 65536;
        }

        public int Frequency { get; }

        public int RealPart
        {
            get { return twosComplement(realPart); }
        }

        public int ImaginaryPart
        {
            get { return twosComplement(imaginaryPart); }
        }

        public double Magnitude
        {
            get {
              
                return Math.Sqrt(Math.Pow(RealPart, 2) + Math.Pow(ImaginaryPart, 2));
            }
        }

        public double Impedance
        {
            get
            {
                return 1.0 / (this.gainFactor * Magnitude);
            }
        }

        public double Phase
        {
            get
            {
                int real = RealPart;
                int imaginary = ImaginaryPart;

                if ((real > 0) && (imaginary > 0))
                    return Math.Atan((double)ImaginaryPart / (double)RealPart);
                if ((real > 0) && (imaginary < 0))
                    return Math.Atan((double)ImaginaryPart / (double)RealPart);
                if ((real < 0) && (imaginary > 0))
                    return Math.Atan((double)ImaginaryPart / (double)RealPart) + Math.PI;
                if ((real < 0) && (imaginary < 0))
                    return Math.Atan((double)ImaginaryPart / (double)RealPart) - Math.PI;
                return 0.0;
            }
        }
    }

}
