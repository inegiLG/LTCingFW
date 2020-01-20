using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW.orm
{
    public class OrmBaseService<T> where T: OrmBaseModel
    {

        [Injected(Name = "OrmBaseDao")]
        public OrmBaseDao baseDao;

        /// <summary>
        /// 如果存在就更新如果不存在就插入
        /// </summary>
        /// <param name="model"></param>
        public virtual void Upsert(OrmBaseModel model)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            List<T> rsl = baseDao.SelectByPrimaryKey<T>(session, model);
            if (rsl.Count == 1)
            {
                baseDao.Update(session, model, true);
            }
            else
            {
                baseDao.Insert(session, model);
            }
        }

        /// <summary>
        /// 默认插入
        /// </summary>
        /// <param name="model"></param>
        public virtual void Insert(OrmBaseModel model)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            baseDao.Insert(session, model);
        }
        /// <summary>
        /// 默认删除
        /// </summary>
        /// <param name="model"></param>
        public virtual void Delete(OrmBaseModel model)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            baseDao.Delete(session, model, false);
        }
        /// <summary>
        /// 根据主键删除
        /// </summary>
        /// <param name="model"></param>
        public virtual void DeleteByPk(OrmBaseModel model)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            baseDao.Delete(session, model, true);
        }
        /// <summary>
        /// 默认更新
        /// </summary>
        /// <param name="model"></param>
        public virtual void Update(OrmBaseModel model)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            baseDao.Update(session, model, false);
        }
        /// <summary>
        /// 根据主键更新
        /// </summary>
        /// <param name="model"></param>
        public virtual void UpdateByPk(OrmBaseModel model)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            baseDao.Update(session, model, true);
        }
        /// <summary>
        /// 绝对更新，NULL值也会更新进去
        /// </summary>
        /// <param name="model"></param>
        public virtual void UpdateByPkWithNull(OrmBaseModel model)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            baseDao.Delete(session, model, true);
            baseDao.Insert(session, model);
        }
        /// <summary>
        /// 默认查询
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual DataTable Select(OrmBaseModel model)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            return baseDao.Select(session, model);
        }
        /// <summary>
        /// 默认查询列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual List<T> SelectList(OrmBaseModel model)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            return baseDao.SelectT<T>(session, model);
        }
        /// <summary>
        /// 通过主键查询
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual DataTable SelectByPrimaryKey(OrmBaseModel model)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            return baseDao.SelectByPrimaryKey(session, model);
        }
        /// <summary>
        /// 通过主键查询列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual List<T> SelectByPrimaryKeyList(OrmBaseModel model)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            return baseDao.SelectByPrimaryKey<T>(session, model);
        }
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual DataTable SelectPage(OrmBaseModel model)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            return baseDao.SelectPage(session, model);
        }
        /// <summary>
        /// 分页查询列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual List<T> SelectPageList(OrmBaseModel model)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            return baseDao.SelectPage<T>(session, model);
        }
        /// <summary>
        /// 获取查询的条目数量
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual int GetItemCount(OrmBaseModel model)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            return baseDao.GetItemCount(session, model);
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
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            return baseDao.executeSqlNotQuery(conn, dbTransaction, sql, parameters);
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
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            return baseDao.Select(session, sql, parameters);
        }

    }
}
