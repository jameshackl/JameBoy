using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JameBoyV1
{
    public static class GlobalData
    {
        public static byte[] cartridgeROM;

        public static string ROMTitle;
        public static bool colorGBFlag;
        public static int externalRamSize = 0;


    }
    public enum memoryBankController
    {
        MBC1,
        MBC2,
        MBC3,
        MBC5
    }
}
