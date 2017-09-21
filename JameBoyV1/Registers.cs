using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JameBoyV1
{
    //this is strictly CPU registers

    public sealed class Registers
    {
        private static Registers instance = null;
        private static readonly object padlock = new object();
        Registers() { }

        public static Registers Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Registers();
                    }
                    return instance;
                }

                
            }
        }

        public RegisterHL AF = new RegisterHL();
        public RegisterHL BC = new RegisterHL();
        public RegisterHL DE = new RegisterHL();
        public RegisterHL HL = new RegisterHL();
        

        public ushort _SP;      //stack pointer
        public ushort _PC;      //program counter

        

        public ushort SP { get; set; }
        public ushort PC { get; set; }

        

        public byte A
        {
            get
            {
                return AF.hi;
            }
            set
            {
                AF.hi = value;
            }
        }
        public byte F
        {
            get
            {
                return AF.lo;
            }
            set
            {
                AF.lo = value;
            }
        }
        public byte B
        {
            get
            {
                return BC.hi;
            }
            set
            {
                BC.hi = value;
            }
        }
        public byte C
        {
            get
            {
                return BC.lo;
            }
            set
            {
                BC.lo = value;
            }
        }
        public byte D
        {
            get
            {
                return DE.hi;
            }
            set
            {
                DE.hi = value;
            }
        }
        public byte E
        {
            get
            {
                return DE.lo;
            }
            set
            {
                DE.lo = value;
            }
        }
        public byte H
        {
            get
            {
                return HL.hi;
            }
            set
            {
                HL.hi = value;
            }
        }
        public byte L
        {
            get
            {
                return HL.lo;
            }
            set
            {
                HL.lo = value;
            }
        }

        public void zeroFlag(bool setting) //setter
        {
            if(setting)
            {
                F |= 128;
            }
            else
            {
                F &= 127;
            }
        } 
        public bool zeroFlag() //getter
        {
            return !(F & 128).Equals(0);
        }

        public void addSubFlag(bool setting)
        {
            if (setting)
            {
                F |= 64;
            }
            else
            {
                F &= 191;
            }
        }
        public bool addSubFlag()
        {
            return !(F & 64).Equals(0);
        }

        public void halfCarryFlag(bool setting)
        {
            if (setting)
            {
                F |= 32;
            }
            else
            {
                F &= 223;
            }
        }
        public bool halfCarryFlag()
        {
            return !(F & 32).Equals(0);
        }

        public void carryFlag(bool setting)
        {
            if (setting)
            {
                F |= 16;
            }
            else
            {
                F &= 239;
            }
        }
        public bool carryFlag()
        {
            return !(F & 16).Equals(0);
        }

        public void maskFlags(byte mask)
        {
            F &= mask;
        }


        public class RegisterHL
        {
            //public RegisterHL() { }


            //private ushort _word;



            private byte _hi;
            private byte _lo;


            public byte hi { get { return _hi; } set { _hi = value; } }

            public byte lo { get { return _lo; } set { _lo = value; } }



            public ushort word
            {
                get
                {
                    return (ushort)((_hi * 256) + _lo);
                }
                set
                {
                    _hi = (byte)(value / 256);
                    _lo = (byte)(value % 256);
                }
            }



            public void wordLoad(ushort word)
            {
                hi = (byte)(word / 256);
                lo = (byte)(word % 256);
            }
        }
    }
}
