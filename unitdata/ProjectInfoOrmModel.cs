 
using LTCingFW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW.unitdata
{
    [OrmTable("project_info", Cached =true)]
    public class ProjectInfoOrmModel : OrmBaseModel
    {
        /// <summary>
        /// 项目名
        /// </summary>
        [Validate(Items = ValidateEnum.NOT_NULL)]
        [OrmColumn("PROJECT_NAME", (int)OrmDataType.CommonType.STRING, PrimaryKey = true)]
        public object PROJECT_NAME { get; set; }
        
        /// <summary>
        /// 更新时间
        /// </summary>

        [OrmColumn("UPDATE_TIME", (int)OrmDataType.CommonType.DATE)]
        public object UPDATE_TIME { get; set; }
												
		/// <summary>
        /// 项目描述
        /// </summary>
		[OrmColumn("PROJECT_DESC", (int)OrmDataType.CommonType.STRING)]	
		public object PROJECT_DESC { get; set; }

        //关联表
        [OrmForeign(ForeignColumnName = "OWNER_PROJECT", LocalColumnName = "PROJECT_NAME", LZModel = LZModelEnum.LAZY)]
        public ForeignOrmModel<TableInfoOrmModel> TableList { get; set; }
    }
}
