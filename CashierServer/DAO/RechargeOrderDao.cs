using CashierLibrary.Model.Bill;
using CashierLibrary.Util;
using CashierServer.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CashierServer.DAO
{
    class RechargeOrderDao : BaseDao
    {

        public IDictionary<string, object> AddRechargeOrder(RechargeOrderDTO dto)
        {

            IDictionary<string, object> root = new Dictionary<string, object>();

            // 获取序列号
            long idseq = this.GetSeqRechargeOrder();
            if (idseq == 0)
            {
                root["state"] = -1;
                root["info"] = "序列号生成失败";
                return root;
            }

            CDbMysql mysql = getConnection();
            try
            {
                List<MySqlParameter> parameters = new List<MySqlParameter>();

                MySqlParameter sourceParameter = new MySqlParameter("@source_", MySqlDbType.Int32);
                sourceParameter.Value = dto.orderSource;
                sourceParameter.Direction = ParameterDirection.Input;
                parameters.Add(sourceParameter);

                MySqlParameter rechargeCompaignIDParameter = new MySqlParameter("@rechargeCompaignID_", MySqlDbType.Int32);
                rechargeCompaignIDParameter.Value = dto.rechargeCompaignID;
                rechargeCompaignIDParameter.Direction = ParameterDirection.Input;
                parameters.Add(rechargeCompaignIDParameter);

                MySqlParameter memberIDParameter = new MySqlParameter("@memberID_", MySqlDbType.UInt64);
                memberIDParameter.Value = dto.memberID;
                memberIDParameter.Direction = ParameterDirection.Input;
                parameters.Add(memberIDParameter);


                MySqlParameter rechargeWayParameter = new MySqlParameter("@rechargeWay_", MySqlDbType.Byte);
                rechargeWayParameter.Value = dto.rechargeWay;
                rechargeWayParameter.Direction = ParameterDirection.Input;
                parameters.Add(rechargeWayParameter);

                MySqlParameter rechargeTypeParameter = new MySqlParameter("@rechargeType_", MySqlDbType.Byte);
                rechargeTypeParameter.Value = dto.rechargeType;
                rechargeTypeParameter.Direction = ParameterDirection.Input;
                parameters.Add(rechargeTypeParameter);

                MySqlParameter depositParameter = new MySqlParameter("@deposit_", MySqlDbType.Decimal);
                depositParameter.Value = dto.cashBalance;
                depositParameter.Direction = ParameterDirection.Input;
                parameters.Add(depositParameter);

                MySqlParameter rechargeFeeParameter = new MySqlParameter("@rechargeFee_", MySqlDbType.Decimal);
                rechargeFeeParameter.Value = dto.rechargeFee;
                rechargeFeeParameter.Direction = ParameterDirection.Input;
                parameters.Add(rechargeFeeParameter);

                MySqlParameter adwardFeeParameter = new MySqlParameter("@adwardFee_", MySqlDbType.Decimal);
                adwardFeeParameter.Value = dto.awardFee;
                adwardFeeParameter.Direction = ParameterDirection.Input;
                parameters.Add(adwardFeeParameter);

                MySqlParameter stateParameter = new MySqlParameter("@state_", MySqlDbType.Byte);
                stateParameter.Value = dto.state;
                stateParameter.Direction = ParameterDirection.Input;
                parameters.Add(stateParameter);

                MySqlParameter posAccountParameter = new MySqlParameter("@posAccount_", MySqlDbType.Int32);
                posAccountParameter.Value = dto.posAccount;
                posAccountParameter.Direction = ParameterDirection.Input;
                parameters.Add(posAccountParameter);

                MySqlParameter rechargeSourceParameter = new MySqlParameter("@rechargeSource_", MySqlDbType.Byte);
                rechargeSourceParameter.Value = dto.rechargeSource;
                rechargeSourceParameter.Direction = ParameterDirection.Input;
                parameters.Add(rechargeSourceParameter);

                MySqlParameter gidParameter = new MySqlParameter("@gid_", MySqlDbType.Int32);
                gidParameter.Value = dto.gid;
                gidParameter.Direction = ParameterDirection.Input;
                parameters.Add(gidParameter);

                MySqlParameter idseqParameter = new MySqlParameter("@idseq", MySqlDbType.UInt64);
                idseqParameter.Value = idseq;
                idseqParameter.Direction = ParameterDirection.Input;
                parameters.Add(idseqParameter);

                MySqlParameter eventIDParameter = new MySqlParameter("@eventID_", MySqlDbType.Int32);
                if (null == dto.eventID)
                {
                    dto.eventID = 0;
                }
                eventIDParameter.Value = dto.eventID;
                eventIDParameter.Direction = ParameterDirection.Input;
                parameters.Add(eventIDParameter);

                MySqlParameter result = new MySqlParameter("@result", MySqlDbType.Int32);
                result.Value = -1;
                result.Direction = ParameterDirection.Output;
                parameters.Add(result);

                DataTable dt = mysql.ExcProcedure("addRechargeOrderNew", parameters.ToArray());
                if (result.Value != null)
                {
                    IDictionary<string, object> data = new Dictionary<string, object>();
                    if (Convert.ToInt32(result.Value) == 1)
                    {
                        data["cashBalance"] = Convert.ToDecimal(dt.Rows[0]["deposit"] == null ? "0" : dt.Rows[0]["deposit"].ToString());
                        data["baseBalance"] = Convert.ToDecimal(dt.Rows[0]["baseBalance"] == null ? "0" : dt.Rows[0]["baseBalance"].ToString());
                        data["awardBalance"] = Convert.ToDecimal(dt.Rows[0]["awardBalance"] == null ? "0" : dt.Rows[0]["awardBalance"].ToString());
                        //data["memberID"] = Convert.ToDouble(dt.Rows[0]["memberID"] == null ? "0" : dt.Rows[0]["memberID"].ToString());
                        data["orderID"] = Convert.ToDouble(dt.Rows[0]["rechargeOrderID_"] == null ? "0" : dt.Rows[0]["rechargeOrderID_"].ToString());

                        LogHelper.WriteLog("add recharge order success, id = " + idseq + ", memberID = " + dto.memberID.ToString());
                        root["state"] = 0;
                        root["info"] = "成功";
                        root["data"] = data;
                        return root;
                    }

                    data["memberID"] = "0";

                    root["state"] = Convert.ToInt32(result.Value);
                    root["data"] = data;

                    if (Convert.ToInt32(result.Value) == 1403)
                    {
                        root["info"] = "会员不存在";
                    }
                    else if (Convert.ToInt32(result.Value) == 14035)
                    {
                        root["info"] = "会员账户不存在";
                    }
                    else
                    {
                        root["state"] = -1;
                        root["info"] = "未知";
                    }
                    LogHelper.WriteLog("add recharge order error, id = " + idseq + ", memberID = " + dto.memberID.ToString() + ", info=" + JsonUtil.SerializeObject(root));
                    return root;
                }
                else
                {
                    LogHelper.WriteLog("会员充值 【" + dto.memberID + "】 出错,错误码:\n" + Convert.ToInt32(null == result.Value ? "-1" : result.Value));
                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("会员充值数据出错:\n" + ex.Message);
            }
            finally
            {
                this.releaseConnection(mysql);
            }

            root["state"] = -1;
            return root;
        }

        /// <summary>
		/// 获取序列号
		/// </summary>
		/// <returns></returns>
        private long GetSeqRechargeOrder()
        {
            CDbMysql mysql = getConnection();

            try
            {
                List<MySqlParameter> parameters = new List<MySqlParameter>();
                MySqlParameter tableNameParameter = new MySqlParameter("@tableName_", MySqlDbType.VarChar, 30);
                tableNameParameter.Value = "seq_recharge_order";
                tableNameParameter.Direction = ParameterDirection.Input;
                parameters.Add(tableNameParameter);
                MySqlParameter result = new MySqlParameter("@result", MySqlDbType.Int32);
                result.Direction = ParameterDirection.Output;
                parameters.Add(result);

                DataTable dt = mysql.ExcProcedure("getSeq", parameters.ToArray());
                if ((result.Value) != null && (Convert.ToInt32(result.Value) == 1))
                {
                    //dto.MemberId = Convert.ToDouble(dt.Rows[0]["memberID_"] == null ?"0": dt.Rows[0]["memberID_"].ToString());
                    return Convert.ToInt64(dt.Rows[0]["id"] == null ? "0" : dt.Rows[0]["id"].ToString());
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("GetSeqRechargeOrder error:",ex);
            }
            finally
            {
                this.releaseConnection(mysql);
            }
            return 0;
        }

        public void updateRechargeOnlineId(Int64 rechargeOnlineId)
        {
            string sql = "update seq_recharge_order set id=" + rechargeOnlineId + " where id < " + rechargeOnlineId;
            this.execute(sql);
        }
    }
}
