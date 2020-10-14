 
using LTCingFW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW.unitdata
{
    [OrmTable("column_info", Cached = true)]
    public class ColumnInfoOrmModel : OrmBaseModel
    {
		/// <summary>
        /// ID
        /// </summary>
		[Validate(Items = ValidateEnum.NOT_NULL)]
		[OrmColumn("ID", (int)OrmDataType.CommonType.INT,PrimaryKey = true)]
        public object ID { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>

        [OrmColumn("UPDATE_TIME", (int)OrmDataType.CommonType.DATE)]
        public object UPDATE_TIME { get; set; }
        /// <summary>
        /// 所属表
        /// </summary>

        [OrmColumn("TABLE_ID", (int)OrmDataType.CommonType.INT)]	
		public object TABLE_ID { get; set; }												
		/// <summary>
        /// 列类型
        /// </summary>
		
		[OrmColumn("VALUE_TYPE", (int)OrmDataType.CommonType.STRING)]	
		public object VALUE_TYPE { get; set; }												
		/// <summary>
        /// 列名
        /// </summary>
		
		[OrmColumn("COLUMN_NAME", (int)OrmDataType.CommonType.STRING)]	
		public object COLUMN_NAME { get; set; }												
		/// <summary>
        /// 列描述
        /// </summary>
		
		[OrmColumn("COLUMN_DESC", (int)OrmDataType.CommonType.STRING)]	
		public object COLUMN_DESC { get; set; }												
    }
}
