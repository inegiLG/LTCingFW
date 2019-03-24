using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW.opc
{
    public enum PLCTypeEnum 
    {
        DWORD = 0,//4字节32位
        DINT = 1,//4字节32位
        REAL =2,//4字节32位浮点数
        WORD =3,//2字节16位
        INT =4,//2字节16位
        CHAR =5,//1字节8位
        BYTE =6,//1字节8位
        BOOL = 7,//1位
        DATETIME = 8 //8字节
    }
}
