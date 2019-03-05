using OPCAutomation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW.opc
{
    /// <summary>
    /// 条目信息
    /// </summary>
    public class ItemInfo
    {

        /// <summary>
        /// OPCItem的ID，即KepWare里完整的名字
        /// </summary>
        public String OPCItemID { get; set; }

        /// <summary>
        /// 客户操作使用ID
        /// </summary>
        public int ClientHandleID { get; set; }

        /// <summary>
        /// 服务器操作使用ID
        /// </summary>
        public int ServerHandleID { get; set; }

        /// <summary>
        /// 操作返回是否有错误,一般为变量格式不匹配
        /// </summary>
        public bool Error { get; set; } = true;

        /// <summary>
        /// 操作返回的数据质量
        /// </summary>
        public int Quality { get; set; }

        /// <summary>
        /// 操作返回的执行时间
        /// </summary>
        public DateTime TimeStamp { get; set; } = new DateTime();

        /// <summary>
        /// 读写所需要的值
        /// </summary>
        public object Value { get; set; }

        public OPCItem OPCItem { get; set; }

        /// <summary>
        /// 读取使用的构造函数
        /// </summary>
        /// <param name="OPCItemID">OPCItem的ID，即KepWare里完整的名字</param>
        public ItemInfo(String OPCItemID) {
            this.OPCItemID = OPCItemID;
        }

        /// <summary>
        /// 写入使用的构造函数
        /// </summary>
        /// <param name="OPCItemID"></param>
        /// <param name="Value"></param>
        public ItemInfo(String OPCItemID, object Value) {
            this.OPCItemID = OPCItemID;
            this.Value = Value;
        }

        /// <summary>
        /// 清除除了OPCItem信息其他信息
        /// </summary>
        public void ClearValues() {
            this.Error = true;
            this.Quality = 0;
            this.TimeStamp = new DateTime();
            this.Value = null;
        }
    }
}
