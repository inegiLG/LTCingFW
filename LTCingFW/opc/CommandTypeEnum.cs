using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW.opc
{
    public enum CommandTypeEnum
    {
        None = 0,
        /// <summary>
        /// 同步读
        /// </summary>
        SyncRead = 1,
        /// <summary>
        /// 同步写
        /// </summary>
        SyncWrite = 2,
        /// <summary>
        /// 异步读
        /// </summary>
        AsyncRead = 3,
        /// <summary>
        /// 异步写
        /// </summary>
        AsyncWrite = 4,
        /// <summary>
        /// 异步变化感知
        /// </summary>
        DiffSense = 5
    }
}
