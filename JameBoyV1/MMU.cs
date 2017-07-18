using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JameBoyV1
{
    public sealed class MMU
    {
        private static MMU instance = null;
        private static readonly object padlock = new object();
        MMU() { }

        public static MMU Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new MMU();
                    }
                    return instance;
                }
            }
        }

        byte currentByte = 0;
        ushort currentUShort = 0;

        Registers reg = Registers.Instance;

        public byte[] RAM = new byte[0xFFFF + 1];

        //        Interrupt Enable Register
        //--------------------------- FFFF
        //Internal RAM
        //--------------------------- FF80
        //Empty but unusable for I/O
        //--------------------------- FF4C
        //I/O ports
        //--------------------------- FF00
        //Empty but unusable for I/O
        //--------------------------- FEA0
        //Sprite Attrib Memory(OAM)
        //--------------------------- FE00
        //Echo of 8kB Internal RAM
        //--------------------------- E000
        //8kB Internal RAM
        //--------------------------- C000
        //8kB switchable RAM bank
        //--------------------------- A000
        //8kB Video RAM
        //--------------------------- 8000 --
        //16kB switchable ROM bank |
        //--------------------------- 4000 |= 32kB Cartrigbe
        //16kB ROM bank #0 |
        //--------------------------- 0000 --
        //* NOTE: b = bit, B = byte

        public void initalROMLoad(byte[] rom)
        {
            Buffer.BlockCopy(rom, 0, RAM, 0, rom.Count());
        }

        public byte readByte(ushort programCounter)
        {
            currentByte = RAM[programCounter];
            return currentByte;
        }

        public byte readByte()
        {
            currentByte = RAM[reg.PC];
            reg.PC++;
            return currentByte;
        }

        public ushort readWord(ushort address)
        {
            return (ushort)((RAM[address + 1] * 256) + RAM[address]); //little endian
        }

        public ushort readWord()
        {
            currentUShort = (ushort)((RAM[reg.PC + 1] * 256) + RAM[reg.PC]);//little endian
            reg.PC += 2;
            return currentUShort; //little endian
        }

        public void writeByte(byte b, ushort address)
        {
            RAM[address] = b;
        }

        public void incrementByte(ushort address)
        {
            RAM[address]++;
        }
        public void decrementByte(ushort address)
        {
            RAM[address]--;
        }

        public void writeWordToStack(ushort w)
        {
            reg.SP--;
            writeByte((byte)(w / 256), reg.SP);
            reg.SP--;
            writeByte((byte)(w % 256), reg.SP);
        }

        public ushort readWordFromStack()
        {
            currentUShort = 0;
            currentUShort += (ushort)(readByte(reg.SP) * 256);
            reg.SP++;
            currentUShort += readByte(reg.SP);
            reg.SP++;

            return currentUShort;
        }
    }
}
