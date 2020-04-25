using CashierLibrary.Util;
using CashierServer.Util;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 管理cashierServer基本数据库连接
/// </summary>
namespace CashierServer.DAO
{
	public class BaseDao:IDisposable
	{

        private int poolSize = 5;

        private ConcurrentStack<CDbMysql> mysqlList = new ConcurrentStack<CDbMysql>();

		private bool disposed = false;

		/// <summary>
		/// 
		/// </summary>
		public BaseDao()
		{
			this.InitMysql();
		}

		private void InitMysql()
		{
			try
			{
                for (int i = 0; i < poolSize; i++)
                {
                    this.mysqlList.Push(new CDbMysql(IniUtil.dbHost(), AesUtil.Decrypt(IniUtil.user()), AesUtil.Decrypt(IniUtil.password()), IniUtil.dbName(), IniUtil.dbPort()));
                }
			}
			catch (Exception e)
			{
				LogHelper.WriteLog("BaseDao初始化数据库出错", e);
			}
		}

        protected CDbMysql getConnection()
        {
            //
            CDbMysql mysql;

            //TODO
            while (this.mysqlList.TryPop(out mysql))
            {
                if (mysql.TestConnection())
                {
                    return mysql;
                }
            }
            //if (this.mysqlList.TryPop(out mysql))
            //{
            //    return mysql;
            //}
            

            for (int i = 0; i < 5; i++)
            {
                this.mysqlList.Push(new CDbMysql(IniUtil.dbHost(), AesUtil.Decrypt(IniUtil.user()), AesUtil.Decrypt(IniUtil.password()), IniUtil.dbName(), IniUtil.dbPort()));
            }
            this.mysqlList.TryPop(out mysql); 
            return mysql;
        }

        protected void releaseConnection(CDbMysql connection)
        {
            if (connection.TestConn())
            {
                this.mysqlList.Push(connection);
            }
            else
            {
                connection.DbDispose();
            }
        }

        protected List<IDictionary<string, object>> selectList(string readSql)
        {
            List<IDictionary<String, Object>> list = new List<IDictionary<String, Object>>();
            CDbMysql mysql = this.getConnection();
            MySqlDataReader reader = mysql.GetDataReader(readSql);
            if (reader is null)
            {
                return list;
            }
            try
            {

                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        IDictionary<String, Object> item = new Dictionary<String, Object>();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Object o = reader.GetValue(i);
                            if (o.GetType() == typeof(DateTime))
                            {
                                item.Add(reader.GetName(i), ((DateTime)o).ToString("yyyy-MM-dd HH:mm:ss"));
                            }
                            else if (o.GetType() == typeof(SByte) || o.GetType() == typeof(Byte) || o.GetType() == typeof(Boolean) || o.GetType() == typeof(bool))
                            {
                                item.Add(reader.GetName(i), int.Parse(o.ToString()));
                            }
                            else
                            {
                                item.Add(reader.GetName(i), o);
                            }
                        }

                        list.Add(item);
                        
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("readSql:" + readSql, ex);
            }
            finally
            {
                reader.Close();
                this.releaseConnection(mysql);
            }


            return list;
        }

        protected IDictionary<String, Object> selectOne(String readSql)
        {
            IDictionary<String, Object> map = new Dictionary<String, Object>();

            CDbMysql mysql = this.getConnection();
            MySqlDataReader reader = mysql.GetDataReader(readSql);
            try
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Object o = reader.GetValue(i);
                            if (o.GetType() == typeof(DateTime))
                            {
                                map.Add(reader.GetName(i), ((DateTime)o).ToString("yyyy-MM-dd HH:mm:ss"));
                            }
                            else if (o.GetType() == typeof(SByte) || o.GetType() == typeof(Byte) || o.GetType() == typeof(Boolean) || o.GetType() == typeof(bool))
                            {
                                map.Add(reader.GetName(i), int.Parse(o.ToString()));
                            }
                            else
                            {
                                map.Add(reader.GetName(i), o);
                            }
                        }

                        return map;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("readSql:" + readSql, ex);
            }
            finally
            {
                reader.Close();
                this.releaseConnection(mysql);
            }

            return null;
        }

        protected int execute(string sql)
        {
            CDbMysql mysql = this.getConnection();
            try
            {
                return mysql.ExecuteSql(sql);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("excu sql err:",ex);
                return 0;
            }
            finally
            {
                this.releaseConnection(mysql);
            }


        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void CloseMysql()
		{
			try
			{
				foreach(CDbMysql mysql in this.mysqlList)
                {
                    mysql.DbClose();
                }
			}
			catch (Exception ex)
			{
				LogHelper.WriteLog("关闭数据库出错", ex);
			}
		}

		/// <summary>
		/// 实现结构
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// 加入手动处理数据库连接
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if (!this.disposed)
			{
				// If disposing equals true, dispose all managed
				// and unmanaged resources.
				if (disposing)
				{
                    // Dispose managed resources.
                    this.CloseMysql();
				}
				// Note disposing has been done.
				disposed = true;

			}
		}

		/// <summary>
		/// 析构函数
		/// </summary>
		~BaseDao()
		{
			Dispose(true);
		}

	}
}
