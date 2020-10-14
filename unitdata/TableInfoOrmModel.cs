 
using LTCingFW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW.unitdata
{
    [OrmTable("table_info", Cached = true)]
    public class TableInfoOrmModel : OrmBaseModel
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
        /// 表名
        /// </summary>

        [OrmColumn("TABLE_NAME", (int)OrmDataType.CommonType.STRING)]	
		public object TABLE_NAME { get; set; }												
		/// <summary>
        /// 表描述
        /// </summary>
		
		[OrmColumn("TABLE_DESC", (int)OrmDataType.CommonType.STRING)]	
		public object TABLE_DESC { get; set; }												
		/// <summary>
        /// 所属项目
        /// </summary>
		
		[OrmColumn("OWNER_PROJECT", (int)OrmDataType.CommonType.STRING, PrimaryKey = true)]	
		public object OWNER_PROJECT { get; set; }
        /// <summary>
        /// 是否使用行
        /// </summary>

        [OrmColumn("USE_ROW", (int)OrmDataType.CommonType.INT)]
        public object USE_ROW { get; set; } = 0;
        /// <summary>
        /// 是否使用列
        /// </summary>

        [OrmColumn("USE_COLUMN", (int)OrmDataType.CommonType.INT)]
        public object USE_COLUMN { get; set; } = 1;

        [OrmForeign(ForeignColumnName = "TABLE_ID", LocalColumnName = "ID", LZModel = LZModelEnum.LAZY)]
        public ForeignOrmModel<ColumnInfoOrmModel> ColumnList { get; set; }
    }
}
