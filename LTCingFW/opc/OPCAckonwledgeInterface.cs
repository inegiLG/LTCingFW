using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW.opc
{
    public interface OPCAckonwledgeInterface
    {
        /// <summary>
        /// 异步读完成
        /// </summary>
        event AcknowledgeHandler OuterNotice_AsyncReadComplete;
        /// <summary>
        /// 异步写完成
        /// </summary>
        event AcknowledgeHandler OuterNotice_AsyncWriteComplete;
        /// <summary>
        /// 数据变化感知外部通知
        /// </summary>
        event DataChangeAcknowledgeHandler OuterNotice_DataChangeSensed;
    }
}
