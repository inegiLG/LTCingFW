using LTCingFW.beans;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    public class OrmBaseService
    {

        [Injected(Name = "OrmBaseDao")]
        public OrmBaseDao dao;

        /// <summary>
        /// 查询单条
        /// 使用model作为查询条件;
        /// 根据model中的primarykey属性查询;
        /// 根据model中的where属性查询;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query_model"></param>
        /// <returns>T</returns>
        public virtual T QueryByPk<T>(T query_model) where T : OrmBaseModel
        {
            ThreadContext t = LTCingFWSet.GetThreadContext();
            DBSession session = t.DBSession;
            List<T> res = dao.SelectByPrimaryKey<T>(session, query_model);
            if (res.Count == 1)
            {
                return res[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 查询集合
        /// 使用model作为查询条件;
        /// 根据model中的非空属性查询;
        /// 根据model中的where属性查询;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query_model"></param>
        /// <returns>List<T></returns>
        public virtual List<T> QueryList<T>(T query_model) where T : OrmBaseModel
        {
                ThreadContext t = LTCingFWSet.GetThreadContext();
                DBSession session = t.DBSession;
                return dao.SelectT<T>(session, query_model);
        }

        /// <summary>
        /// 查询集合
        /// 使用model作为查询条件;
        /// 根据model中的非空属性查询;
        /// 根据model中的where属性查询;
        /// </summary>
        /// <param name="query_model"></param>
        /// <returns>DataTable</returns>
        public virtual DataTable QueryList_DT(OrmBaseModel query_model)
        {
            ThreadContext t = LTCingFWSet.GetThreadContext();
            DBSession session = t.DBSession;
            return dao.Select(session, query_model);
        }

        /// <summary>
        /// 分页查询
        /// 使用model作为查询条件;
        /// 填写mdoel中当前页(current_page)和每页条数(page_item_count)分页信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query_model"></param>
        /// <returns>List<T></returns>
        public virtual List<T> QueryPage<T>(T query_model) where T : OrmBaseModel
        {
            ThreadContext t = LTCingFWSet.GetThreadContext();
            DBSession session = t.DBSession;
            return dao.SelectPage<T>(session, query_model);
        }

        /// <summary>
        /// 分页查询
        /// 使用model作为查询条件;
        /// 填写mdoel中当前页(current_page)和每页条数(page_item_count)分页信息
        /// </summary>
        /// <param name="query_model"></param>
        /// <returns>DataTable</returns>
        public virtual DataTable QueryPage_DT(OrmBaseModel query_model)
        {
            ThreadContext t = LTCingFWSet.GetThreadContext();
            DBSession session = t.DBSession;
            return dao.SelectPage(session, query_model);
        }

        /// <summary>
        /// 查询条数
        /// 使用model作为查询条件;
        /// 查询该条件下的所有条目
        /// </summary>
        /// <param name="query_model"></param>
        /// <returns></returns>
        public virtual int GetItemCount(OrmBaseModel query_model)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            return dao.GetItemCount(session, query_model);
        }

        /// <summary>
        /// 插入
        /// 使用model作为插入内容;
        /// </summary>
        /// <param name="insert_model"></param>
        public virtual void Insert(OrmBaseModel insert_model)
        {
            ThreadContext t = LTCingFWSet.GetThreadContext();
            DBSession session = t.DBSession;
            dao.Insert(session, insert_model);
        }

        /// <summary>
        /// 更新
        /// 使用Where查询条件,Where不可为空;
        /// 使用model作为更新内容;
        /// </summary>
        /// <param name="update_model"></param>
        public virtual void UpdateByWhere(OrmBaseModel update_model)
        {
            ThreadContext t = LTCingFWSet.GetThreadContext();
            DBSession session = t.DBSession;
            dao.Update(session, update_model, false);
        }

        /// <summary>
        /// 更新
        /// 使用model中的主键属性作为查询条件;
        /// 使用Where查询条件;
        /// 使用model中的非主键属性作为更新内容;
        /// </summary>
        /// <param name="update_model"></param>
        public virtual void UpdateByPkWhere(OrmBaseModel update_model)
        {
            ThreadContext t = LTCingFWSet.GetThreadContext();
            DBSession session = t.DBSession;
            dao.Update(session, update_model, true);
        }

        /// <summary>
        /// 原数据库中存在则更新，不存在则插入
        /// 使用Where查询条件,Where不可为空;
        /// 使用model中的非主键属性作为更新内容;
        /// 使用model作为插入内容;
        /// </summary>
        /// <param name="model"></param>
        public virtual void UpSertByWhere(OrmBaseModel model)
        {
            ThreadContext t = LTCingFWSet.GetThreadContext();
            DBSession session = t.DBSession;
            DataTable rsl = dao.SelectByPrimaryKey(session, model);
            if (rsl.Rows.Count > 0)
            {
                dao.Update(session, model, false);
            }
            else
            {
                dao.Insert(session, model);
            }
        }

        /// <summary>
        /// 原数据库中存在则更新，不存在则插入
        /// 使用model中的主键属性作为查询条件;
        /// 使用Where查询条件;
        /// 使用model中的非主键属性作为更新内容;
        /// 使用model作为插入内容;
        /// </summary>
        /// <param name="update_model"></param>
        public virtual void UpSertByPk(OrmBaseModel update_model)
        {
            ThreadContext t = LTCingFWSet.GetThreadContext();
            DBSession session = t.DBSession;
            DataTable rsl = dao.SelectByPrimaryKey(session, update_model);
            if (rsl.Rows.Count == 1)
            {
                dao.Update(session, update_model, true);
            }
            else
            {
                dao.Insert(session, update_model);
            }
        }

        /// <summary>
        /// 删除
        /// 使用model作为查询条件;
        /// </summary>
        /// <param name="query_model"></param>
        public virtual void Delete(OrmBaseModel query_model)
        {
            ThreadContext t = LTCingFWSet.GetThreadContext();
            DBSession session = t.DBSession;
            dao.Delete(session, query_model, true);
        }

        /// <summary>
        /// 自定义SQL语句，针对UPDATE/DELETE/INSERT
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dbTransaction"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual int UserDefinedSqlNotQuery(DbConnection conn, DbTransaction dbTransaction, String sql, DbParameter[] parameters)
        {
            ThreadContext t = LTCingFWSet.GetThreadContext();
            DBSession session = t.DBSession;
            return dao.executeSqlNotQuery(conn, dbTransaction, sql, parameters);
        }
        /// <summary>
        /// 自定义SQL查询
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual DataTable UserDefinedQuery(DbConnection conn, DbTransaction dbTransaction, String sql, DbParameter[] parameters)
        {
            ThreadContext t = LTCingFWSet.GetThreadContext();
            DBSession session = t.DBSession;
            return dao.Select(session, sql, parameters);
        }

    }
}
