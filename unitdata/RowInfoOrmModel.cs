 
using LTCingFW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW.unitdata
{
    [OrmTable("row_info")]
    public class RowInfoOrmModel : OrmBaseModel
    {
		/// <summary>
        /// ID
        /// </summary>
		[Validate(Items = ValidateEnum.NOT_NULL)]
		[OrmColumn("ID", (int)OrmDataType.CommonType.INT,PrimaryKey = true)]	
		public object ID { get; set; }
        /// <summary>
        /// 所属表
        /// </summary>
        [Validate(Items = ValidateEnum.NOT_NULL)]
        [OrmColumn("OWNER_TABLE", (int)OrmDataType.CommonType.INT, PrimaryKey = true)]	
		public object OWNER_TABLE { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Validate(Items = ValidateEnum.NOT_NULL)]
        [OrmColumn("CREATE_TIME", (int)OrmDataType.CommonType.DATE)]
        public object CREATE_TIME { get; set; }
    }
}
