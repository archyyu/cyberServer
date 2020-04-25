using CashierLibrary.Util;
using CashierServer.Util;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierServer.Logic
{
    public class BaseLogic
    {

        protected CDbMysql mysql = null;

        private ConcurrentStack<CDbMysql> mysqllist = new ConcurrentStack<CDbMysql>();

        protected void initMysql()
        {
            try
            {

                if (mysqllist.Count == 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        mysqllist.Push(new CDbMysql(IniUtil.dbHost(), AesUtil.Decrypt(IniUtil.user()), AesUtil.Decrypt(IniUtil.password()), IniUtil.dbName(), IniUtil.dbPort()));
                    }
                }

                this.mysqllist.TryPop(out mysql);

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("initMysql error", ex);
            }
        }

        protected void closeMysql()
        {
            try
            {
                if (this.mysql.TestConn())
                {
                    mysqllist.Push(this.mysql);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("closeMysql error",ex);
            }
        }

        public virtual void tick() { }

        #region 优化调整

        /// <summary>
        /// 数据库连接
        /// </summary>
        /// <returns></returns>
        protected CDbMysql getConnection()
        {
            CDbMysql mysql;
            while (this.mysqllist.TryPop(out mysql))
            {
                if (mysql.TestConnection())
                {
                    return mysql;
                }
            }

            for (int i = 0; i < 5; i++)
            {
                this.mysqllist.Push(new CDbMysql(IniUtil.dbHost(), AesUtil.Decrypt(IniUtil.user()), AesUtil.Decrypt(IniUtil.password()), IniUtil.dbName(), IniUtil.dbPort()));
            }
            this.mysqllist.TryPop(out mysql);
            return mysql;
        }

        protected void releaseConnection(CDbMysql connection)
        {
            if (connection.TestConn())
            {
                this.mysqllist.Push(connection);
            }
            else
            {
                connection.DbDispose();
            }
        }

        public virtual void syncMemberTick() { }

        public virtual void syncOrderTick() { }

        public virtual void syncOnlineTick() { }

        public virtual void syncOmitMemberTick() { }

        protected List<IDictionary<String, Object>> readList(String readSql, List<String> columns, CDbMysql db)
        {
            List<IDictionary<String, Object>> list = new List<IDictionary<String, Object>>();
            MySqlDataReader reader = db.GetDataReader(readSql);

            try
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        IDictionary<String, Object> item = new Dictionary<String, Object>();
                        for (int i = 0; i < columns.Count; i++)
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
                        if (list.Count > 10)
                        {
                            break;
                        }
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
            }
            return list;
        }

        protected List<IDictionary<String, Object>> readList2(String readSql, List<String> columns, CDbMysql db)
        {
            List<IDictionary<String, Object>> list = new List<IDictionary<String, Object>>();
            MySqlDataReader reader = db.GetDataReader(readSql);

            try
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        IDictionary<String, Object> item = new Dictionary<String, Object>();
                        for (int i = 0; i < columns.Count; i++)
                        {
                            Object o = 0;
                            try
                            {
                                o = reader.GetValue(i);
                            }
                            catch (Exception)
                            {
                                if (reader.GetName(i) == "birthday")
                                {
                                    o = new DateTime(1995, 1, 1, 1, 1, 1);
                                }
                                else
                                {
                                    o = DBNull.Value;
                                }
                            }
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
            }
            return list;
        }

        protected IDictionary<String, Object> readMap(String readSql, List<String> columns, CDbMysql db)
        {
            IDictionary<String, Object> map = new Dictionary<String, Object>();
            MySqlDataReader reader = db.GetDataReader(readSql);

            try
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        for (int i = 0; i < columns.Count; i++)
                        {
                            map.Add(reader.GetName(i), reader.GetValue(i).ToString());
                        }
                        reader.Close();
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
            }
            return map;
        }

        #endregion

        protected List<IDictionary<String, Object>> readList2(String readSql, List<String> columns)
        {
            List<IDictionary<String, Object>> list = new List<IDictionary<String, Object>>();

            MySqlDataReader reader = mysql.GetDataReader(readSql);

            try
            {

                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        IDictionary<String, Object> item = new Dictionary<String, Object>();

                        for (int i = 0; i < columns.Count; i++)
                        {
                            Object o = 0;
                            try
                            {
                                o = reader.GetValue(i);
                            }
                            catch (Exception)
                            {
                                if (reader.GetName(i) == "birthday")
                                {
                                    o = new DateTime(1995, 1, 1, 1, 1, 1);
                                }
                                else
                                {
                                    o = DBNull.Value;
                                }

                            }
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
            }

            
            return list;

        }

        protected List<IDictionary<String, Object>> readList(String readSql, List<String> columns)
        {
            List<IDictionary<String, Object>> list = new List<IDictionary<String, Object>>();


            MySqlDataReader reader = mysql.GetDataReader(readSql);

            try
            {

                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        IDictionary<String, Object> item = new Dictionary<String, Object>();

                        for (int i = 0; i < columns.Count; i++)
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

                        if (list.Count > 10)
                        {
                            break;
                        }
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
            }

           
            return list;

        }

        protected IDictionary<String, Object> readMap(String readSql, List<String> columns)
        {
            IDictionary<String, Object> map = new Dictionary<String, Object>();

            MySqlDataReader reader = mysql.GetDataReader(readSql);
            try
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        for (int i = 0; i < columns.Count; i++)
                        {
                            map.Add(reader.GetName(i), reader.GetValue(i).ToString());
                        }
                        reader.Close();
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
            }
            
            return map;
        }


    }
}
