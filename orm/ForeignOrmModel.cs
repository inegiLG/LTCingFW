using LTCingFW.utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    public class ForeignOrmModel<T> : OrmBaseDao
    {
        public string DbAlias { get; set; }
        public OrmBaseModel OwnerModel { get; set; }
        public OrmForeignAttribute Attr { get; set; }

        private List<T> result;
        /// <summary>
        /// 结果
        /// </summary>
        public List<T> Result
        {
            get {
                DBSession session = null ;
                try
                {
                    //LAZY暂未处理,此时处理
                    if (result == null)
                    {
                        //1.查询所需的条件实例
                        Type aimType = typeof(T);
                        object inst = aimType.Assembly.CreateInstance(aimType.FullName);
                        //2.查询所需的条件变量读取并设置到条件实例中
                        string[] localColumnNames = Attr.LocalColumnName.Split(',');
                        string[] foreignColumnNames = Attr.ForeignColumnName.Split(',');
                        for (int i = 0; i < localColumnNames.Length; i++)
                        {
                            object value = FwUtilFunc.GetObjectPropertyValue(OwnerModel, localColumnNames[i]);
                            FwUtilFunc.SetObjectPropertyValue(inst, foreignColumnNames[i], value);
                        }
                        //3.查询
                        session = DBSession.OpenSession(DbAlias,false) ;
                        MethodInfo Select_Method = typeof(OrmBaseDao).GetMethod("SelectT", new Type[] { typeof(DBSession), typeof(OrmBaseModel) });
                        MethodInfo Cur_Select_Method = Select_Method.MakeGenericMethod(aimType);
                        object Final_Result = Cur_Select_Method.Invoke(this, new object[] { session, inst as OrmBaseModel });
                        //DataTable resultDT = Select(session, inst as OrmBaseModel);
                        //4.通过反射调用LoadOrmModelListFromDataTable<T>，返回List<T>
                        //MethodInfo Load_Method = typeof(FwUtilFunc).GetMethod("LoadOrmModelListFromDataTable", new Type[] { typeof(DataTable) });
                        //MethodInfo Cur_Load_Method = Load_Method.MakeGenericMethod(aimType);
                        //object Final_Result = Cur_Load_Method.Invoke(null, new object[] { resultDT });
                        //5.设置List进如ForeignOrmModel
                        result = Final_Result as List<T>;
                    }
                    return result;
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally {
                   
                }
            }
            set {
                result = value;
            }
        }

       
    }
}
