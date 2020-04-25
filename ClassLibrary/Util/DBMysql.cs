/*
    MySql 类
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MySql.Data;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace CashierLibrary.Util
{
    public class CDbMysql
    {
        #region 字段设置
        /// <summary>
        /// 数据库连接-IP地址
        /// </summary>
        public string db_host { set; private get; }
        /// <summary>
        /// 数据库连接-用户名
        /// </summary>
        public string db_uname { set; private get; }
        /// <summary>
        /// 数据库连接-密码
        /// </summary>
        public string db_upswd { set; private get; }
        /// <summary>
        /// 数据库连接-数据库名称
        /// </summary>
        public string db_database { set; private get; }
        /// <summary>
        /// 数据库连接-端口
        /// </summary>
        public string db_prost { set; private get; }
        /// <summary>
        /// 数据库连接-数据库编码
        /// </summary>
        public string db_charset { set; private get; }
        /// <summary>
        /// 数据库连接-连接句柄
        /// </summary>
        public MySqlConnection m_conn=null;

		/// <summary>
		/// 事务管理
		/// </summary>
		private MySqlTransaction m_trans = null;

		/// <summary>
		/// 默认不开启事务
		/// </summary>
		private bool isTrans = false;

        /// <summary>
        /// 连接字符串
        /// </summary>
        private string dh_con_string { set; get; }

        public string DbError { private set; get; }
		#endregion

	    #region 构造函数

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="host">主机IP</param>
		/// <param name="uname">用户名</param>
		/// <param name="upassword">密码</param>
		/// <param name="prost">端口</param>
		/// <param name="charset">编码-默认utf8</param>
		public CDbMysql(string host, string uname, string upassword, string dbname, string prost, string charset = "utf8")
        {
            this.db_host = host;
            this.db_uname = uname;
            this.db_upswd = upassword;
            this.db_database = dbname;
            this.db_prost = prost;
            this.db_charset = charset;
            // User Id=root;Host=localhost;Database=studb;Password=root;Port=3307
            this.dh_con_string = string.Format("User Id={0};Host={1};Database={2};Password={3};Port={4};Allow User Variables=True;Treat Tiny As Boolean=false", this.db_uname,
                                               this.db_host, this.db_database, this.db_upswd, this.db_prost
                                               );

            this.DbConnection();
        }


        #endregion


        #region 公共函数

        /// <summary>
        /// 执行脚本文件
        /// </summary>
        /// <param name="script"></param>
        public void ExecuteScript(String script)
        {
            try
            {
                MySqlScript sqlScript = new MySqlScript(this.m_conn);
                sqlScript.Query = script;
                int nCount = sqlScript.Execute();
                LogHelper.WriteLog("执行数据脚本，共执行[" + nCount + "]条语句");
            }
            catch (Exception ex)
            {

                LogHelper.WriteLog("执行数据脚本出错，错误原因：\n", ex);
            }
        }

		/// <summary>
		/// 执行SQL语句
		/// </summary>
		/// <param name="QueryString"></param>
		/// <returns></returns>
		public int ExecuteSql(string QueryString)
		{
			try
			{
				using (MySqlCommand comm = new MySqlCommand(QueryString, this.m_conn))
				{
					int result = comm.ExecuteNonQuery();
					return result;
				}
			}
			catch (MySqlException ex)
			{
				this.DbError = ex.Message.ToString();
                LogHelper.WriteLog("执行数据脚本" + QueryString + "出错，错误原因：\n", ex);
				return -1;
			}
		}

		/// <summary>
		/// 执行存储过程
		/// </summary>
		/// <param name="storedProcName">存储过程名称</param>
		/// <param name="parameters">存储过程参数列表</param>
		/// <returns>数据集</returns>
		public DataSet RunProcedure(string storedProcName, MySqlParameter[] parameters)
		{
            using (MySqlCommand cmd = new MySqlCommand(storedProcName, this.m_conn))
            {
                cmd.CommandText = storedProcName;
                cmd.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                {

                    foreach (MySqlParameter parameter in parameters)
                    {

                        if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) && parameter.Value == null)
                        {
                            parameter.Value = DBNull.Value;
                        }
                        cmd.Parameters.Add(parameter);
                    }

                }
                using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear(); 
                    }
                    catch (MySql.Data.MySqlClient.MySqlException ex)
                    {
                        throw new Exception(ex.Message);
                    }

                    return ds;
                }
            }
            

		}

		/// <summary>
		/// 执行一个带参数的存储过程，注意事务处理是在存储中进行
		/// </summary>
		/// <param name="procName">存储过程名称</param>
		/// <param name="parameters">输入输出参数列表</param>
		/// <returns>-1失败，》=0成功</returns>
		public int ExcutProcedure(String procName, MySqlParameter[] parameters)
		{
			int nRet = -1;
			try
			{

				MySqlCommand cmd = new MySqlCommand();
				cmd.Connection = this.m_conn;
				cmd.CommandText = procName;
				cmd.CommandType = CommandType.StoredProcedure;

				if (parameters != null)
				{

					foreach (MySqlParameter parameter in parameters)
					{

						if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) && parameter.Value == null)
						{
							parameter.Value = DBNull.Value;
						}
						cmd.Parameters.Add(parameter);
					}

				}
				nRet = cmd.ExecuteNonQuery();

			}
			catch (Exception ex)
			{

				throw ex;
			}
			return nRet;
		}

		public DataTable ExcProcedure(String storedProcName, MySqlParameter[] parameters)
		{

			MySqlCommand cmd = new MySqlCommand();
			cmd.Connection = this.m_conn;
			cmd.CommandText = storedProcName;
			cmd.CommandType = CommandType.StoredProcedure;

			if (parameters != null)
			{

				foreach (MySqlParameter parameter in parameters)
				{

					if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) && parameter.Value == null)
					{
						parameter.Value = DBNull.Value;
					}
					cmd.Parameters.Add(parameter);
				}

			}
			using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
			{
				DataSet ds = new DataSet();
				try
				{
					da.Fill(ds, "ds");
					cmd.Parameters.Clear();
				}
				catch (MySql.Data.MySqlClient.MySqlException ex)
				{
					throw new Exception(ex.Message);
				}
				if (null != ds && ds.Tables.Count > 0)
				{
					return ds.Tables[0];
				}
				else
					return new DataTable();

				
			}
		}


		/// <summary>
		/// 返回DataTable
		/// </summary>
		/// <param name="storedProcName">存储名称</param>
		/// <param name="TableName">表名称</param>
		/// <returns>返回数据表</returns>
		public DataTable ExcutProcedure(String storedProcName, String TableName)
		{
			MySqlCommand cmd = new MySqlCommand();
			cmd.Connection = this.m_conn;
			cmd.CommandText = storedProcName;
			cmd.CommandType = CommandType.StoredProcedure;


			using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
			{
				DataSet ds = new DataSet();
				try
				{
					da.Fill(ds, TableName);
					cmd.Parameters.Clear();
				}
				catch (MySql.Data.MySqlClient.MySqlException ex)
				{
					throw new Exception(ex.Message);
				}

				return ds.Tables[0];
			}
		}

		/// <summary>
		/// 返回DataTable
		/// </summary>
		/// <param name="SqlString"></param>
		/// <param name="TablName"></param>
		/// <returns></returns>
		public DataTable GetDataTable(string SqlString, string TablName)
		{
			try
			{
                using (MySqlDataAdapter Da = new MySqlDataAdapter(SqlString, this.m_conn))
                {
                    DataTable dt = new DataTable(TablName);
                    Da.Fill(dt);
                    return dt;

                }
			}
			catch (MySqlException ex)
			{
				this.DbError = ex.Message.ToString();
                this.DbClose();
                LogHelper.WriteLog("CDBmysql,获取数据库出错",ex);
			}
            return null;
		}
		/// <summary>
		/// 返回DataReader对象
		/// </summary>
		/// <param name="SqlString"></param>
		/// <returns></returns>
		public MySqlDataReader GetDataReader(string SqlString)
		{
			try
			{
				MySqlCommand comm = new MySqlCommand(SqlString, this.m_conn);
				MySqlDataReader dread = comm.ExecuteReader(CommandBehavior.Default);
				return dread;
			}
			catch (MySqlException ex)
			{
				this.DbError = ex.Message.ToString();
                this.DbClose();
				LogHelper.WriteLog("mysql error", ex);
				return null;
			}

		}
		/// <summary>
		/// 获取DataAdapter对象
		/// </summary>
		/// <param name="SqlString"></param>
		/// <returns></returns>
		private MySqlDataAdapter GetDataAdapter(string SqlString)
		{
			try
			{
				MySqlDataAdapter dadapter = new MySqlDataAdapter(SqlString, this.m_conn);
				return dadapter;
			}
			catch (MySqlException ex)
			{
				this.DbError = ex.Message.ToString();
                this.DbClose();
				return null;
			}

		}
		/// <summary>
		/// 返回DataSet对象
		/// </summary>
		/// <param name="SqlString"></param>
		/// <param name="TableName"></param>
		/// <returns></returns>
		public DataSet GetDataSet(string SqlString, string TableName)
		{
			try
			{
				MySqlDataAdapter Da = this.GetDataAdapter(SqlString);
				DataSet ds = new DataSet();
				Da.Fill(ds, TableName);
				return ds;
			}
			catch (MySqlException ex)
			{
				this.DbError = ex.Message.ToString();
                this.DbClose();
                LogHelper.WriteLog("DBMysql,GetDataSet出错",ex);
				return null;
			}
		}

        /// <summary>
        /// 查询数据量
        /// </summary>
        /// <param name="SqlString">count sql</param>
        /// <returns></returns>
        public int GetCount(string SqlString)
        {
            int count = 0;
            try
            {
                MySqlCommand comm = new MySqlCommand(SqlString, this.m_conn);
                count = Convert.ToInt32(comm.ExecuteScalar().ToString());
            }
            catch (Exception ex)
            {
                count = -1;
                this.DbClose();
                this.DbError = ex.Message.ToString();
                LogHelper.WriteLog("DBMysql,GetCount出错", ex);
            }

            return count;
        }
		/// <summary>
		/// 获取一条数据
		/// </summary>
		/// <param name="SqlString"></param>
		/// <returns></returns>
		public string GetOne(string SqlString)
		{
			string result = null;
			try
			{
				MySqlCommand comm = new MySqlCommand(SqlString, this.m_conn);
				MySqlDataReader dr = comm.ExecuteReader();
				if (dr.Read())
				{
					result = dr[0].ToString();
					dr.Close();
				}
				else
				{
					result = null;
					dr.Close();
				}

			}
			catch (MySqlException ex)
			{
                this.DbClose();
				this.DbError = ex.Message.ToString();
			}
			return result;
		}
		/// <summary>
		/// 连接测试
		/// </summary>
		/// <returns></returns>
		public bool TestConn()
		{
            return this.TestConnection();
		}

        public bool TestConnection()
        {
             return this.GetCount("select 1") > 0;
        }


        /// <summary>
        /// 关闭数据库句柄
        /// </summary>
        public void DbClose(MySqlConnection DbHeader)
		{
			if (DbHeader != null)
			{
				this.m_conn.Close();
				this.m_conn.Dispose();
			};

			GC.Collect();
		}

		public void DbClose()
		{
			if (null != this.m_conn)
			{
				//this.m_conn.Close();
				//this.m_conn.Dispose();
			}
			GC.Collect();
		}

        public void DbDispose()
        {
            this.m_conn.Dispose();
        }

		#endregion

		#region 私有函数

		/// <summary>
		/// 连接数据库
		/// </summary>
		private void DbConnection()
		{
			this.m_conn = new MySqlConnection(this.dh_con_string);
			try
			{
				this.m_conn.Open();
			}
			catch (Exception ex)
			{
                this.DbClose();
				LogHelper.WriteLog("实例化数据库对象出错:", ex);
			}
		}


		#endregion





	}
}