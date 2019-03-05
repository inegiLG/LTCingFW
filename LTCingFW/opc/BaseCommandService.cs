
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW.opc
{
    public abstract class BaseCommandService
    {      

        /// <summary>
        /// 同步读接口方法
        /// </summary>
        /// <param name="control"></param>
        public virtual void SyncRead(OPCControl control,String CommandName)
        {
            OPCCommand command = OPCControl.GetNewXmlCommand(CommandName);
            control.Excute(command);
        }
        /// <summary>
        /// 同步写
        /// </summary>
        /// <param name="control"></param>
        public virtual void SyncWrite(OPCControl control, String CommandName)
        {
            OPCCommand command = OPCControl.GetNewXmlCommand(CommandName);
            control.Excute(command);
        }
        /// <summary>
        /// 异步读
        /// </summary>
        /// <param name="control"></param>
        public virtual void AsyncRead(OPCControl control, String CommandName)
        {
            OPCCommand command = OPCControl.GetNewXmlCommand(CommandName);
            command.OuterNotice_AsyncReadComplete += this.NoticeAsynReadCompleted;
            control.Excute(command);
        }
        /// <summary>
        /// 异步写
        /// </summary>
        /// <param name="control"></param>
        public virtual void AsyncWrite(OPCControl control, String CommandName)
        {
            OPCCommand command = OPCControl.GetNewXmlCommand(CommandName);
            command.OuterNotice_AsyncWriteComplete += this.NoticeAsynWriteCompleted;
            control.Excute(command);
        }
        /// <summary>
        /// 注册，数值改变自动响应
        /// </summary>
        /// <param name="control"></param>
        public virtual void Subscribe(OPCControl control, String CommandName)
        {
            OPCCommand command = OPCControl.GetNewXmlCommand(CommandName);
            command.OuterNotice_DataChangeSensed += this.NoticeDataChangedCallBack;
            control.Excute(command);
        }


        /// <summary>
        /// 异步读回调通知,
        /// </summary>
        /// <param name="command">命令</param>
        public abstract void NoticeAsynReadCompleted(OPCCommand command);

        /// <summary>
        /// 异步写回调通知,是否错误可以通过Item的Error属性查看
        /// </summary>
        /// <param name="command">命令</param>
        public abstract void NoticeAsynWriteCompleted(OPCCommand command);

        /// <summary>
        /// 数据有改变回调通知
        /// </summary>
        /// <param name="command">命令</param>
        /// <param name="ItemNums">改变数据的数量</param>
        /// <param name="ClientHandles">Item的ClientId，若为0说明，该项有错误,可通过该项找到OPCComand中Item的值</param>
        public abstract void NoticeDataChangedCallBack(OPCCommand command, Array ClientHandles);        

    }
}
