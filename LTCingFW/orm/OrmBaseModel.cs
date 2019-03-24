using LTCingFW;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    public class OrmBaseModel
    {

        //分页使用
        protected DataTable tablePage = new DataTable();
        /// <summary>
        /// 当前页数，默认为1
        /// </summary>
        protected int current_page = 1;
        public virtual int CurrentPage {
            get { return current_page; }
            set { current_page = value; }
        }
        /// <summary>
        /// 每页数据条目数,默认20条
        /// </summary>
        protected int page_item_count = 20;
        public virtual int PageItemCount
        {
            get { return page_item_count; }
            set { page_item_count = value; }
        }
        /// <summary>
        /// 总页数,默认为1
        /// </summary>
        protected int total_page_count = 1;
        /// <summary>
        /// 总条数，默认为0
        /// </summary>
        protected int total_item_count = 0;



        /// <summary>
        /// 获取下限
        /// </summary>
        /// <returns></returns>
        public int LowLimitNumber
        {
            get
            {
                return (current_page - 1) * page_item_count + 1;
            }
        }

        /// <summary>
        /// 获取上限
        /// </summary>
        /// <returns></returns>
        public int UpLimitNumber
        {
            get
            {
                return current_page * page_item_count;
            }
        }

        /// <summary>
        /// 模糊查询的项
        /// </summary>
        public List<string> FuzzyColumnNames { get; } = new List<string>();

        /// <summary>
        /// 自定义OrderBy
        /// </summary>
        public string OrderBy { get; set; }
        /// <summary>
        /// 多为日期等带有大于小于或模糊查询使用
        /// </summary>
        public string Where { get; set; }
        /// <summary>
        /// 是否使用distinct
        /// </summary>
        public bool Distinct { get; set; } = false;

        /// <summary>
        /// 查询上锁，预防 脏读
        /// </summary>
        public bool SelectForUpdate { get; set; } = false;
        /// <summary>
        /// 是否使用SqlServer的Select语句的Top
        /// </summary>
        public int SqlServerTop { get; set; } = 0;
        private List<OrmColumnBean> ormList;
        public List<OrmColumnBean> OrmList
        {
            get
            {
                if (ormList == null || ormList.Count == 0)
                {
                    if (ormList == null)
                    {
                        ormList = new List<OrmColumnBean>();
                    }
                    PropertyInfo[] infos = this.GetType().GetProperties();

                    foreach (PropertyInfo info in infos)
                    {
                        object[] attrs = info.GetCustomAttributes(typeof(OrmColumnAttribute), true);
                        if (attrs.Length > 0)
                        {
                            OrmColumnBean ormBean = new OrmColumnBean();
                            foreach (OrmColumnAttribute attr in attrs)
                            {

                                //列上不存在时寻找表的DbAlias
                                if (attr.DbAlias == null)
                                {
                                    OrmTableAttribute tbAttr = this.GetType().GetCustomAttribute<OrmTableAttribute>();
                                    if (tbAttr.DbAlias == null)
                                    {
                                        //循环加入到所有连接内
                                        foreach (String dbAlias in LTCingFWSet.dbDic.Keys)
                                        {
                                            ormBean.OrmColumnAttributeDic.Add(dbAlias, attr);
                                        }
                                    }
                                    else
                                    {
                                        ormBean.OrmColumnAttributeDic.Add(tbAttr.DbAlias, attr);
                                    }

                                }
                                //存在时单加
                                else
                                {
                                    ormBean.OrmColumnAttributeDic.Add(attr.DbAlias, attr);
                                }

                            }
                            ormBean.ColumnName = info.Name;
                            ormBean.Value = info.GetValue(this);
                            ormList.Add(ormBean);

                        }
                    }
                }
                else
                {
                    foreach (OrmColumnBean bean in ormList)
                    {
                        PropertyInfo info = this.GetType().GetProperty(bean.ColumnName);
                        bean.Value = info.GetValue(this);
                    }

                }
                if (ormList.Count == 0)
                {
                    throw new Exception(this.GetType() + "类中无数据库对应列!");
                }
                return ormList;
            }
        }





    }
}
