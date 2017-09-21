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

        private static byte[] RAM = new byte[0xFFFF + 1];

        #region Video Registers
        public byte LCDControlRegister
        {
            get
            {
                return RAM[0xFF40];
            }
        }

        public byte LCDStatusRegister
        {
            get
            {
                return RAM[0xFF41];
            }
        }

        public int[] BGPalette
        {
            //bit 0 and 1 - color 0, 2 and 3 - 1, 4 and 5 - 2, 6 and 7 - 3
            //colours are 0 - white to 3 - black
            get
            {
                int[] palette = new int[4];
                for (int i = 0;i<4;i++)
                {
                    palette[i] = (RAM[0xFF47] & (3 << i * 2)) >> i * 2;
                }

                return palette;
            }
        }

        public int[] OBPalette0
        {
            get
            {
                int[] palette = new int[4];
                for (int i = 0; i < 4; i++)
                {
                    palette[i] = (RAM[0xFF48] & (3 << i * 2)) >> i * 2;
                }

                return palette;
            }
        }

        public int[] OBPalette1
        {
            get
            {
                int[] palette = new int[4];
                for (int i = 0; i < 4; i++)
                {
                    palette[i] = (RAM[0xFF49] & (3 << i * 2)) >> i * 2;
                }

                return palette;
            }
        }

        public byte scrollY
        {
            get
            {
                return RAM[0xFF42];
            }
        }

        public byte scrollX
        {
            get
            {
                return RAM[0xFF43];
            }
        }

        public byte LY
        {
            get
            {
                return RAM[0xFF44];
            }
        }

        public byte LYCompare
        {
            //TODO: this one is weird
            get
            {
                return RAM[0xFF45];
            }
        }

        public byte windowY
        {
            get
            {
                return RAM[0xFF4A];
            }
        }

        public byte windowX //minus 7
        {
            get
            {
                return RAM[0xFF4B];
            }
        }

        public byte DMATransferStartAddress;
        #endregion

        //Memory Map
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

        #region RAM Operations
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
            //when certain areas of memory are written to it can trigger an interrupt
            //this and the increment, decrement, and rom load functions are the only ones that write to the ram, I don't know if these are used to trigger interrupts 


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
        #endregion

        #region Video ram methods
        public byte[,] BGTileMap()
        {
            byte[,] tileMap = new byte[32, 32];

            //true: 9C00 - 9FFFF, false: 9800 - 9BFF
            for(int i = 0;i<32;i++)
            {
                for(int j = 0;j<32;j++)
                {
                    tileMap[i, j] = RAM[0x9800 + (i * 32) + j + (BGTileMapDisplaySelect ? 0x400 : 0)];
                }
            }

            return tileMap;
        }

        public byte[,] WindowBGTileMap()
        {

            byte[,] tileMap = new byte[32, 32];

            //true: 9C00 - 9FFFF, false: 9800 - 9BFF
            for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    tileMap[i, j] = RAM[0x9800 + (i * 32) + j + (WindowTileMapDisplaySelect ? 0x400 : 0)];
                }
            }

            return tileMap;
        }

        public Dictionary<int,Tile> TileSet()
        {
            Dictionary<int, Tile> tiles = new Dictionary<int, Tile>();
            List<byte> tileBytes = new List<byte>();
            //true: 8000 - 8FFF, false: 8800 - 97FF
            if(BGWindowTileDataSelect)
            {
                 for(int i = 0; i < 256;i ++)
                {
                    for(int j =0; j< 16;j++)
                    {
                        tileBytes.Add(RAM[0x8000 + i * 16 + j]);
                    }
                    tiles.Add(i, new Tile(tileBytes));
                }
            }
            else
            {
                for (int i = -127; i < 129; i++)
                {
                    for (int j = 0; j < 16; j++)
                    {
                        tileBytes.Add(RAM[0x8800 + i * 16 + j]);
                    }
                    tiles.Add(i, new Tile(tileBytes));
                }
            }

            return tiles;
        }
        #endregion

        #region LCD Control Register
        public bool LCDEnable
        {
            get
            {
                return (LCDControlRegister & 128) > 0;
            }
        }

        public bool WindowTileMapDisplaySelect
        {
            get
            {
                //true: 9C00 - 9FFFF, false: 9800 - 9BFF
                return (LCDControlRegister & 64) > 0;
            }
        }

        public bool WindowDisplayEnable
        {
            get
            {
                return (LCDControlRegister & 32) > 0;
            }
        }

        public bool BGWindowTileDataSelect
        {
            get
            {
                //true: 8000 - 8FFF, false: 8800 - 97FF
                return (LCDControlRegister & 16) > 0;
            }
        }

        public bool BGTileMapDisplaySelect
        {
            get
            {
                //true: 9C00 - 9FFFF, false: 9800 - 9BFF
                return (LCDControlRegister & 8) > 0;
            }
        }

        public bool SpriteSize
        {
            get
            {
                return (LCDControlRegister & 4) > 0;
            }
        }

        public bool SpriteDisplayEnable
        {
            get
            {
                return (LCDControlRegister & 2) > 0;
            }
        }

        public bool BGDisplay
        {
            get
            {
                return (LCDControlRegister & 1) > 0;
            }
        }

        #endregion

        #region LCD Status Register
        public bool coincidenceInterruptEnable
        {
            get
            {
                return (RAM[0xFF41] & 64) > 0;
            }
            set
            {
                RAM[0xFF41] = (byte)(value ? RAM[0xFF41] | 64 : RAM[0xFF41] & ~64);
            }
        }
        public bool mode2InterruptEnable
        {
            get
            {
                return (RAM[0xFF41] & 32) > 0;
            }
            set
            {
                RAM[0xFF41] = (byte)(value ? RAM[0xFF41] | 32 : RAM[0xFF41] & ~32);
            }
        }
        public bool mode1InterruptEnable
        {
            get
            {
                return (RAM[0xFF41] & 16) > 0;
            }
            set
            {
                RAM[0xFF41] = (byte)(value ? RAM[0xFF41] | 16 : RAM[0xFF41] & ~16);
            }
        }
        public bool mode0InterruptEnable
        {
            get
            {
                return (RAM[0xFF41] & 8) > 0;
            }
            set
            {
                RAM[0xFF41] = (byte)(value ? RAM[0xFF41] | 8 : RAM[0xFF41] & ~8);
            }
        }
        public bool coincidenceFlag
        {
            get
            {
                //TODO: need a way to write this coincidence to ram
                return RAM[0xFF44] == RAM[0xFF45];
            }
        }
        public int modeFlag
        {
            get
            {
                return RAM[0xFF41] & 3;
            }
        }



        #endregion
        public enum AreaSelect
        {
            Background,
            Window
        }

    }
}
