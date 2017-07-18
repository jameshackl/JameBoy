using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JameBoyV1
{
    class Video
    {
        int[,] backgroundBuffer = new int[256,256];
        int[,] backgroundTileMap = new int[32, 32];
        int[,] backgroundTile = new int[8, 8];
        

        //upper left corner of 160x144 pixels to be displayed
        int scrollx;
        int scrolly;

        int[,] smallSprite = new int[8, 8];
        int[,] bigSprite = new int[8, 16];

        byte[,] OAM = new byte[40, 4];

    }
}
