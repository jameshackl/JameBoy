using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace JameBoyV1
{
    class Program
    {

        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());

            string romPath = "c:\\Testing\\tetris.gb";
            byte[] cartridgeROM = File.ReadAllBytes(romPath);
            string gameTitle = "Title: ";
        
            for(int i = 0x0134; i <= 0x0142;i++)
            {
                gameTitle = gameTitle + Convert.ToChar(cartridgeROM[i]);
            }
            Console.WriteLine(gameTitle);

            switch (cartridgeROM[0x0143])
            {
                case 80:
                    Console.WriteLine("Color Gameboy: Color GameBoy");
                    break;
                default:
                    Console.WriteLine("Color Gameboy: Not Color GameBoy");
                    break;
            }

            Console.Write("Cartridge type: ");
            switch (cartridgeROM[0x147])
            {
                case 0x0: Console.WriteLine("ROM ONLY"); break;
                case 0x1: Console.WriteLine("ROM+MBC1"); break;
                case 0x2: Console.WriteLine("ROM+MBC1+RAM"); break;
                case 0x3: Console.WriteLine("ROM+MBC1+RAM+BATT"); break;
                case 0x5: Console.WriteLine("ROM+MBC2"); break;
                case 0x6: Console.WriteLine("ROM+MBC2+BATTERY"); break;
                case 0x8: Console.WriteLine("ROM+RAM"); break;
                case 0x9: Console.WriteLine("ROM+RAM+BATTERY"); break;
                case 0xB: Console.WriteLine("ROM+MMM01"); break;
                case 0xC: Console.WriteLine("ROM+MMM01+SRAM"); break;
                case 0xD: Console.WriteLine("ROM+MMM01+SRAM+BATT"); break;
                case 0xF: Console.WriteLine("ROM+MBC3+TIMER+BATT"); break;
                case 0x10: Console.WriteLine("ROM+MBC3+TIMER+RAM+BATT"); break;
                case 0x11: Console.WriteLine("ROM+MBC3"); break;
                case 0x12: Console.WriteLine("ROM+MBC3+RAM"); break;
                case 0x13: Console.WriteLine("ROM+MBC3+RAM+BATT"); break;
                case 0x19: Console.WriteLine("ROM+MBC5"); break;
                case 0x1A: Console.WriteLine("ROM+MBC5+RAM"); break;
                case 0x1B: Console.WriteLine("ROM+MBC5+RAM+BATT"); break;
                case 0x1C: Console.WriteLine("ROM+MBC5+RUMBLE"); break;
                case 0x1D: Console.WriteLine("ROM+MBC5+RUMBLE+SRAM"); break;
                case 0x1E: Console.WriteLine("ROM+MBC5+RUMBLE+SRAM+BATT"); break;
                case 0x1F: Console.WriteLine("Pocket Camera"); break;
                case 0xFD: Console.WriteLine("Bandai TAMA5"); break;
                case 0xFE: Console.WriteLine("Hudson HuC-3"); break;
                case 0xFF: Console.WriteLine("Hudson HuC-1"); break;
                default:
                    break;
            }

            Console.Write("ROM Type: ");
            switch (cartridgeROM[0x0148])
            {
                case 0x0: Console.WriteLine("256Kbit = 32KByte = 2 banks"); break;
                case 0x1: Console.WriteLine("512Kbit = 64KByte = 4 banks"); break;
                case 0x2: Console.WriteLine("1Mbit = 128KByte = 8 banks"); break;
                case 0x3: Console.WriteLine("2Mbit = 256KByte = 16 banks"); break;
                case 0x4: Console.WriteLine("4Mbit = 512KByte = 32 banks"); break;
                case 0x5: Console.WriteLine("8Mbit = 1MByte = 64 banks"); break;
                case 0x6: Console.WriteLine("16Mbit = 2MByte = 128 banks"); break;
                case 0x52: Console.WriteLine("9Mbit = 1.1MByte = 72 banks"); break;
                case 0x53: Console.WriteLine("10Mbit = 1.2MByte = 80 banks"); break;
                case 0x54: Console.WriteLine("12Mbit = 1.5MByte = 96 banks"); break;
                default: break;
            }

            Console.Write("RAM Type: ");
            switch (cartridgeROM[0x149])
            {
                case 0: Console.WriteLine("None"); break;
                case 1: Console.WriteLine("16kBit = 2kB = 1 bank"); break;
                case 2: Console.WriteLine("64kBit = 8kB = 1 bank"); break;
                case 3: Console.WriteLine("256kBit = 32kB = 4 banks"); break;
                case 4: Console.WriteLine("1MBit = 128kB = 16 banks"); break;
                default: break;
            }

            Console.WriteLine("");
            Console.WriteLine("-----------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("");
            //cursor row 8

            //Console.SetCursorPosition(10, 10);

            //make the parts of the gameboy
            CPU cpu = CPU.Instance;
            MMU mmu = MMU.Instance;
            Registers reg = Registers.Instance;


            //load rom into ram
            mmu.initalROMLoad(cartridgeROM); //this only works for Tetris. most others are too big

            //inital values
            
            reg.AF.word = 0x01B0; //for GB or Super GB
            reg.BC.word = 0x0013;
            reg.DE.word = 0x00D8;
            reg.HL.word = 0x014D;

            reg.SP = 0xfffe;
            reg.PC = 0x0100;

            

            mmu.RAM[0xFF05] = 0x00;
            mmu.RAM[0xFF06] = 0x00;
            mmu.RAM[0xFF07] = 0x00;
            mmu.RAM[0xFF10] = 0x80;
            mmu.RAM[0xFF11] = 0xBF;
            mmu.RAM[0xFF12] = 0xF3;
            mmu.RAM[0xFF14] = 0xBF;
            mmu.RAM[0xFF16] = 0x3F;
            mmu.RAM[0xFF17] = 0x00;
            mmu.RAM[0xFF19] = 0xBF;
            mmu.RAM[0xFF1A] = 0x7F;
            mmu.RAM[0xFF1B] = 0xFF;
            mmu.RAM[0xFF1C] = 0x9F;
            mmu.RAM[0xFF1E] = 0xBF;
            mmu.RAM[0xFF20] = 0xFF;
            mmu.RAM[0xFF21] = 0x00;
            mmu.RAM[0xFF22] = 0x00;
            mmu.RAM[0xFF23] = 0xBF;
            mmu.RAM[0xFF24] = 0x77;
            mmu.RAM[0xFF25] = 0xF3;
            mmu.RAM[0xFF26] = 0xF1;
            mmu.RAM[0xFF40] = 0x91;
            mmu.RAM[0xFF42] = 0x00;
            mmu.RAM[0xFF43] = 0x00;
            mmu.RAM[0xFF45] = 0x00;
            mmu.RAM[0xFF47] = 0xFC;
            mmu.RAM[0xFF48] = 0xFF;
            mmu.RAM[0xFF49] = 0xFF;
            mmu.RAM[0xFF4A] = 0x00;
            mmu.RAM[0xFF4B] = 0x00;
            mmu.RAM[0xFFFF] = 0x00;


            //Console.WriteLine(reg.PC);
            byte opcode;
            do
            {
                opcode = mmu.readByte();
                //cpu process opcode
                cpu.runOpcode(opcode);
                Console.Write("Opcode: {0,2:X} , A = {1,2:X}, F = {2,2:X}, B = {3,2:X}, C = {4,2:X}, D = {5,2:X}, E = {6,2:X}, H = {7,2:X}, L = {8,2:X}", opcode, reg.A, reg.F, reg.B, reg.C, reg.D, reg.E, reg.H, reg.L);
                Console.WriteLine(cpu.CPUCycles);
                //Console.ReadLine();
                //if(cpu.CPUCycles % 0xffff == 0) { Console.WriteLine(cpu.CPUCycles); }
                //Console.ReadLine();

            } while (true);

            //Console.WriteLine(cartrigeROM.ToString());

            Console.ReadLine();
            

        }

    }
}
