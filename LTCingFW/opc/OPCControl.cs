
using OPCAutomation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LTCingFW.opc
{

    /// <summary>
    /// OPC连接
    /// </summary>
    public class OPCControl
    {

        #region 内部属性


        private OPCServer _server;
        /// <summary>
        /// OPCServer实例
        /// </summary>
        public OPCServer OPCServer
        {
            get
            {
                return _server;
            }
        }
        /// <summary>
        /// 已经注册进来的OPCCommand命令集合。
        /// 这标志着在OPCServer中有其对应组。
        /// </summary>
        public List<OPCCommand> UsingCommandList { get; } = new List<OPCCommand>();
        #endregion

        #region 内部方法

        //私有化构造函数
        private OPCControl()
        {
            _server = new OPCServer();
            _server.ServerShutDown += ServerShutDownEventHandler;
        }
        private OPCControl(OPCServer server)
        {
            _server = server;
            _server.ServerShutDown += ServerShutDownEventHandler;
        }

        /// <summary>
        /// 获取在用的OPC命令
        /// </summary>
        /// <param name="cmdName"></param>
        /// <returns></returns>
        public OPCCommand GetUsingOpcCommand(string cmdName)
        {

            foreach (OPCCommand cmd in UsingCommandList)
            {
                if (cmd.CommandName == cmdName)
                {
                    return cmd;
                }
            }
            return null;
        }


        /// <summary>
        /// 执行OPCServer连接
        /// </summary>
        /// <param name="ServerName">OPC服务器名</param>
        /// <param name="IP">OPC服务器IP</param>
        public void Connect(String ServerName, String IP)
        {
            lock (_server)
            {
                try
                {
                    if (OPCServer.ServerState != (int)OPCServerState.OPCRunning)
                    {
                        //断开连接
                        DisConnect();
                        //连接OPC服务器
                        OPCServer.Connect(ServerName, IP);
                        //检查状态
                        if (OPCServer.ServerState != (int)OPCServerState.OPCRunning)
                        {
                            throw new OPCServerRWException("OPCServer启动失败，请检查连接信息！");
                        }
                    }
                    else
                    {
                        throw new OPCServerRWException("OPCServer已连接，请先关闭连接！");
                    }

                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 外部关闭OPCServer事件
        /// </summary>
        /// <param name="Reason"></param>
        private void ServerShutDownEventHandler(string Reason)
        {
            OPCServer.OPCGroups.RemoveAll();
            DisConnect();
            throw new OPCServerRWException("OPC服务器外部关闭，" + Reason);
        }

        /// <summary>
        /// 断开OPCServer连接
        /// </summary>
        public void DisConnect()
        {
            lock (_server)
            {
                try
                {
                    OPCServer.Disconnect();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 添加命令
        /// </summary>
        public void AddOPCCommand(OPCCommand command)
        {
            OPCGroups OPCGroups = _server.OPCGroups;
            OPCGroup OPCGroup = null;
            int i = 0;
            
            //检查
            if (OPCServer.ServerState != (int)OPCServerState.OPCRunning)
            {
                throw new OPCServerRWException("连接不可用，请检查连接！");
            }
            if (command.ItemGroupList.Count == 0)
            {
                throw new OPCServerRWException("未添加Item信息！");
            }
            if (command.CommandName != null && command.CommandName.Trim() != "")
            {
                OPCCommand c = GetUsingOpcCommand(command.CommandName);
                if (c != null)
                {
                    throw new OPCServerRWException("该命令名的命令已经存在，请使用其他命令名或不使用命令名！");
                    //RemoveOPCCommand(c);
                }
            }
            else
            {
                command.CommandName = StaticUtils.GetRandomGroupName();
            }
            //添加
            try
            {
                OPCGroup = OPCGroups.Add(command.CommandName);
                for (; i < command.ItemGroupList.Count; i++)
                {
                    int clientID = StaticUtils.ClientHandleID++;
                    OPCItem opc_item = OPCGroup.OPCItems.AddItem(command.ItemGroupList[i].OPCItemID, clientID);
                    command.ItemGroupList[i].OPCItem = opc_item;
                    command.ItemGroupList[i].ClientHandleID = clientID;
                    command.ItemGroupList[i].ServerHandleID = opc_item.ServerHandle;
                }
                command.OPCGroup = OPCGroup;
                UsingCommandList.Add(command);
            }
            catch (Exception e)
            {
                if (OPCGroup != null)
                {
                    OPCGroups.Remove(command.CommandName);
                }
                throw new OPCServerRWException("添加OPCItem出错，出错变量名["+ command.ItemGroupList[i].OPCItemID+"]", e);
            }
        }

        /// <summary>
        /// 清除命令,对于临时仅用一次的命令，建议用完就清除
        /// </summary>
        /// <param name="command"></param>
        public void RemoveOPCCommand(OPCCommand command)
        {
            OPCGroups OPCGroups = _server.OPCGroups;
            lock (command)
            {
                if (OPCGroups.GetOPCGroup(command.CommandName) != null)
                {
                    OPCGroups.Remove(command.CommandName);
                    command.OPCGroup = null;
                }
                if (UsingCommandList.Contains(command))
                {
                    UsingCommandList.Remove(command);
                }
            }

        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="command">命令</param>
        public OPCCommand Excute(OPCCommand command)
        {
            return Excute(command, CommandTypeEnum.None);
        }


        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="command">命令</param>
        /// <param name="type">优先级大于命令中的优先级</param>
        /// <returns></returns>
        public OPCCommand Excute(OPCCommand command, CommandTypeEnum type)
        {
            try
            {
                //检测OPCServer状态
                if (OPCServer.ServerState != (int)OPCServerState.OPCRunning)
                {
                    throw new Exception("连接不可用，请检查连接！");
                }
                //检测是否创建组
                if (!UsingCommandList.Contains(command))
                {
                    AddOPCCommand(command);
                }
                CommandTypeEnum finalType = type;
                if (finalType == CommandTypeEnum.None)
                {
                    finalType = command.CommandType;
                }
                //执行
                lock (command)
                {
                    switch (finalType)
                    {
                        case CommandTypeEnum.SyncRead:
                            ExecuteSyncRead(command);
                            break;
                        case CommandTypeEnum.SyncWrite:
                            ExecuteSyncWrite(command);
                            break;
                        case CommandTypeEnum.AsyncRead:
                            ExecuteAsyncRead(command);
                            break;
                        case CommandTypeEnum.AsyncWrite:
                            ExecuteAsyncWrite(command);
                            break;
                        case CommandTypeEnum.DiffSense:
                            SubscribeDiffSensor(command);
                            break;
                        default:
                            throw new OPCServerRWException("不可识别的命令类型！");
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            { }
            return command;
        }

        /// <summary>
        /// 同步读
        /// </summary>
        /// <param name="command">执行命令</param>
        private void ExecuteSyncRead(OPCCommand command)
        {
            try
            {
                DateTime now = DateTime.Now;
                List<int> ServerHandleList = new List<int>();
                foreach (ItemInfo info in command.ItemGroupList)
                {
                    ServerHandleList.Add(info.ServerHandleID);
                }
                int ItemNums = ServerHandleList.Count;
                ServerHandleList.Insert(0, 0);
                Array ServerHandles = ServerHandleList.ToArray();
                Array Values;
                Array Errors;
                object Qulities;
                object TimeStamps;
                //读取
                command.ClearValues();
                command.OPCGroup.SyncRead((short)OPCAutomation.OPCDataSource.OPCCache, ItemNums, ref ServerHandles, out Values, out Errors, out Qulities, out TimeStamps);
                for (int i = 0; i < command.ItemGroupList.Count; i++)
                {
                    if (Convert.ToInt32(Errors.GetValue(i + 1)) == 0)
                    {
                        command.ItemGroupList[i].Error = false;
                    }

                    if (Qulities is Array)
                    {
                        object q = (Qulities as Array).GetValue(i + 1);
                        int qulity = Convert.ToInt32(q == null ? 0 : q);
                        command.ItemGroupList[i].Quality = qulity;
                        if (qulity > 0)
                        {
                            command.ItemGroupList[i].Value = Values.GetValue(i + 1);
                            command.ItemGroupList[i].TimeStamp = now;
                        }
                    }

                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        /// <summary>
        /// 同步写
        /// </summary>
        /// <param name="command">执行命令</param>
        private static void ExecuteSyncWrite(OPCCommand command)
        {
            DateTime now = DateTime.Now;
            try
            {
                List<int> ServerHandleList = new List<int>();
                List<object> ValueList = new List<object>();
                foreach (ItemInfo info in command.ItemGroupList)
                {
                    info.Error = true;
                    if (info.Value != null)
                    {
                        ServerHandleList.Add(info.ServerHandleID);
                        ValueList.Add(info.Value);
                        info.TimeStamp = now;
                    }
                }
                int ItemNums = ServerHandleList.Count;
                if (ItemNums == 0)
                {
                    throw new OPCServerRWException("同步写没有设定值！");
                }
                ServerHandleList.Insert(0, 0);
                ValueList.Insert(0, 0);
                Array ServerHandles = ServerHandleList.ToArray();
                Array Values = ValueList.ToArray();
                Array Errors;
                //写入
                command.OPCGroup.SyncWrite(ItemNums, ref ServerHandles, ref Values, out Errors);
                for (int i = 0; i < ItemNums; i++)
                {
                    int error = Convert.ToInt32(Errors.GetValue(i + 1));
                    if (error == 0)
                    {
                        command.ItemGroupList[i].Error = false;
                    }
                }
                command.CommandState = CommandStateEnum.Write_Over;
            }
            catch (Exception)
            {
                throw;
            }

        }

        /// <summary>
        /// 执行异步读
        /// </summary>
        /// <param name="command"></param>
        private void ExecuteAsyncRead(OPCCommand command)
        {
            try
            {
                command.OPCGroup.IsActive = true;
                command.OPCGroup.IsSubscribed = true;
                command.OPCGroup.AsyncReadComplete -= command.OpcGroupAsyncReadComplete;
                command.OPCGroup.AsyncReadComplete += command.OpcGroupAsyncReadComplete;

                List<int> ServerHandleList = new List<int>();
                foreach (ItemInfo info in command.ItemGroupList)
                {
                    ServerHandleList.Add(info.ServerHandleID);
                }
                int ItemNums = ServerHandleList.Count;

                ServerHandleList.Insert(0, 0);
                Array ServerHandles = ServerHandleList.ToArray();
                int cancelID;
                Array Errors;
                int TransActionID = command.TransactionID == 0 ?StaticUtils.TransActionID: command.TransactionID;
                //异步读
                command.ClearValues();
                command.OPCGroup.AsyncRead(ItemNums, ServerHandles, out Errors, TransActionID, out cancelID);
                command.CancelID = cancelID;
                command.CommandState = CommandStateEnum.Reading;

            }
            catch (Exception)
            {
                throw;
            }

        }

        /// <summary>
        /// 异步写
        /// </summary>
        /// <param name="command"></param>
        private void ExecuteAsyncWrite(OPCCommand command)
        {
            DateTime now = DateTime.Now;
            try
            {
                //订阅事件
                command.OPCGroup.IsActive = true;
                command.OPCGroup.IsSubscribed = true;
                command.OPCGroup.AsyncWriteComplete -= command.OpcGroupAsyncWriteComplete;
                command.OPCGroup.AsyncWriteComplete += command.OpcGroupAsyncWriteComplete;

                List<int> ServerHandleList = new List<int>();
                List<object> ValueList = new List<object>();
                foreach (ItemInfo info in command.ItemGroupList)
                {
                    info.Error = true;
                    if (info.Value != null)
                    {
                        ServerHandleList.Add(info.ServerHandleID);
                        ValueList.Add(info.Value);
                        info.TimeStamp = now;
                    }
                }
                int ItemNums = ServerHandleList.Count;
                if (ItemNums == 0)
                {
                    throw new OPCServerRWException("异步写没有设定值！");
                }
                ServerHandleList.Insert(0, 0);
                ValueList.Insert(0, 0);
                Array ServerHandles = ServerHandleList.ToArray();
                Array Values = ValueList.ToArray();
                Array Errors;
                int TransActionID = command.TransactionID == 0 ? StaticUtils.TransActionID : command.TransactionID;
                int cancelID;
                //异步写
                command.OPCGroup.AsyncWrite(ItemNums, ref ServerHandles, ref Values, out Errors, TransActionID, out cancelID);
                command.CancelID = cancelID;
                command.CommandState = CommandStateEnum.Writing;

            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 注册事件改变以及内部通知方法
        /// </summary>
        /// <param name="command"></param>
        private void SubscribeDiffSensor(OPCCommand command)
        {

            try
            {
                lock (command)
                {
                    command.OPCGroup.IsActive = true;
                    command.OPCGroup.IsSubscribed = true;
                    command.OPCGroup.DataChange += command.DataChanged;
                    command.OPCGroup.UpdateRate = command.SensorUpdateRate;
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        /// <summary>
        /// 针对异步读写的取消(在读/在写)事务的方法
        /// </summary>
        /// <param name="command"></param>
        public void CancelCommandTransaction(OPCCommand command)
        {

            try
            {
                switch (command.CommandType)
                {
                    case CommandTypeEnum.AsyncRead:
                    case CommandTypeEnum.AsyncWrite:
                        command.OPCGroup.AsyncCancel(command.CancelID);
                        break;
                    case CommandTypeEnum.SyncRead:
                    case CommandTypeEnum.SyncWrite:
                    case CommandTypeEnum.DiffSense:
                    default:
                        break;
                }


            }
            catch (Exception)
            {
                throw;
            }

        }
        #endregion

        #region 静态
        private static OPCControl op_server_control;
        /// <summary>
        /// 获取OPCControl实例
        /// </summary>
        /// <param name="ServerName"></param>
        /// <param name="IP"></param>
        /// <returns></returns>
        public static OPCControl GetOPCServerControl(String ServerName, String IP)
        {
            op_server_control = new OPCControl();
            if (ServerName != null && IP != null)
            {
                op_server_control.Connect(ServerName, IP);
            }
            return op_server_control;
        }

        public static OPCControl GetOPCServerControl(OPCServer server)
        {
            op_server_control = new OPCControl(server);
            if (op_server_control.OPCServer.ServerState != (int)OPCServerState.OPCRunning)
            {
                throw new OPCServerRWException("OPCServer不是已经连接的Server");
            }
            return op_server_control;
        }

        public static OPCControl GetOPCServerControl()
        {
            if (op_server_control == null)
            {
                op_server_control = new OPCControl();
            }
            return op_server_control;
        }

        /// <summary>
        /// 释放该OPCControl的连接
        /// </summary>
        public static void Disconnect()
        {
            if (op_server_control != null)
            {
                op_server_control.DisConnect();
            }
        }
        /// <summary>
        /// XML的Command对应类
        /// </summary>
        public static XmlOpcCmdRoot xoc;
        /// <summary>
        /// 读取XML配置文件中该命令名的命令
        /// </summary>
        /// <param name="commandName">命令名</param>
        /// <returns>OPCCommand</returns>
        public static OPCCommand GetNewXmlCommand(string commandName)
        {

            try
            {
                if (xoc == null)
                {
                    xoc = StaticUtils.DESerializer<XmlOpcCmdRoot>(File.ReadAllText(@"Commands.xml"));
                }
                foreach (XmlOpcCmd cmd in xoc.OpcCommands)
                {
                    if (cmd.Name == commandName.Trim())
                    {
                        OPCCommand command = new OPCCommand();
                        //命令名
                        command.CommandName = cmd.Name;
                        //命令类型
                        switch (cmd.Mode.ToLower())
                        {
                            case "syncread":
                                command.CommandType = CommandTypeEnum.SyncRead;
                                break;
                            case "syncwrite":
                                command.CommandType = CommandTypeEnum.SyncWrite;
                                break;
                            case "asyncread":
                                command.CommandType = CommandTypeEnum.AsyncRead;
                                break;
                            case "asyncwrite":
                                command.CommandType = CommandTypeEnum.AsyncWrite;
                                break;
                            case "diffsense":
                                command.CommandType = CommandTypeEnum.DiffSense;
                                break;
                            default:
                                throw new Exception(commandName + "命令MODE类型错误，请选择AsyncRead、AsyncWrite、SyncRead、SyncWrite、DiffSense");
                        }
                        //命令Item
                        foreach (XmlOpcItem itm in cmd.OpcItems)
                        {
                            ItemInfo iif = new ItemInfo(itm.Id);
                            iif.OPCItemType = itm.Type;
                            iif.OPCItemAddress = itm.Address;
                            iif.OPCItemDesc = itm.Describe;
                            command.ItemGroupList.Add(iif);
                        }
                        return command;
                    }
                }

            }
            catch (Exception e)
            {
                throw e;
            }
            return null;
        }


        /// <summary>
        /// 从XML中获取所有的Command
        /// </summary>
        /// <returns></returns>
        public static List<OPCCommand> GetAllXmlCommand()
        {
            try
            {
                List<OPCCommand> cmds = new List<OPCCommand>();
                if (xoc == null)
                {
                    xoc = StaticUtils.DESerializer<XmlOpcCmdRoot>(File.ReadAllText(@"Commands.xml"));
                }
                foreach (XmlOpcCmd cmd in xoc.OpcCommands)
                {
                    OPCCommand command = new OPCCommand();
                    //命令名
                    command.CommandName = cmd.Name;
                    //命令类型
                    switch (cmd.Mode.ToLower())
                    {
                        case "syncread":
                            command.CommandType = CommandTypeEnum.SyncRead;
                            break;
                        case "syncwrite":
                            command.CommandType = CommandTypeEnum.SyncWrite;
                            break;
                        case "asyncread":
                            command.CommandType = CommandTypeEnum.AsyncRead;
                            break;
                        case "asyncwrite":
                            command.CommandType = CommandTypeEnum.AsyncWrite;
                            break;
                        case "diffsense":
                            command.CommandType = CommandTypeEnum.DiffSense;
                            break;
                        default:
                            throw new Exception(command.CommandName + "命令MODE类型错误，请选择AsyncRead、AsyncWrite、SyncRead、SyncWrite、DiffSense");
                    }
                    //命令Item
                    foreach (XmlOpcItem itm in cmd.OpcItems)
                    {
                        ItemInfo itf = new ItemInfo(itm.Id);
                        itf.OPCItemDesc = itm.Describe;
                        itf.OPCItemType = itm.Type;
                        itf.OPCItemAddress = itm.Address;
                        command.ItemGroupList.Add(itf);
                    }
                    cmds.Add(command);

                }
                return cmds;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

    }
}
