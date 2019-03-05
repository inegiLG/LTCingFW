using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW.opc
{
    public enum CommandStateEnum
    {
        UnDo = 0,
        Reading = 1,
        Writing = 2,
        Read_Over = 3,
        Write_Over = 4
    }
}
