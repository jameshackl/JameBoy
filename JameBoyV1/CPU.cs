using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JameBoyV1
{
    public sealed class CPU
    {
        private static CPU instance = null;
        private static readonly object padlock = new object();
        CPU() { }

        public static CPU Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new CPU();
                    }
                    return instance;
                }
            }
        }



        Registers reg = Registers.Instance;
        MMU memoryManager = MMU.Instance;

        public long CPUCycles = 0;
        bool IME = false; 
        //long MachineCycles = 0;


        private byte workingByte;
        private sbyte workingSByte;
        private ushort workingWord;
        private int workingInt;
        private byte workingByteMask;


        public void runOpcode(byte opcode)
        {
            switch (opcode)
            {
                case 0x00: //0x00 NOP
                    CPUCycles += 4;
                    break;
                case 0x10://TODO: stop
                    CPUCycles += 4;
                    break;
                case 0x20: //JR NZ,r8
                    workingSByte = (sbyte)memoryManager.readByte();
                    if (!reg.zeroFlag())
                    {
                        if (workingSByte < 0)
                        {
                            reg.PC = (ushort)(reg.PC + workingSByte);
                        }
                        else
                        {
                            reg.PC += (byte)workingSByte;
                        }
                        CPUCycles += 4;
                    }


                    CPUCycles += 8;
                    break;
                case 0x30: //JR NC,r8
                    workingSByte = (sbyte)memoryManager.readByte();
                    if (reg.carryFlag())
                    {
                        if (workingSByte < 0)
                        {
                            reg.PC = (ushort)(reg.PC + workingSByte);
                        }
                        else
                        {
                            reg.PC += (byte)workingSByte;
                        }
                        CPUCycles += 4;
                    }

                    CPUCycles += 8;
                    break;


                case 0x01: //LD nn, D16
                    reg.C = memoryManager.readByte();
                    reg.B = memoryManager.readByte();
                    CPUCycles += 12;
                    break;
                case 0x11: //LD nn, D16
                    reg.E = memoryManager.readByte();
                    reg.D = memoryManager.readByte();
                    CPUCycles += 12;
                    break;
                case 0x21: //LD nn, D16
                    reg.L = memoryManager.readByte();
                    reg.H = memoryManager.readByte();
                    CPUCycles += 12;
                    break;
                case 0x31: //LD nn, D16
                    reg.SP = (ushort)(memoryManager.readByte() + memoryManager.readByte() * 256);
                    CPUCycles += 12;
                    break;

                case 0x02: //LD (nn), A
                    memoryManager.writeByte(reg.A, reg.BC.word);
                    CPUCycles += 8;
                    break;
                case 0x12: 
                    memoryManager.writeByte(reg.A, reg.DE.word);
                    CPUCycles += 8;
                    break;
                case 0x22: 
                    memoryManager.writeByte(reg.A, reg.HL.word);
                    reg.HL.word += 1;
                    CPUCycles += 8;
                    break;
                case 0x32: 
                    memoryManager.writeByte(reg.A, reg.HL.word);
                    reg.HL.word -= 1;
                    CPUCycles += 8;
                    break;

                case 0x03: //INC nn
                    reg.BC.word += 1;
                    CPUCycles += 8;
                    break;
                case 0x13: 
                    reg.DE.word += 1;
                    CPUCycles += 8;
                    break;
                case 0x23: 
                    reg.HL.word += 1;
                    CPUCycles += 8;
                    break;
                case 0x33: 
                    reg.SP += 1;
                    CPUCycles += 8;
                    break;

                case 0x04:  incN(reg.B);reg.B++; CPUCycles += 8; break;
                case 0x14:  incN(reg.D);reg.D++; CPUCycles += 8; break;
                case 0x24:  incN(reg.H);reg.H++; CPUCycles += 8; break;
                case 0x34:  incN(memoryManager.readByte(reg.HL.word));memoryManager.incrementByte(reg.HL.word); CPUCycles += 12; break;

                case 0x05:  decN(reg.B);reg.B--; CPUCycles += 8; break;
                case 0x15:  decN(reg.D);reg.D--; CPUCycles += 8; break;
                case 0x25:  decN(reg.H);reg.H--; CPUCycles += 8; break;
                case 0x35:  decN(memoryManager.readByte(reg.HL.word));memoryManager.decrementByte(reg.HL.word); CPUCycles += 12; break;

                case 0x06:
                    reg.B = memoryManager.readByte();
                    CPUCycles += 8;
                    break;
                case 0x16:
                    reg.D = memoryManager.readByte();
                    CPUCycles += 8;
                    break;
                case 0x26:
                    reg.H = memoryManager.readByte();
                    CPUCycles += 8;
                    break;
                case 0x36:
                    memoryManager.writeByte(memoryManager.readByte(), reg.HL.word);
                    CPUCycles += 8;
                    break;

                case 0x07: reg.A = cb_rlc(reg.A); CPUCycles += 4; break;
                case 0x17: reg.A = cb_rl(reg.A); CPUCycles += 4; break;
                case 0x27:
                    workingWord = reg.A;
                    if ((reg.F & 32) > 0)
                    {
                        workingWord += 6;
                        reg.halfCarryFlag(true);

                    }

                    if ((reg.F & 16) > 0)
                    {
                        workingWord += 0x60;
                        reg.carryFlag(true);
                    }

                    reg.zeroFlag(workingWord.Equals(0));
                    reg.A = (byte)(workingWord % 256);
                    CPUCycles += 4;
                    break;
                case 0x37:
                    reg.addSubFlag(false);
                    reg.halfCarryFlag(false);
                    reg.carryFlag(true);
                    CPUCycles += 4;
                    break;

                case 0x08: //LD (nn),SP
                    workingWord = (ushort)(memoryManager.readByte() + memoryManager.readByte() * 256);
                    memoryManager.writeByte((byte)(reg.SP % 256), workingWord);
                    workingWord += 1;
                    memoryManager.writeByte((byte)(reg.SP / 256), workingWord);
                    CPUCycles += 20;
                    break;
                case 0x18: //JR r8
                    workingSByte = (sbyte) memoryManager.readByte();
                    if (workingSByte < 0)
                    {
                        reg.PC -= absoluteOfNegativeSByte(workingSByte);
                    }
                    else
                    {
                        reg.PC += (byte)workingSByte;
                    }
                    CPUCycles += 18;
                    break;
                case 0x28: //JR Z,r8
                    workingSByte = (sbyte)memoryManager.readByte();
                    if ((reg.F & 128) > 0)
                    {
                        if (workingSByte < 0)
                        {
                            reg.PC -= absoluteOfNegativeSByte(workingSByte);
                        }
                        else
                        {
                            reg.PC += (byte)workingSByte;
                        }
                        CPUCycles += 4;
                    }

                    CPUCycles += 8;
                    break;
                case 0x38: //JR C,r8
                    workingSByte = (sbyte)memoryManager.readByte();
                    if((reg.F & 16) > 0)
                    {
                        if (workingSByte < 0)
                        {
                            reg.PC -= absoluteOfNegativeSByte(workingSByte);
                        }
                        else
                        {
                            reg.PC += (byte)workingSByte;
                        }
                        CPUCycles += 4;
                    }

                    CPUCycles += 8;
                    break;

                case 0x09: addHL(reg.BC.word); CPUCycles += 8; break;
                case 0x19: addHL(reg.DE.word); CPUCycles += 8; break;
                case 0x29: addHL(reg.HL.word); CPUCycles += 8; break;
                case 0x39: addHL(reg.SP); CPUCycles += 8; break;


                case 0x0A: //LD A,(nn)
                    reg.A = memoryManager.readByte(reg.BC.word);
                    CPUCycles += 8;
                    break;
                case 0x1A: //LD A,(nn)
                    reg.A = memoryManager.readByte(reg.DE.word);
                    CPUCycles += 8;
                    break;
                case 0x2A: //LD A,(nn)
                    reg.A = memoryManager.readByte(reg.HL.word);
                    reg.HL.word += 1;
                    CPUCycles += 8;
                    break;
                case 0x3A: //LD A,(nn)
                    reg.A = memoryManager.readByte(reg.HL.word);
                    reg.HL.word -= 1;
                    CPUCycles += 8;
                    break;



                case 0x0B: //DEC nn
                    reg.BC.word -= 1;
                    CPUCycles += 8;
                    break;
                case 0x1B: 
                    reg.DE.word -= 1;
                    CPUCycles += 8;
                    break;
                case 0x2B: 
                    reg.HL.word -= 1;
                    CPUCycles += 8;
                    break;
                case 0x3B: 
                    reg.SP -= 1;
                    CPUCycles += 8;
                    break;

                case 0x0C: incN(reg.C); reg.C++; CPUCycles += 8; break;
                case 0x1C: incN(reg.E); reg.E++; CPUCycles += 8; break;
                case 0x2C: incN(reg.L); reg.L++; CPUCycles += 8; break;
                case 0x3C: incN(reg.A); reg.A++; CPUCycles += 8; break; 

                case 0x0D: decN(reg.C); reg.C--; CPUCycles += 8; break;
                case 0x1D: decN(reg.E); reg.E--; CPUCycles += 8; break;
                case 0x2D: decN(reg.L); reg.L--; CPUCycles += 8; break;
                case 0x3D: decN(reg.A); reg.A--; CPUCycles += 8; break;

                case 0x0E: //LD n,D8
                    reg.C = memoryManager.readByte();
                    CPUCycles += 8;
                    break;
                case 0x1E: 
                    reg.E = memoryManager.readByte();
                    CPUCycles += 8;
                    break;
                case 0x2E: 
                    reg.L = memoryManager.readByte();
                    CPUCycles += 8;
                    break;
                case 0x3E: 
                    reg.A = memoryManager.readByte();
                    CPUCycles += 8;
                    break;

                case 0x0F: reg.A = cb_rrc(reg.A); CPUCycles += 4; break;
                case 0x1F: reg.A = cb_rr(reg.A); CPUCycles += 4; break;
                case 0x2F:
                    reg.A = (byte)~(reg.A);
                    reg.addSubFlag(true);
                    reg.halfCarryFlag(true);
                    CPUCycles += 4;
                    break;
                case 0x3f:
                    reg.addSubFlag(false);
                    reg.halfCarryFlag(false);
                    reg.carryFlag(!reg.carryFlag());
                    CPUCycles += 4;
                    break;

                case 0x40: //LD B,n
                    reg.B = reg.B;
                    CPUCycles += 4;
                    break;
				case 0x41:
                    reg.B = reg.C;
                    CPUCycles += 4;
                    break;
				case 0x42: 
                    reg.B = reg.D;
                    CPUCycles += 4;
                    break;
                case 0x43: 
                    reg.B = reg.E;
                    CPUCycles += 4;
                    break;
                case 0x44:
                    reg.B = reg.H;
                    CPUCycles += 4;
                    break;
                case 0x45: 
                    reg.B = reg.L;
                    CPUCycles += 4;
                    break;
                case 0x46:
                    reg.B = memoryManager.readByte(reg.HL.word);
                    CPUCycles += 8;
                    break;
                case 0x47:
                    reg.B = reg.A;
                    CPUCycles += 4;
                    break;
                case 0x48: //LD C,n
                    reg.C = reg.B;
                    CPUCycles += 4;
                    break;
                case 0x49:
                    reg.C = reg.C;
                    CPUCycles += 4;
                    break;
                case 0x4a:
                    reg.C = reg.D;
                    CPUCycles += 4;
                    break;
                case 0x4b:
                    reg.C = reg.E;
                    CPUCycles += 4;
                    break;
                case 0x4c:
                    reg.C = reg.H;
                    CPUCycles += 4;
                    break;
                case 0x4d:
                    reg.C = reg.L;
                    CPUCycles += 4;
                    break;
                case 0x4e:
                    reg.C = memoryManager.readByte(reg.HL.word);
                    CPUCycles += 8;
                    break;
                case 0x4f:
                    reg.C = reg.A;
                    CPUCycles += 4;
                    break;

                case 0x50: //LD D,n
                    reg.D = reg.B;
                    CPUCycles += 4;
                    break;
                case 0x51:
                    reg.D = reg.C;
                    CPUCycles += 4;
                    break;
                case 0x52:
                    reg.D = reg.D;
                    CPUCycles += 4;
                    break;
                case 0x53:
                    reg.D = reg.E;
                    CPUCycles += 4;
                    break;
                case 0x54:
                    reg.D = reg.H;
                    CPUCycles += 4;
                    break;
                case 0x55:
                    reg.D = reg.L;
                    CPUCycles += 4;
                    break;
                case 0x56:
                    reg.D = memoryManager.readByte(reg.HL.word);
                    CPUCycles += 8;
                    break;
                case 0x57:
                    reg.D = reg.A;
                    CPUCycles += 4;
                    break;
                case 0x58: //LD E,n
                    reg.E = reg.B;
                    CPUCycles += 4;
                    break;
                case 0x59:
                    reg.E = reg.C;
                    CPUCycles += 4;
                    break;
                case 0x5a:
                    reg.E = reg.D;
                    CPUCycles += 4;
                    break;
                case 0x5b:
                    reg.E = reg.E;
                    CPUCycles += 4;
                    break;
                case 0x5c:
                    reg.E = reg.H;
                    CPUCycles += 4;
                    break;
                case 0x5d:
                    reg.E = reg.L;
                    CPUCycles += 4;
                    break;
                case 0x5e:
                    reg.E = memoryManager.readByte(reg.HL.word);
                    CPUCycles += 8;
                    break;
                case 0x5f:
                    reg.E = reg.A;
                    CPUCycles += 4;
                    break;

                case 0x60: //LD H,n
                    reg.H = reg.B;
                    CPUCycles += 4;
                    break;
                case 0x61:
                    reg.H = reg.C;
                    CPUCycles += 4;
                    break;
                case 0x62:
                    reg.H = reg.D;
                    CPUCycles += 4;
                    break;
                case 0x63:
                    reg.H = reg.E;
                    CPUCycles += 4;
                    break;
                case 0x64:
                    reg.H = reg.H;
                    CPUCycles += 4;
                    break;
                case 0x65:
                    reg.H = reg.L;
                    CPUCycles += 4;
                    break;
                case 0x66:
                    reg.H = memoryManager.readByte(reg.HL.word);
                    CPUCycles += 8;
                    break;
                case 0x67:
                    reg.H = reg.A;
                    CPUCycles += 4;
                    break;
                case 0x68: //LD L,n
                    reg.L = reg.B;
                    CPUCycles += 4;
                    break;
                case 0x69:
                    reg.L = reg.C;
                    CPUCycles += 4;
                    break;
                case 0x6a:
                    reg.L = reg.D;
                    CPUCycles += 4;
                    break;
                case 0x6b:
                    reg.L = reg.E;
                    CPUCycles += 4;
                    break;
                case 0x6c:
                    reg.L = reg.H;
                    CPUCycles += 4;
                    break;
                case 0x6d:
                    reg.L = reg.L;
                    CPUCycles += 4;
                    break;
                case 0x6e:
                    reg.L = memoryManager.readByte(reg.HL.word);
                    CPUCycles += 8;
                    break;
                case 0x6f:
                    reg.L = reg.A;
                    CPUCycles += 4;
                    break;

                case 0x70: //LD (HL),n
                    memoryManager.writeByte(reg.B,reg.HL.word);
                    CPUCycles += 8;
                    break;
                case 0x71:
                    memoryManager.writeByte(reg.B, reg.HL.word);
                    CPUCycles += 8;
                    break;
                case 0x72:
                    memoryManager.writeByte(reg.B, reg.HL.word);
                    CPUCycles += 8;
                    break;
                case 0x73:
                    memoryManager.writeByte(reg.B, reg.HL.word);
                    CPUCycles += 8;
                    break;
                case 0x74:
                    memoryManager.writeByte(reg.B, reg.HL.word);
                    CPUCycles += 8;
                    break;
                case 0x75:
                    memoryManager.writeByte(reg.B, reg.HL.word);
                    CPUCycles += 8;
                    break;
                
                case 0x77:
                    memoryManager.writeByte(reg.B, reg.HL.word);
                    CPUCycles += 8;
                    break;


                case 0x76://HALT
                    //TODO
                    CPUCycles += 8;
                    break;



                case 0x78: //LD A,n
                    reg.A = reg.B;
                    CPUCycles += 4;
                    break;
                case 0x79:
                    reg.A = reg.C;
                    CPUCycles += 4;
                    break;
                case 0x7a:
                    reg.A = reg.D;
                    CPUCycles += 4;
                    break;
                case 0x7b:
                    reg.A = reg.E;
                    CPUCycles += 4;
                    break;
                case 0x7c:
                    reg.A = reg.H;
                    CPUCycles += 4;
                    break;
                case 0x7d:
                    reg.A = reg.L;
                    CPUCycles += 4;
                    break;
                case 0x7e:
                    reg.A = memoryManager.readByte(reg.HL.word);
                    CPUCycles += 8;
                    break;
                case 0x7f:
                    reg.A = reg.A;
                    CPUCycles += 4;
                    break;

                case 0x80: addA(reg.B); CPUCycles += 4; break;
                case 0x81: addA(reg.C); CPUCycles += 4; break;
                case 0x82: addA(reg.D); CPUCycles += 4; break;
                case 0x83: addA(reg.E); CPUCycles += 4; break;
                case 0x84: addA(reg.H); CPUCycles += 4; break;
                case 0x85: addA(reg.L); CPUCycles += 4; break;
                case 0x86: addA(memoryManager.readByte(reg.HL.word)); CPUCycles += 8; break;
                case 0x87: addA(reg.A); CPUCycles += 4; break;
                case 0x88: adcA(reg.B); CPUCycles += 4; break;
                case 0x89: adcA(reg.C); CPUCycles += 4; break;
                case 0x8A: adcA(reg.D); CPUCycles += 4; break;
                case 0x8B: adcA(reg.E); CPUCycles += 4; break;
                case 0x8C: adcA(reg.H); CPUCycles += 4; break;
                case 0x8D: adcA(reg.L); CPUCycles += 4; break;
                case 0x8E: adcA(memoryManager.readByte(reg.HL.word)); CPUCycles += 8; break;
                case 0x8F: adcA(reg.A); CPUCycles += 4; break;
                case 0x90: subA(reg.B); CPUCycles += 4; break;
                case 0x91: subA(reg.C); CPUCycles += 4; break;
                case 0x92: subA(reg.D); CPUCycles += 4; break;
                case 0x93: subA(reg.E); CPUCycles += 4; break;
                case 0x94: subA(reg.H); CPUCycles += 4; break;
                case 0x95: subA(reg.L); CPUCycles += 4; break;
                case 0x96: subA(memoryManager.readByte(reg.HL.word)); CPUCycles += 8; break;
                case 0x97: subA(reg.A); CPUCycles += 4; break;
                case 0x98: sbcA(reg.B); CPUCycles += 4; break;
                case 0x99: sbcA(reg.C); CPUCycles += 4; break;
                case 0x9A: sbcA(reg.D); CPUCycles += 4; break;
                case 0x9B: sbcA(reg.E); CPUCycles += 4; break;
                case 0x9C: sbcA(reg.H); CPUCycles += 4; break;
                case 0x9D: sbcA(reg.L); CPUCycles += 4; break;
                case 0x9E: sbcA(memoryManager.readByte(reg.HL.word)); CPUCycles += 8; break;
                case 0x9F: andA(reg.A); CPUCycles += 4; break;
                case 0xA0: andA(reg.B); CPUCycles += 4; break;
                case 0xA1: andA(reg.C); CPUCycles += 4; break;
                case 0xA2: andA(reg.D); CPUCycles += 4; break;
                case 0xA3: andA(reg.E); CPUCycles += 4; break;
                case 0xA4: andA(reg.H); CPUCycles += 4; break;
                case 0xA5: andA(reg.L); CPUCycles += 4; break;
                case 0xA6: andA(memoryManager.readByte(reg.HL.word)); CPUCycles += 8; break;
                case 0xA7: andA(reg.A); CPUCycles += 4; break;
                case 0xA8: xorA(reg.B); CPUCycles += 4; break;
                case 0xA9: xorA(reg.C); CPUCycles += 4; break;
                case 0xAA: xorA(reg.D); CPUCycles += 4; break;
                case 0xAB: xorA(reg.E); CPUCycles += 4; break;
                case 0xAC: xorA(reg.H); CPUCycles += 4; break;
                case 0xAD: xorA(reg.L); CPUCycles += 4; break;
                case 0xAE: xorA(memoryManager.readByte(reg.HL.word)); CPUCycles += 8; break;
                case 0xAF: xorA(reg.A); CPUCycles += 4; break;
                case 0xB0: orA(reg.B); CPUCycles += 4; break;
                case 0xB1: orA(reg.C); CPUCycles += 4; break;
                case 0xB2: orA(reg.D); CPUCycles += 4; break;
                case 0xB3: orA(reg.E); CPUCycles += 4; break;
                case 0xB4: orA(reg.H); CPUCycles += 4; break;
                case 0xB5: orA(reg.L); CPUCycles += 4; break;
                case 0xB6: orA(memoryManager.readByte(reg.HL.word)); CPUCycles += 8; break;
                case 0xB7: orA(reg.A); CPUCycles += 4; break;
                case 0xB8: cpA(reg.B); CPUCycles += 4; break;
                case 0xB9: cpA(reg.C); CPUCycles += 4; break;
                case 0xBA: cpA(reg.D); CPUCycles += 4; break;
                case 0xBB: cpA(reg.E); CPUCycles += 4; break;
                case 0xBC: cpA(reg.H); CPUCycles += 4; break;
                case 0xBD: cpA(reg.L); CPUCycles += 4; break;
                case 0xBE: cpA(memoryManager.readByte(reg.HL.word)); CPUCycles += 8; break;
                case 0xBF: cpA(reg.A); CPUCycles += 4; break;



                case 0xCB:
                    workingByte = memoryManager.readByte();
                    switch (workingByte)
                    {
                        //auto generated by excel
                        case 0x00: reg.B = cb_rlc(reg.B); CPUCycles += 8; break;
                        case 0x01: reg.C = cb_rlc(reg.C); CPUCycles += 8; break;
                        case 0x02: reg.D = cb_rlc(reg.D); CPUCycles += 8; break;
                        case 0x03: reg.E = cb_rlc(reg.E); CPUCycles += 8; break;
                        case 0x04: reg.H = cb_rlc(reg.H); CPUCycles += 8; break;
                        case 0x05: reg.L = cb_rlc(reg.L); CPUCycles += 8; break;
                        case 0x06: memoryManager.writeByte(cb_rlc(memoryManager.readByte(reg.HL.word)), reg.HL.word); CPUCycles += 16; break;
                        case 0x07: reg.A = cb_rlc(reg.A); CPUCycles += 8; break;
                        case 0x08: reg.B = cb_rrc(reg.B); CPUCycles += 8; break;
                        case 0x09: reg.C = cb_rrc(reg.C); CPUCycles += 8; break;
                        case 0x0A: reg.D = cb_rrc(reg.D); CPUCycles += 8; break;
                        case 0x0B: reg.E = cb_rrc(reg.E); CPUCycles += 8; break;
                        case 0x0C: reg.H = cb_rrc(reg.H); CPUCycles += 8; break;
                        case 0x0D: reg.L = cb_rrc(reg.L); CPUCycles += 8; break;
                        case 0x0E: memoryManager.writeByte(cb_rrc(memoryManager.readByte(reg.HL.word)), reg.HL.word); CPUCycles += 16; break;
                        case 0x0F: reg.A = cb_rrc(reg.A); CPUCycles += 8; break;
                        case 0x10: reg.B = cb_rr(reg.B); CPUCycles += 8; break;
                        case 0x11: reg.C = cb_rr(reg.C); CPUCycles += 8; break;
                        case 0x12: reg.D = cb_rr(reg.D); CPUCycles += 8; break;
                        case 0x13: reg.E = cb_rr(reg.E); CPUCycles += 8; break;
                        case 0x14: reg.H = cb_rr(reg.H); CPUCycles += 8; break;
                        case 0x15: reg.L = cb_rr(reg.L); CPUCycles += 8; break;
                        case 0x16: memoryManager.writeByte(cb_rr(memoryManager.readByte(reg.HL.word)), reg.HL.word); CPUCycles += 16; break;
                        case 0x17: reg.A = cb_rr(reg.A); CPUCycles += 8; break;
                        case 0x18: reg.B = cb_rl(reg.B); CPUCycles += 8; break;
                        case 0x19: reg.C = cb_rl(reg.C); CPUCycles += 8; break;
                        case 0x1A: reg.D = cb_rl(reg.D); CPUCycles += 8; break;
                        case 0x1B: reg.E = cb_rl(reg.E); CPUCycles += 8; break;
                        case 0x1C: reg.H = cb_rl(reg.H); CPUCycles += 8; break;
                        case 0x1D: reg.L = cb_rl(reg.L); CPUCycles += 8; break;
                        case 0x1E: memoryManager.writeByte(cb_rl(memoryManager.readByte(reg.HL.word)), reg.HL.word); CPUCycles += 16; break;
                        case 0x1F: reg.A = cb_rl(reg.A); CPUCycles += 8; break;
                        case 0x20: reg.B = cb_sla(reg.B); CPUCycles += 8; break;
                        case 0x21: reg.C = cb_sla(reg.C); CPUCycles += 8; break;
                        case 0x22: reg.D = cb_sla(reg.D); CPUCycles += 8; break;
                        case 0x23: reg.E = cb_sla(reg.E); CPUCycles += 8; break;
                        case 0x24: reg.H = cb_sla(reg.H); CPUCycles += 8; break;
                        case 0x25: reg.L = cb_sla(reg.L); CPUCycles += 8; break;
                        case 0x26: memoryManager.writeByte(cb_sla(memoryManager.readByte(reg.HL.word)), reg.HL.word); CPUCycles += 16; break;
                        case 0x27: reg.A = cb_sla(reg.A); CPUCycles += 8; break;
                        case 0x28: reg.B = cb_sra(reg.B); CPUCycles += 8; break;
                        case 0x29: reg.C = cb_sra(reg.C); CPUCycles += 8; break;
                        case 0x2A: reg.D = cb_sra(reg.D); CPUCycles += 8; break;
                        case 0x2B: reg.E = cb_sra(reg.E); CPUCycles += 8; break;
                        case 0x2C: reg.H = cb_sra(reg.H); CPUCycles += 8; break;
                        case 0x2D: reg.L = cb_sra(reg.L); CPUCycles += 8; break;
                        case 0x2E: memoryManager.writeByte(cb_sra(memoryManager.readByte(reg.HL.word)), reg.HL.word); CPUCycles += 16; break;
                        case 0x2F: reg.A = cb_sra(reg.A); CPUCycles += 8; break;
                        case 0x30: reg.B = cb_swap(reg.B); CPUCycles += 8; break;
                        case 0x31: reg.C = cb_swap(reg.C); CPUCycles += 8; break;
                        case 0x32: reg.D = cb_swap(reg.D); CPUCycles += 8; break;
                        case 0x33: reg.E = cb_swap(reg.E); CPUCycles += 8; break;
                        case 0x34: reg.H = cb_swap(reg.H); CPUCycles += 8; break;
                        case 0x35: reg.L = cb_swap(reg.L); CPUCycles += 8; break;
                        case 0x36: memoryManager.writeByte(cb_swap(memoryManager.readByte(reg.HL.word)), reg.HL.word); CPUCycles += 16; break;
                        case 0x37: reg.A = cb_swap(reg.A); CPUCycles += 8; break;
                        case 0x38: reg.B = cb_srl(reg.B); CPUCycles += 8; break;
                        case 0x39: reg.C = cb_srl(reg.C); CPUCycles += 8; break;
                        case 0x3A: reg.D = cb_srl(reg.D); CPUCycles += 8; break;
                        case 0x3B: reg.E = cb_srl(reg.E); CPUCycles += 8; break;
                        case 0x3C: reg.H = cb_srl(reg.H); CPUCycles += 8; break;
                        case 0x3D: reg.L = cb_srl(reg.L); CPUCycles += 8; break;
                        case 0x3E: memoryManager.writeByte(cb_srl(memoryManager.readByte(reg.HL.word)), reg.HL.word); CPUCycles += 16; break;
                        case 0x3F: reg.A = cb_srl(reg.A); CPUCycles += 8; break;
                        case 0x40: cb_bit(reg.B, 0); CPUCycles += 8; break;
                        case 0x41: cb_bit(reg.C, 0); CPUCycles += 8; break;
                        case 0x42: cb_bit(reg.D, 0); CPUCycles += 8; break;
                        case 0x43: cb_bit(reg.E, 0); CPUCycles += 8; break;
                        case 0x44: cb_bit(reg.H, 0); CPUCycles += 8; break;
                        case 0x45: cb_bit(reg.L, 0); CPUCycles += 8; break;
                        case 0x46: cb_bit(memoryManager.readByte(reg.HL.word), 0); CPUCycles += 16; break;
                        case 0x47: cb_bit(reg.A, 0); CPUCycles += 8; break;
                        case 0x48: cb_bit(reg.B, 1); CPUCycles += 8; break;
                        case 0x49: cb_bit(reg.C, 1); CPUCycles += 8; break;
                        case 0x4A: cb_bit(reg.D, 1); CPUCycles += 8; break;
                        case 0x4B: cb_bit(reg.E, 1); CPUCycles += 8; break;
                        case 0x4C: cb_bit(reg.H, 1); CPUCycles += 8; break;
                        case 0x4D: cb_bit(reg.L, 1); CPUCycles += 8; break;
                        case 0x4E: cb_bit(memoryManager.readByte(reg.HL.word), 1); CPUCycles += 16; break;
                        case 0x4F: cb_bit(reg.A, 1); CPUCycles += 8; break;
                        case 0x50: cb_bit(reg.B, 2); CPUCycles += 8; break;
                        case 0x51: cb_bit(reg.C, 2); CPUCycles += 8; break;
                        case 0x52: cb_bit(reg.D, 2); CPUCycles += 8; break;
                        case 0x53: cb_bit(reg.E, 2); CPUCycles += 8; break;
                        case 0x54: cb_bit(reg.H, 2); CPUCycles += 8; break;
                        case 0x55: cb_bit(reg.L, 2); CPUCycles += 8; break;
                        case 0x56: cb_bit(memoryManager.readByte(reg.HL.word), 2); CPUCycles += 16; break;
                        case 0x57: cb_bit(reg.A, 2); CPUCycles += 8; break;
                        case 0x58: cb_bit(reg.B, 3); CPUCycles += 8; break;
                        case 0x59: cb_bit(reg.C, 3); CPUCycles += 8; break;
                        case 0x5A: cb_bit(reg.D, 3); CPUCycles += 8; break;
                        case 0x5B: cb_bit(reg.E, 3); CPUCycles += 8; break;
                        case 0x5C: cb_bit(reg.H, 3); CPUCycles += 8; break;
                        case 0x5D: cb_bit(reg.L, 3); CPUCycles += 8; break;
                        case 0x5E: cb_bit(memoryManager.readByte(reg.HL.word), 3); CPUCycles += 16; break;
                        case 0x5F: cb_bit(reg.A, 3); CPUCycles += 8; break;
                        case 0x60: cb_bit(reg.B, 4); CPUCycles += 8; break;
                        case 0x61: cb_bit(reg.C, 4); CPUCycles += 8; break;
                        case 0x62: cb_bit(reg.D, 4); CPUCycles += 8; break;
                        case 0x63: cb_bit(reg.E, 4); CPUCycles += 8; break;
                        case 0x64: cb_bit(reg.H, 4); CPUCycles += 8; break;
                        case 0x65: cb_bit(reg.L, 4); CPUCycles += 8; break;
                        case 0x66: cb_bit(memoryManager.readByte(reg.HL.word), 4); CPUCycles += 16; break;
                        case 0x67: cb_bit(reg.A, 4); CPUCycles += 8; break;
                        case 0x68: cb_bit(reg.B, 5); CPUCycles += 8; break;
                        case 0x69: cb_bit(reg.C, 5); CPUCycles += 8; break;
                        case 0x6A: cb_bit(reg.D, 5); CPUCycles += 8; break;
                        case 0x6B: cb_bit(reg.E, 5); CPUCycles += 8; break;
                        case 0x6C: cb_bit(reg.H, 5); CPUCycles += 8; break;
                        case 0x6D: cb_bit(reg.L, 5); CPUCycles += 8; break;
                        case 0x6E: cb_bit(memoryManager.readByte(reg.HL.word), 5); CPUCycles += 16; break;
                        case 0x6F: cb_bit(reg.A, 5); CPUCycles += 8; break;
                        case 0x70: cb_bit(reg.B, 6); CPUCycles += 8; break;
                        case 0x71: cb_bit(reg.C, 6); CPUCycles += 8; break;
                        case 0x72: cb_bit(reg.D, 6); CPUCycles += 8; break;
                        case 0x73: cb_bit(reg.E, 6); CPUCycles += 8; break;
                        case 0x74: cb_bit(reg.H, 6); CPUCycles += 8; break;
                        case 0x75: cb_bit(reg.L, 6); CPUCycles += 8; break;
                        case 0x76: cb_bit(memoryManager.readByte(reg.HL.word), 6); CPUCycles += 16; break;
                        case 0x77: cb_bit(reg.A, 6); CPUCycles += 8; break;
                        case 0x78: cb_bit(reg.B, 7); CPUCycles += 8; break;
                        case 0x79: cb_bit(reg.C, 7); CPUCycles += 8; break;
                        case 0x7A: cb_bit(reg.D, 7); CPUCycles += 8; break;
                        case 0x7B: cb_bit(reg.E, 7); CPUCycles += 8; break;
                        case 0x7C: cb_bit(reg.H, 7); CPUCycles += 8; break;
                        case 0x7D: cb_bit(reg.L, 7); CPUCycles += 8; break;
                        case 0x7E: cb_bit(memoryManager.readByte(reg.HL.word), 7); CPUCycles += 16; break;
                        case 0x7F: cb_bit(reg.A, 7); CPUCycles += 8; break;
                        case 0x80: reg.B = (byte)(reg.B & ~(byte)(1 << 0)); CPUCycles += 8; break;
                        case 0x81: reg.C = (byte)(reg.C & ~(byte)(1 << 0)); CPUCycles += 8; break;
                        case 0x82: reg.D = (byte)(reg.D & ~(byte)(1 << 0)); CPUCycles += 8; break;
                        case 0x83: reg.E = (byte)(reg.E & ~(byte)(1 << 0)); CPUCycles += 8; break;
                        case 0x84: reg.H = (byte)(reg.H & ~(byte)(1 << 0)); CPUCycles += 8; break;
                        case 0x85: reg.L = (byte)(reg.L & ~(byte)(1 << 0)); CPUCycles += 8; break;
                        case 0x86: memoryManager.writeByte((byte)(memoryManager.readByte(reg.HL.word) & ~(byte)(1 << 0)), reg.HL.word); CPUCycles += 16; break;
                        case 0x87: reg.A = (byte)(reg.A & ~(byte)(1 << 0)); CPUCycles += 8; break;
                        case 0x88: reg.B = (byte)(reg.B & ~(byte)(1 << 1)); CPUCycles += 8; break;
                        case 0x89: reg.C = (byte)(reg.C & ~(byte)(1 << 1)); CPUCycles += 8; break;
                        case 0x8A: reg.D = (byte)(reg.D & ~(byte)(1 << 1)); CPUCycles += 8; break;
                        case 0x8B: reg.E = (byte)(reg.E & ~(byte)(1 << 1)); CPUCycles += 8; break;
                        case 0x8C: reg.H = (byte)(reg.H & ~(byte)(1 << 1)); CPUCycles += 8; break;
                        case 0x8D: reg.L = (byte)(reg.L & ~(byte)(1 << 1)); CPUCycles += 8; break;
                        case 0x8E: memoryManager.writeByte((byte)(memoryManager.readByte(reg.HL.word) & ~(byte)(1 << 1)), reg.HL.word); CPUCycles += 16; break;
                        case 0x8F: reg.A = (byte)(reg.A & ~(byte)(1 << 1)); CPUCycles += 8; break;
                        case 0x90: reg.B = (byte)(reg.B & ~(byte)(1 << 2)); CPUCycles += 8; break;
                        case 0x91: reg.C = (byte)(reg.C & ~(byte)(1 << 2)); CPUCycles += 8; break;
                        case 0x92: reg.D = (byte)(reg.D & ~(byte)(1 << 2)); CPUCycles += 8; break;
                        case 0x93: reg.E = (byte)(reg.E & ~(byte)(1 << 2)); CPUCycles += 8; break;
                        case 0x94: reg.H = (byte)(reg.H & ~(byte)(1 << 2)); CPUCycles += 8; break;
                        case 0x95: reg.L = (byte)(reg.L & ~(byte)(1 << 2)); CPUCycles += 8; break;
                        case 0x96: memoryManager.writeByte((byte)(memoryManager.readByte(reg.HL.word) & ~(byte)(1 << 2)), reg.HL.word); CPUCycles += 16; break;
                        case 0x97: reg.A = (byte)(reg.A & ~(byte)(1 << 2)); CPUCycles += 8; break;
                        case 0x98: reg.B = (byte)(reg.B & ~(byte)(1 << 3)); CPUCycles += 8; break;
                        case 0x99: reg.C = (byte)(reg.C & ~(byte)(1 << 3)); CPUCycles += 8; break;
                        case 0x9A: reg.D = (byte)(reg.D & ~(byte)(1 << 3)); CPUCycles += 8; break;
                        case 0x9B: reg.E = (byte)(reg.E & ~(byte)(1 << 3)); CPUCycles += 8; break;
                        case 0x9C: reg.H = (byte)(reg.H & ~(byte)(1 << 3)); CPUCycles += 8; break;
                        case 0x9D: reg.L = (byte)(reg.L & ~(byte)(1 << 3)); CPUCycles += 8; break;
                        case 0x9E: memoryManager.writeByte((byte)(memoryManager.readByte(reg.HL.word) & ~(byte)(1 << 3)), reg.HL.word); CPUCycles += 16; break;
                        case 0x9F: reg.A = (byte)(reg.A & ~(byte)(1 << 3)); CPUCycles += 8; break;
                        case 0xA0: reg.B = (byte)(reg.B & ~(byte)(1 << 4)); CPUCycles += 8; break;
                        case 0xA1: reg.C = (byte)(reg.C & ~(byte)(1 << 4)); CPUCycles += 8; break;
                        case 0xA2: reg.D = (byte)(reg.D & ~(byte)(1 << 4)); CPUCycles += 8; break;
                        case 0xA3: reg.E = (byte)(reg.E & ~(byte)(1 << 4)); CPUCycles += 8; break;
                        case 0xA4: reg.H = (byte)(reg.H & ~(byte)(1 << 4)); CPUCycles += 8; break;
                        case 0xA5: reg.L = (byte)(reg.L & ~(byte)(1 << 4)); CPUCycles += 8; break;
                        case 0xA6: memoryManager.writeByte((byte)(memoryManager.readByte(reg.HL.word) & ~(byte)(1 << 4)), reg.HL.word); CPUCycles += 16; break;
                        case 0xA7: reg.A = (byte)(reg.A & ~(byte)(1 << 4)); CPUCycles += 8; break;
                        case 0xA8: reg.B = (byte)(reg.B & ~(byte)(1 << 5)); CPUCycles += 8; break;
                        case 0xA9: reg.C = (byte)(reg.C & ~(byte)(1 << 5)); CPUCycles += 8; break;
                        case 0xAA: reg.D = (byte)(reg.D & ~(byte)(1 << 5)); CPUCycles += 8; break;
                        case 0xAB: reg.E = (byte)(reg.E & ~(byte)(1 << 5)); CPUCycles += 8; break;
                        case 0xAC: reg.H = (byte)(reg.H & ~(byte)(1 << 5)); CPUCycles += 8; break;
                        case 0xAD: reg.L = (byte)(reg.L & ~(byte)(1 << 5)); CPUCycles += 8; break;
                        case 0xAE: memoryManager.writeByte((byte)(memoryManager.readByte(reg.HL.word) & ~(byte)(1 << 5)), reg.HL.word); CPUCycles += 16; break;
                        case 0xAF: reg.A = (byte)(reg.A & ~(byte)(1 << 5)); CPUCycles += 8; break;
                        case 0xB0: reg.B = (byte)(reg.B & ~(byte)(1 << 6)); CPUCycles += 8; break;
                        case 0xB1: reg.C = (byte)(reg.C & ~(byte)(1 << 6)); CPUCycles += 8; break;
                        case 0xB2: reg.D = (byte)(reg.D & ~(byte)(1 << 6)); CPUCycles += 8; break;
                        case 0xB3: reg.E = (byte)(reg.E & ~(byte)(1 << 6)); CPUCycles += 8; break;
                        case 0xB4: reg.H = (byte)(reg.H & ~(byte)(1 << 6)); CPUCycles += 8; break;
                        case 0xB5: reg.L = (byte)(reg.L & ~(byte)(1 << 6)); CPUCycles += 8; break;
                        case 0xB6: memoryManager.writeByte((byte)(memoryManager.readByte(reg.HL.word) & ~(byte)(1 << 6)), reg.HL.word); CPUCycles += 16; break;
                        case 0xB7: reg.A = (byte)(reg.A & ~(byte)(1 << 6)); CPUCycles += 8; break;
                        case 0xB8: reg.B = (byte)(reg.B & ~(byte)(1 << 7)); CPUCycles += 8; break;
                        case 0xB9: reg.C = (byte)(reg.C & ~(byte)(1 << 7)); CPUCycles += 8; break;
                        case 0xBA: reg.D = (byte)(reg.D & ~(byte)(1 << 7)); CPUCycles += 8; break;
                        case 0xBB: reg.E = (byte)(reg.E & ~(byte)(1 << 7)); CPUCycles += 8; break;
                        case 0xBC: reg.H = (byte)(reg.H & ~(byte)(1 << 7)); CPUCycles += 8; break;
                        case 0xBD: reg.L = (byte)(reg.L & ~(byte)(1 << 7)); CPUCycles += 8; break;
                        case 0xBE: memoryManager.writeByte((byte)(memoryManager.readByte(reg.HL.word) & ~(byte)(1 << 7)), reg.HL.word); CPUCycles += 16; break;
                        case 0xBF: reg.A = (byte)(reg.A & ~(byte)(1 << 7)); CPUCycles += 8; break;
                        case 0xC0: reg.B = (byte)(reg.B | (byte)(1 << 0)); CPUCycles += 8; break;
                        case 0xC1: reg.C = (byte)(reg.C | (byte)(1 << 0)); CPUCycles += 8; break;
                        case 0xC2: reg.D = (byte)(reg.D | (byte)(1 << 0)); CPUCycles += 8; break;
                        case 0xC3: reg.E = (byte)(reg.E | (byte)(1 << 0)); CPUCycles += 8; break;
                        case 0xC4: reg.H = (byte)(reg.H | (byte)(1 << 0)); CPUCycles += 8; break;
                        case 0xC5: reg.L = (byte)(reg.L | (byte)(1 << 0)); CPUCycles += 8; break;
                        case 0xC6: memoryManager.writeByte((byte)(memoryManager.readByte(reg.HL.word) | (byte)(1 << 0)), reg.HL.word); CPUCycles += 16; break;
                        case 0xC7: reg.A = (byte)(reg.A | (byte)(1 << 0)); CPUCycles += 8; break;
                        case 0xC8: reg.B = (byte)(reg.B | (byte)(1 << 1)); CPUCycles += 8; break;
                        case 0xC9: reg.C = (byte)(reg.C | (byte)(1 << 1)); CPUCycles += 8; break;
                        case 0xCA: reg.D = (byte)(reg.D | (byte)(1 << 1)); CPUCycles += 8; break;
                        case 0xCB: reg.E = (byte)(reg.E | (byte)(1 << 1)); CPUCycles += 8; break;
                        case 0xCC: reg.H = (byte)(reg.H | (byte)(1 << 1)); CPUCycles += 8; break;
                        case 0xCD: reg.L = (byte)(reg.L | (byte)(1 << 1)); CPUCycles += 8; break;
                        case 0xCE: memoryManager.writeByte((byte)(memoryManager.readByte(reg.HL.word) | (byte)(1 << 1)), reg.HL.word); CPUCycles += 16; break;
                        case 0xCF: reg.A = (byte)(reg.A | (byte)(1 << 1)); CPUCycles += 8; break;
                        case 0xD0: reg.B = (byte)(reg.B | (byte)(1 << 2)); CPUCycles += 8; break;
                        case 0xD1: reg.C = (byte)(reg.C | (byte)(1 << 2)); CPUCycles += 8; break;
                        case 0xD2: reg.D = (byte)(reg.D | (byte)(1 << 2)); CPUCycles += 8; break;
                        case 0xD3: reg.E = (byte)(reg.E | (byte)(1 << 2)); CPUCycles += 8; break;
                        case 0xD4: reg.H = (byte)(reg.H | (byte)(1 << 2)); CPUCycles += 8; break;
                        case 0xD5: reg.L = (byte)(reg.L | (byte)(1 << 2)); CPUCycles += 8; break;
                        case 0xD6: memoryManager.writeByte((byte)(memoryManager.readByte(reg.HL.word) | (byte)(1 << 2)), reg.HL.word); CPUCycles += 16; break;
                        case 0xD7: reg.A = (byte)(reg.A | (byte)(1 << 2)); CPUCycles += 8; break;
                        case 0xD8: reg.B = (byte)(reg.B | (byte)(1 << 3)); CPUCycles += 8; break;
                        case 0xD9: reg.C = (byte)(reg.C | (byte)(1 << 3)); CPUCycles += 8; break;
                        case 0xDA: reg.D = (byte)(reg.D | (byte)(1 << 3)); CPUCycles += 8; break;
                        case 0xDB: reg.E = (byte)(reg.E | (byte)(1 << 3)); CPUCycles += 8; break;
                        case 0xDC: reg.H = (byte)(reg.H | (byte)(1 << 3)); CPUCycles += 8; break;
                        case 0xDD: reg.L = (byte)(reg.L | (byte)(1 << 3)); CPUCycles += 8; break;
                        case 0xDE: memoryManager.writeByte((byte)(memoryManager.readByte(reg.HL.word) | (byte)(1 << 3)), reg.HL.word); CPUCycles += 16; break;
                        case 0xDF: reg.A = (byte)(reg.A | (byte)(1 << 3)); CPUCycles += 8; break;
                        case 0xE0: reg.B = (byte)(reg.B | (byte)(1 << 4)); CPUCycles += 8; break;
                        case 0xE1: reg.C = (byte)(reg.C | (byte)(1 << 4)); CPUCycles += 8; break;
                        case 0xE2: reg.D = (byte)(reg.D | (byte)(1 << 4)); CPUCycles += 8; break;
                        case 0xE3: reg.E = (byte)(reg.E | (byte)(1 << 4)); CPUCycles += 8; break;
                        case 0xE4: reg.H = (byte)(reg.H | (byte)(1 << 4)); CPUCycles += 8; break;
                        case 0xE5: reg.L = (byte)(reg.L | (byte)(1 << 4)); CPUCycles += 8; break;
                        case 0xE6: memoryManager.writeByte((byte)(memoryManager.readByte(reg.HL.word) | (byte)(1 << 4)), reg.HL.word); CPUCycles += 16; break;
                        case 0xE7: reg.A = (byte)(reg.A | (byte)(1 << 4)); CPUCycles += 8; break;
                        case 0xE8: reg.B = (byte)(reg.B | (byte)(1 << 5)); CPUCycles += 8; break;
                        case 0xE9: reg.C = (byte)(reg.C | (byte)(1 << 5)); CPUCycles += 8; break;
                        case 0xEA: reg.D = (byte)(reg.D | (byte)(1 << 5)); CPUCycles += 8; break;
                        case 0xEB: reg.E = (byte)(reg.E | (byte)(1 << 5)); CPUCycles += 8; break;
                        case 0xEC: reg.H = (byte)(reg.H | (byte)(1 << 5)); CPUCycles += 8; break;
                        case 0xED: reg.L = (byte)(reg.L | (byte)(1 << 5)); CPUCycles += 8; break;
                        case 0xEE: memoryManager.writeByte((byte)(memoryManager.readByte(reg.HL.word) | (byte)(1 << 5)), reg.HL.word); CPUCycles += 16; break;
                        case 0xEF: reg.A = (byte)(reg.A | (byte)(1 << 5)); CPUCycles += 8; break;
                        case 0xF0: reg.B = (byte)(reg.B | (byte)(1 << 6)); CPUCycles += 8; break;
                        case 0xF1: reg.C = (byte)(reg.C | (byte)(1 << 6)); CPUCycles += 8; break;
                        case 0xF2: reg.D = (byte)(reg.D | (byte)(1 << 6)); CPUCycles += 8; break;
                        case 0xF3: reg.E = (byte)(reg.E | (byte)(1 << 6)); CPUCycles += 8; break;
                        case 0xF4: reg.H = (byte)(reg.H | (byte)(1 << 6)); CPUCycles += 8; break;
                        case 0xF5: reg.L = (byte)(reg.L | (byte)(1 << 6)); CPUCycles += 8; break;
                        case 0xF6: memoryManager.writeByte((byte)(memoryManager.readByte(reg.HL.word) | (byte)(1 << 6)), reg.HL.word); CPUCycles += 16; break;
                        case 0xF7: reg.A = (byte)(reg.A | (byte)(1 << 6)); CPUCycles += 8; break;
                        case 0xF8: reg.B = (byte)(reg.B | (byte)(1 << 7)); CPUCycles += 8; break;
                        case 0xF9: reg.C = (byte)(reg.C | (byte)(1 << 7)); CPUCycles += 8; break;
                        case 0xFA: reg.D = (byte)(reg.D | (byte)(1 << 7)); CPUCycles += 8; break;
                        case 0xFB: reg.E = (byte)(reg.E | (byte)(1 << 7)); CPUCycles += 8; break;
                        case 0xFC: reg.H = (byte)(reg.H | (byte)(1 << 7)); CPUCycles += 8; break;
                        case 0xFD: reg.L = (byte)(reg.L | (byte)(1 << 7)); CPUCycles += 8; break;
                        case 0xFE: memoryManager.writeByte((byte)(memoryManager.readByte(reg.HL.word) | (byte)(1 << 7)), reg.HL.word); CPUCycles += 16; break;
                        case 0xFF: reg.A = (byte)(reg.A | (byte)(1 << 7)); CPUCycles += 8; break;



                        default: break;
                    }
                    break;

                case 0xC0: //RET NZ
                    if (!reg.zeroFlag())
                    {
                        ret();
                        CPUCycles += 12;
                    }

                    CPUCycles += 8;
                    break;
                case 0xD0: //RET NC
                    if (!reg.carryFlag())
                    {
                        ret();
                        CPUCycles += 12;
                    }

                    CPUCycles += 8;
                    break;
                case 0xE0:
                    memoryManager.writeByte(reg.A, (ushort)(0xFF00 + memoryManager.readByte()));
                    CPUCycles += 12;
                    break;
                case 0xF0:
                    reg.A = memoryManager.readByte((ushort)(0xFF00 + memoryManager.readByte()));
                    CPUCycles += 12;
                    break;

                case 0xC1: reg.BC.word = memoryManager.readWordFromStack(); CPUCycles += 12; break;
                case 0xD1: reg.DE.word = memoryManager.readWordFromStack(); CPUCycles += 12; break;
                case 0xE1: reg.HL.word = memoryManager.readWordFromStack(); CPUCycles += 12; break;
                case 0xF1: reg.AF.word = memoryManager.readWordFromStack(); CPUCycles += 12; break;

                case 0xC2: //JP NZ,r8
                    workingWord = (ushort)(memoryManager.readWord());
                    if ((reg.F & 128) == 0)
                    {
                        reg.PC = workingWord;
                        CPUCycles += 4;
                    }

                    CPUCycles += 12;
                    break;
                case 0xD2: //JP NC,r8
                    workingWord = (ushort)(memoryManager.readWord());
                    if ((reg.F & 16) == 0)
                    {
                        reg.PC = workingWord;
                        CPUCycles += 4;
                    }

                    CPUCycles += 12;
                    break;
                case 0xE2:
                    memoryManager.writeByte(reg.A, (ushort)(0xFF00 + reg.C));
                    CPUCycles += 8;
                    break;
                case 0xF2:
                    reg.A = memoryManager.readByte((ushort)(0xFF00 + reg.C));
                    CPUCycles += 8;
                    break;

                case 0xC3: //JP nn, jump to nn
                    workingWord = memoryManager.readWord();
                    reg.PC = workingWord;
                    CPUCycles += 4;
                    break;
                case 0xF3: IME = false; CPUCycles += 16; break;

                case 0xC4: //Call NZ, a16
                    workingWord = memoryManager.readWord();
                    if (!reg.zeroFlag())
                    {
                        call(workingWord);
                        CPUCycles += 12;
                    }

                    CPUCycles += 12;
                    break;
                case 0xD4: //Call NC, a16
                    workingWord = memoryManager.readWord();
                    if (!reg.carryFlag())
                    {
                        call(workingWord);
                        CPUCycles += 12;
                    }

                    CPUCycles += 12;
                    break;

                case 0xC5: memoryManager.writeWordToStack(reg.BC.word); CPUCycles += 16; break;
                case 0xD5: memoryManager.writeWordToStack(reg.DE.word); CPUCycles += 16; break;
                case 0xE5: memoryManager.writeWordToStack(reg.HL.word); CPUCycles += 16; break;
                case 0xF5: memoryManager.writeWordToStack(reg.AF.word); CPUCycles += 16; break;

                case 0xC6: addA(memoryManager.readByte()); CPUCycles += 8; break;
                case 0xD6: subA(memoryManager.readByte()); CPUCycles += 8; break;
                case 0xE6: andA(memoryManager.readByte()); CPUCycles += 8; break;
                case 0xF6: orA(memoryManager.readByte()); CPUCycles += 8; break;

                case 0xC7: call(0x0000); CPUCycles += 16; break;
                case 0xD7: call(0x0010); CPUCycles += 16; break;
                case 0xE7: call(0x0020); CPUCycles += 16; break;
                case 0xF7: call(0x0030); CPUCycles += 16; break;

                case 0xC8: //RET Z
                    if (reg.zeroFlag())
                    {
                        ret();
                        CPUCycles += 12;
                    }

                    CPUCycles += 8;
                    break;
                case 0xD8: //RET C
                    if (reg.carryFlag())
                    {
                        ret();
                        CPUCycles += 12;
                    }

                    CPUCycles += 8;
                    break;
                case 0xE8: addSP((sbyte)memoryManager.readByte()); CPUCycles += 16; break;
                case 0xF8:
                    workingSByte = (sbyte)(memoryManager.readByte());
                    workingWord = reg.HL.word = (ushort)(reg.SP + workingSByte);
                    CPUCycles += 12;

                    reg.zeroFlag(false);
                    reg.addSubFlag(false);
                    reg.halfCarryFlag(checkHalfByteCarryAdd(reg.SP, workingSByte));
                    reg.carryFlag(workingWord > 255);

                    break;
                case 0xC9: ret(); CPUCycles += 16; break;
                case 0xD9: ret(); IME = true; CPUCycles += 16; break;
                case 0xE9: reg.PC = reg.HL.word; CPUCycles += 4; break;
                case 0xF9: reg.SP = reg.HL.word; CPUCycles += 8; break;

                case 0xCA: //JP Z,r8
                    workingWord = (ushort)(memoryManager.readWord());
                    if (reg.zeroFlag())
                    {
                        reg.PC = workingWord;
                        CPUCycles += 4;
                    }

                    CPUCycles += 12;
                    break;
                case 0xDA: //JP C,r8
                    workingWord = (ushort)(memoryManager.readWord());
                    if (reg.carryFlag())
                    {
                        reg.PC = workingWord;
                        CPUCycles += 4;
                    }

                    CPUCycles += 12;
                    break;
                case 0xEA:memoryManager.writeByte(reg.A, memoryManager.readWord()); CPUCycles += 16; break;
                case 0xFA:reg.A = memoryManager.readByte(memoryManager.readWord()); CPUCycles += 16; break;

                case 0xFB: IME = true; CPUCycles += 16; break;

                case 0xCC: //Call Z, a16
                    workingWord = memoryManager.readWord();
                    if (reg.zeroFlag())
                    {
                        call(workingWord);
                        CPUCycles += 12;
                    }

                    CPUCycles += 12;
                    break;
                case 0xDC: //Call C, a16
                    workingWord = memoryManager.readWord();
                    if (reg.carryFlag())
                    {
                        call(workingWord);
                        CPUCycles += 12;
                    }

                    CPUCycles += 12;
                    break;

                case 0xCD: call(memoryManager.readWord()); CPUCycles += 24; break;

                case 0xCE: adcA(memoryManager.readByte()); CPUCycles += 8; break;
                case 0xDE: sbcA(memoryManager.readByte()); CPUCycles += 8; break;
                case 0xEE: xorA(memoryManager.readByte()); CPUCycles += 8; break;
                case 0xFE: cpA(memoryManager.readByte()); CPUCycles += 8; break;

                case 0xCF: call(0x0008); CPUCycles += 16; break;
                case 0xDF: call(0x0018); CPUCycles += 16; break;
                case 0xEF: call(0x0028); CPUCycles += 16; break;
                case 0xFF: call(0x0038); CPUCycles += 16; break;
                default:
                    break;
            }
        }

        private byte absoluteOfNegativeSByte(sbyte x)
        {
                return (byte) Math.Abs(x);
        }

        private void addA(byte b)
        {
            workingWord = (ushort)(reg.A + b);

            reg.zeroFlag(workingWord.Equals(0));
            reg.addSubFlag(false);
            reg.halfCarryFlag(checkHalfByteCarryAdd(reg.A, b));
            reg.carryFlag(workingWord > 255); 
            
            reg.A = (byte)(workingWord % 256);
        }
        private void adcA(byte b)
        {
            workingWord = (ushort)(reg.A + b);
            if (reg.carryFlag()) { workingWord++; }

            reg.zeroFlag(workingWord.Equals(0));
            reg.addSubFlag(false);
            reg.halfCarryFlag(checkHalfByteCarryAdd(reg.A, b));
            reg.carryFlag(workingWord > 255);

            reg.A = (byte)(workingWord % 256);
        }
        private void subA(byte b)
        {
            workingByte = (byte)(reg.A - b); 

            reg.zeroFlag(workingByte.Equals(0));
            reg.addSubFlag(true);
            reg.halfCarryFlag(checkHalfByteCarrySub(reg.A, b));
            reg.carryFlag(reg.A - b < 0);

            reg.A = workingByte;
        }
        private void sbcA(byte b)
        {
            workingByte = (byte)(reg.A - b);
            if (reg.carryFlag()) { workingByte++; }

            reg.zeroFlag(workingByte.Equals(0));
            reg.addSubFlag(true);
            reg.halfCarryFlag(checkHalfByteCarrySub(reg.A, b));
            reg.carryFlag(reg.A - b < 0);

            reg.A = workingByte;
        }
        private void andA(byte b)
        {
            workingByte = reg.A &= b;

            reg.zeroFlag(workingByte.Equals(0));
            reg.addSubFlag(false);
            reg.halfCarryFlag(true);
            reg.carryFlag(false);


        }
        private void orA(byte b)
        {
            workingByte = reg.A |= b;

            reg.zeroFlag(workingByte.Equals(0));
            reg.addSubFlag(false);
            reg.halfCarryFlag(false);
            reg.carryFlag(false);

        }
        private void cpA(byte b)
        {
            workingByte = (byte)(reg.A - b);

            reg.zeroFlag(workingByte.Equals(0));
            reg.addSubFlag(true);
            reg.halfCarryFlag(!checkHalfByteCarrySub(reg.A, b));
            reg.carryFlag(reg.A - b < 0);
        }
        private void xorA(byte b)
        {
            workingByte = reg.A ^= b;

            reg.zeroFlag(workingByte.Equals(0));
            reg.addSubFlag(false);
            reg.halfCarryFlag(false);
            reg.carryFlag(false);
        }

        private void incN(byte b)
        {
            workingWord = (ushort)(b+1);

            reg.zeroFlag(workingWord.Equals(0));
            reg.addSubFlag(false);
            reg.halfCarryFlag(checkHalfByteCarryAdd(b, (byte)1));
            //reg.carryFlag();
        }
        private void decN(byte b)
        {
            workingWord = (ushort)(b - 1);

            reg.zeroFlag(workingWord.Equals(0));
            reg.addSubFlag(true);
            reg.halfCarryFlag(checkHalfByteCarrySub(b, 1));
            //reg.carryFlag();
        }

        private void addHL(ushort b)
        {
            workingInt = (reg.HL.word + b);

            //reg.zeroFlag();
            reg.addSubFlag(false);
            reg.halfCarryFlag(checkHalfByteCarryAdd(reg.HL.word, b));
            reg.carryFlag(workingInt > 0xffff);

            reg.HL.word = (ushort)(workingInt % 0xffff);
        }
        private void addSP(sbyte b)
        {
            workingInt = (reg.SP + b);

            reg.zeroFlag(false);
            reg.addSubFlag(false);
            reg.halfCarryFlag(checkHalfByteCarryAdd(reg.SP, b));
            reg.carryFlag(workingInt > 0xffff);

            reg.HL.word = (ushort)(workingInt % 0xffff);
        }

        private bool checkHalfByteCarryAdd(byte x, byte y)
        {
            return (y & 0xf + x & 0xf) > 0xf;
        }
        private bool checkHalfByteCarryAdd(ushort x, ushort y)
        {
            return (y & 0xfff + x & 0xfff) > 0xfff;
        }
        private bool checkHalfByteCarryAdd(ushort x, sbyte y)
        {
            return (y & 0xfff + x & 0xfff) > 0xfff;
        }

        private bool checkHalfByteCarrySub(byte x, byte y)
        {
            return (y & 0xf) - (x & 0xf) < 0;
        }


        private void pop() { }
        private void push()
        {

        }
        private void call(ushort address)
        {
            memoryManager.writeWordToStack(reg.PC);
            reg.PC = memoryManager.readWord();
        }

        private void ret()
        {
            reg.PC = memoryManager.readWordFromStack();
        }


        private byte cb_rlc(byte b)
        {
            workingWord = (byte)(b << 1);

            if ((byte)(b & 128) > 0) { workingWord++; reg.carryFlag(true); }else { reg.carryFlag(false); }
            reg.zeroFlag(workingWord.Equals(0));
            reg.addSubFlag(false);
            reg.halfCarryFlag(false);

            return (byte)(workingWord % 256);
        }
        private byte cb_rrc(byte b)
        {
            workingWord = (byte)(b >> 1);
            
            if ((byte)(b & 1) > 0) { reg.carryFlag(true); } else { reg.carryFlag(false); }
            reg.zeroFlag(workingWord.Equals(0));
            reg.addSubFlag(false);
            reg.halfCarryFlag(false);

            return (byte)(workingWord % 256);
        }
        private byte cb_rl(byte b)
        {
            workingWord = (byte)(b << 1);
            if (reg.carryFlag()) { workingWord++; }
            if ((byte)(b & 128) > 0) {reg.carryFlag(true); } else { reg.carryFlag(false); }
            reg.zeroFlag(workingWord.Equals(0));
            reg.addSubFlag(false);
            reg.halfCarryFlag(false);

            return (byte)(workingWord % 256);
        }
        private byte cb_rr(byte b)
        {
            workingByte = (byte)(b >> 1);
            if (reg.carryFlag()) { workingByte += 128; }
            if ((byte)(b & 1) > 0) { reg.carryFlag(true); } else { reg.carryFlag(false); }
            reg.zeroFlag(workingByte.Equals(0));
            reg.addSubFlag(false);
            reg.halfCarryFlag(false);

            return workingByte;
        }
        private byte cb_sla(byte b)
        {
            workingWord = (byte)(b << 1);

            reg.zeroFlag(workingWord.Equals(0));
            reg.addSubFlag(false);
            reg.halfCarryFlag(false);
            reg.carryFlag(workingWord > 0xff);

            return (byte)(workingWord % 256);
        }
        private byte cb_sra(byte b)
        {
            workingWord = (byte)(b >> 1);

            workingWord &= (byte)(b & 128);

            reg.zeroFlag(workingWord.Equals(0));
            reg.addSubFlag(false);
            reg.halfCarryFlag(false);
            reg.carryFlag((byte)(b & 1) > 0);

            return (byte)(workingWord % 256);
        }
        private byte cb_swap(byte b)
        {
            return (byte)((b << 4) | (b >> 4));
        }
        private byte cb_srl(byte b)
        {
            workingWord = (byte)(b >> 1);
    
            reg.zeroFlag(workingWord.Equals(0));
            reg.addSubFlag(false);
            reg.halfCarryFlag(false);
            reg.carryFlag((byte)(b & 1) > 0);

            return (byte)(workingWord % 256);
        }
        private void cb_bit(byte b, int bitNumber)
        {
            workingByteMask = (byte)(1 << bitNumber);
            if((workingByteMask & b) == 0) { reg.zeroFlag(true); }
            reg.addSubFlag(false);
            reg.halfCarryFlag(true);

        }
        private void cb_res(byte b, int bitNumber)
        {

        }
        private void cb_set(byte b, int bitNumber)
        {     

        }


        private void ld(ref byte regIn, ref ushort address)
        {

        }

        private void ld(ref ushort address, ref byte regOut)
        {

        }

        private void increment(ref ushort reg16)
        {

        }

        private void increment(ref byte reg)
        {

        }

        private void incrementAt(ushort address)
        {

        }

        private void decrement(ref ushort reg16)
        {

        }

        private void decrement(ref byte reg)
        {

        }

        private void decrementAt(ushort address)
        {

        }
    }
}
