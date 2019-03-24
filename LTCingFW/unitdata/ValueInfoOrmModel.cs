 
using LTCingFW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW.unitdata
{
    [OrmTable("value_info")]
    public class ValueInfoOrmModel : OrmBaseModel
    {
		/// <summary>
        /// ID
        /// </summary>
		[Validate(Items = ValidateEnum.NOT_NULL)]
		[OrmColumn("ID", (int)OrmDataType.CommonType.INT,PrimaryKey = true)]	
		public object ID { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>

        [OrmColumn("CREATE_TIME", (int)OrmDataType.CommonType.DATE)]
        public object CREATE_TIME { get; set; }
        /// <summary>
        /// 行号
        /// </summary>

        [OrmColumn("ROW_ID", (int)OrmDataType.CommonType.INT)]	
		public object ROW_ID { get; set; }

        /// <summary>
        /// 所属表
        /// </summary>

        [OrmColumn("TABLE_ID", (int)OrmDataType.CommonType.INT)]
        public object TABLE_ID { get; set; }

        /// <summary>
        /// 列号
        /// </summary>

        [OrmColumn("COLUMN_ID", (int)OrmDataType.CommonType.INT)]	
		public object COLUMN_ID { get; set; }												
		/// <summary>
        /// 值类型
        /// </summary>
		
		[OrmColumn("VALUE_TYPE", (int)OrmDataType.CommonType.STRING)]	
		public object VALUE_TYPE { get; set; }												
		/// <summary>
        /// 整数型实际值
        /// </summary>
		
		[OrmColumn("V_INT", (int)OrmDataType.CommonType.INT)]	
		public object V_INT { get; set; }												
		/// <summary>
        /// 小数型实际值
        /// </summary>
		
		[OrmColumn("V_DECIMAL", (int)OrmDataType.CommonType.DECIMAL)]	
		public object V_DECIMAL { get; set; }												
		/// <summary>
        /// 字符串型实际值
        /// </summary>
		
		[OrmColumn("V_STRING", (int)OrmDataType.CommonType.STRING)]	
		public object V_STRING { get; set; }												
		/// <summary>
        /// 布尔型实际值
        /// </summary>
		
		[OrmColumn("V_BOOL", (int)OrmDataType.CommonType.BOOL)]	
		public object V_BOOL { get; set; }
        /// <summary>
        /// 二进制型实际值
        /// </summary>

        [OrmColumn("V_BINARY", (int)OrmDataType.CommonType.BINARY)]
        public object V_BINARY { get; set; }												
		/// <summary>
        /// 日期型实际值
        /// </summary>
		
		[OrmColumn("V_DATE", (int)OrmDataType.CommonType.DATE)]	
		public object V_DATE { get; set; }												
    }
}
