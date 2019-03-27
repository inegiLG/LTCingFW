
using OPCAutomation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW.opc
{
    public class OPCCommand : OPCAckonwledgeInterface,ICloneable
    {
        /// <summary>
        /// 命令名
        /// </summary>
        public String CommandName { get; set; }

        /// <summary>
        /// 标志该命令是同步读、异步读、同步写、异步写、更新订阅
        /// </summary>
        public CommandTypeEnum CommandType { get; set; }

        /// <summary>
        /// 命令完成情况
        /// </summary>
        public CommandStateEnum CommandState { get; set; } = CommandStateEnum.UnDo;

        /// <summary>
        /// OPCItem信息
        /// </summary>
        public List<ItemInfo> ItemGroupList { get; } = new List<ItemInfo>();

        /// <summary>
        /// 有关事务的取消ID
        /// </summary>
        public int CancelID { get; set; }

        /// <summary>
        /// OPCGroup
        /// </summary>
        public OPCGroup OPCGroup { get; set; }

        /// <summary>
        /// 数据变化感知模式间隔时间，默认1000毫秒
        /// </summary>
        public int SensorUpdateRate { get; set; } = 1000;

        /// <summary>
        /// 数据变化感知外部通知
        /// </summary>
        public event DataChangeAcknowledgeHandler OuterNotice_DataChangeSensed;
        /// <summary>
        /// 异步读完成
        /// </summary>
        public event AcknowledgeHandler OuterNotice_AsyncReadComplete;
        /// <summary>
        /// 异步写完成
        /// </summary>
        public event AcknowledgeHandler OuterNotice_AsyncWriteComplete;

        /// <summary>
        /// 根据ClientHandleID查找ItemInfo
        /// </summary>
        /// <param name="clientID"></param>
        /// <returns></returns>
        public ItemInfo GetItemByClientHandleID(int clientID)
        {
            foreach (ItemInfo info in ItemGroupList)
            {
                if (info.ClientHandleID == clientID)
                {
                    return info;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据ServerHandleID查找ItemInfo
        /// </summary>
        /// <param name="serverID"></param>
        /// <returns></returns>
        public ItemInfo GetItemByServerHandleID(int serverID)
        {
            foreach (ItemInfo info in ItemGroupList)
            {
                if (info.ServerHandleID == serverID)
                {
                    return info;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据itemID设置值
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="value"></param>
        public void SetItemValueByItemID(string itemID,object value)
        {
            foreach (ItemInfo info in ItemGroupList)
            {
                if (info.OPCItemID == itemID)
                {
                    info.Value = value;
                    break;
                }
            }

        }

        /// <summary>
        /// 清除值
        /// </summary>
        public void ClearValues()
        {
            foreach (ItemInfo info in ItemGroupList)
            {
                info.ClearValues();
                info.Error = true;
            }
            CommandState = CommandStateEnum.UnDo;
            CancelID = 0;
        }



        /// <summary>
        /// 数据改变回调函数
        /// </summary>
        /// <param name="TransactionID"></param>
        /// <param name="NumItems"></param>
        /// <param name="ClientHandles"></param>
        /// <param name="ItemValues"></param>
        /// <param name="Qualities"></param>
        /// <param name="TimeStamps"></param>
        public void DataChanged(int TransactionID, int NumItems, ref Array ClientHandles, ref Array ItemValues, ref Array Qualities, ref Array TimeStamps)
        {
            try
            {
                lock (this)
                {
                    int[] ChangedClientIds = new int[NumItems];
                    //数据变化写入
                    for (int i = 1; i <= NumItems; i++)
                    {
                        ItemInfo info = GetItemByClientHandleID(Convert.ToInt32(ClientHandles.GetValue(i)));
                        info.Value = ItemValues.GetValue(i);
                        info.Quality = Convert.ToInt32(Qualities.GetValue(i));
                        info.TimeStamp = Convert.ToDateTime(TimeStamps.GetValue(i));
                        ChangedClientIds[i - 1] = Convert.ToInt32(ClientHandles.GetValue(i));
                    }
                    //通知外部
                    if (this.OuterNotice_DataChangeSensed != null) {
                        this.OuterNotice_DataChangeSensed(this, ChangedClientIds);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        /// <summary>
        /// 异步读回调函数
        /// </summary>
        /// <param name="TransactionID"></param>
        /// <param name="NumItems"></param>
        /// <param name="ClientHandles"></param>
        /// <param name="ItemValues"></param>
        /// <param name="Qualities"></param>
        /// <param name="TimeStamps"></param>
        /// <param name="Errors"></param>
        public void OpcGroupAsyncReadComplete(int TransactionID, int NumItems, ref Array ClientHandles, ref Array ItemValues, ref Array Qualities, ref Array TimeStamps, ref Array Errors)
        {
            try
            {
                lock (this)
                {
                    for (int i = 0; i < NumItems; i++)
                    {
                        int clientID = Convert.ToInt32(ClientHandles.GetValue(i + 1));
                        int error = Convert.ToInt32(Errors.GetValue(i + 1));
                        int quality = Convert.ToInt32(Qualities.GetValue(i + 1));
                        ItemInfo info = this.GetItemByClientHandleID(clientID);
                        if (info != null)
                        {
                            info.Value = ItemValues.GetValue(i + 1);
                            info.Quality = Convert.ToInt32(Qualities.GetValue(i + 1));
                            info.TimeStamp = Convert.ToDateTime(TimeStamps.GetValue(i + 1));
                        }
                        if (clientID != 0 && error == 0 && quality != 0)
                        {
                            info.Error = false;
                        }
                    }
                    this.CommandState = CommandStateEnum.Read_Over;
                    //通知外部
                    if (this.OuterNotice_AsyncReadComplete != null) {
                        this.OuterNotice_AsyncReadComplete(this);
                    }
                    
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        /// <summary>
        /// 异步写回调函数
        /// </summary>
        /// <param name="TransactionID"></param>
        /// <param name="NumItems"></param>
        /// <param name="ClientHandles"></param>
        /// <param name="Errors"></param>
        public void OpcGroupAsyncWriteComplete(int TransactionID, int NumItems, ref Array ClientHandles, ref Array Errors)
        {
            try
            {
                lock (this)
                {
                    for (int i = 0; i < NumItems; i++)
                    {
                        int clientID = Convert.ToInt32(ClientHandles.GetValue(i + 1));
                        int error = Convert.ToInt32(Errors.GetValue(i + 1));
                        ItemInfo info = this.GetItemByClientHandleID(clientID);
                        if (clientID != 0 && info != null && error == 0)
                        {
                            info.Error = false;
                        }
                    }
                    this.CommandState = CommandStateEnum.Write_Over;
                    //通知外部
                    if (this.OuterNotice_AsyncWriteComplete != null)
                    {
                        this.OuterNotice_AsyncWriteComplete(this);
                    }
                    
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        public object Clone()
        {
            OPCCommand cmd = new OPCCommand();
            cmd.CancelID = this.CancelID;
            cmd.CommandName = this.CommandName;
            cmd.CommandState = this.CommandState;
            cmd.CommandType = this.CommandType;
            cmd.OPCGroup = this.OPCGroup;
            cmd.SensorUpdateRate = this.SensorUpdateRate;
            foreach (ItemInfo item in this.ItemGroupList)
            {
                cmd.ItemGroupList.Add(item.Clone() as ItemInfo);
            }
            return cmd;
        }
    }
}
