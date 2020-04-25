using CashierLibrary.Model.Bill;
using CashierLibrary.Util;
using CashierServer.Model;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CashierServer.DAO
{
    class DutyDao : BaseDao
    {

        public IDictionary<string, object> QueryDutyCheckInfo(int gid)
        {
            IDictionary<string, object> root = new Dictionary<string, object>();
            CDbMysql mysql = getConnection();
            try
            {
                List<MySqlParameter> parameters = new List<MySqlParameter>();

                MySqlParameter gidParameter = new MySqlParameter("@gid_", MySqlDbType.Int32);
                gidParameter.Value = gid;
                gidParameter.Direction = ParameterDirection.Input;
                parameters.Add(gidParameter);

                MySqlParameter result = new MySqlParameter("@result", MySqlDbType.Int32);
                result.Value = -1;
                result.Direction = ParameterDirection.Output;
                parameters.Add(result);

                DataTable dt = mysql.ExcProcedure("generateDutyData", parameters.ToArray());
                if (result.Value != null && Convert.ToInt32(result.Value) == 1)
                {
                    DataTableCollection tales = dt.DataSet.Tables;

                    if (tales.Count < 6)
                    {
                        LogHelper.WriteLog("call generateDutyData error:\n获取到数据长度" + tales.Count);

                        root["state"] = 1002;
                        root["info"] = ErrDefine.DUTY_RESP_DB_QUERY_INFO_ERROR;
                        return root;
                    }

                    IDictionary<string, object> data = new Dictionary<string, object>();

                    // 整理数据
                    FormatDutyInfoJson(gid, tales, ref data);

                    root["state"] = 0;
                    root["info"] = "成功";
                    root["data"] = data;
                    return root;
                }
                else
                {
                    LogHelper.WriteLog("预交班失败,错误码:\n" + Convert.ToInt32(null == result.Value ? "-1" : result.Value));

                    root["state"] = 1002;
                    root["info"] = ErrDefine.DUTY_RESP_DB_QUERY_ERROR;
                    return root;
                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("预交班失败:\n" , ex);
            }
            finally
            {
                this.releaseConnection(mysql);
            }

            root["state"] = -1;
            return root;
        }

        public void updateDutyId(Int64 onlineId)
        {
            string sql = "update seq_duty set id=" + onlineId + " where id < " + onlineId;
            this.execute(sql);
        }

        public IDictionary<string, object> UpdateDutyCheckInfo(JObject data, int gid)
        {
            IDictionary<string, object> root = new Dictionary<string, object>();

            try
            {
                List<MySqlParameter> parameters = new List<MySqlParameter>();

                bool bSucc = PrepareSQLForUpdateDuty(data, gid, ref parameters);
                if (!bSucc)
                {
                    root["state"] = 1004;
                    root["info"] = ErrDefine.DUTY_RESP_PARA_ERROR;
                    return root;
                }

                DutyDataSave(parameters, ref root);

                return root;

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("交班失败:\n", ex);
            }

            root["state"] = -1;
            return root;
        }

        private bool PrepareSQLForUpdateDuty(JObject data, int gid, ref List<MySqlParameter> parameters)
        {

            try
            {

                IDictionary<string, object> balanceInfo = new Dictionary<string, object>();
                IDictionary<string, object> goodsInfo = new Dictionary<string, object>();

                //balanceInfo = (IDictionary<string, object>)(object)data["balanceInfo"];
                //goodsInfo = (IDictionary<string, object>)(object)data["goodsInfo"];

                balanceInfo = JsonUtil.ToDictionary(data["balanceInfo"]);
                goodsInfo = JsonUtil.ToDictionary(data["goodsInfo"]);

                int nDutyID = CreateDutyID();

                int nCurrentShiftID = Convert.ToInt32(balanceInfo["shiftID"]);
                int nNextShiftID = 0;
                int nCurrentNetBarUserID = Convert.ToInt32(balanceInfo["currentNetBarUserID"]);
                int nNextNetBarUserID = Convert.ToInt32(balanceInfo["nextNetBarUserID"]);

                string strnextNetBarUserPhone = balanceInfo["nextNetBarUserPhone"].ToString();
                string strDutyBeginTime = balanceInfo["dutyBeginTime"].ToString();
                string strDutyEndTime = balanceInfo["dutyEndTime"].ToString();
                string strDutyDate = balanceInfo["dutyDate"].ToString();


                string strCupList = "", strProblemList = "", strIncomeDetail = "", strPayWayDetail = "";

                //第一次使用交接班strDutyBeginTime为空
                if (string.IsNullOrEmpty(strDutyBeginTime))
                {
                    strDutyBeginTime = "1970-01-01 00:00:00";
                }

                ConvertCupListJsonToString(goodsInfo, ref strCupList);
                GetIncomeDetailStringFromJson(balanceInfo, ref strIncomeDetail);
                GetPayWayDetailStringFromJson(balanceInfo, ref strPayWayDetail);
                ConvertstrProblemListJsonToString(balanceInfo, ref strProblemList);
                
                IDictionary<string, object> map = new Dictionary<string, object>();
                map["cupScrapArray_"] = strCupList;
                map["dutyDate_"] = strDutyDate;
                map["beginDate_"] = strDutyBeginTime;
                map["endDate_"] = strDutyEndTime;
                map["revenueArray_"] = strIncomeDetail;
                map["payWayArray_"] = strPayWayDetail;
                map["problemArray_"] = strProblemList;
                map["currentnetBarUserID_"] = nCurrentNetBarUserID;
                map["nextnetBarUserID_"] = nNextNetBarUserID;
                map["shiftID_"] = nCurrentShiftID;
                map["nextShiftID_"] = nNextShiftID;
                map["dutyID_"] = nDutyID;
                map["gid_"] = gid;

                InitParamsList(map, ref parameters);

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("准备更新交班失败:\n", ex);
            }

            return false;
        }

        private void InitParamsList(IDictionary<string, object> map, ref List<MySqlParameter> parameters)
        {
            MySqlParameter cupScrapArrayParameter = new MySqlParameter("@cupScrapArray_", MySqlDbType.VarChar, 1000);
            cupScrapArrayParameter.Value = map["cupScrapArray_"];
            cupScrapArrayParameter.Direction = ParameterDirection.Input;
            parameters.Add(cupScrapArrayParameter);

            MySqlParameter dutyDateParameter = new MySqlParameter("@dutyDate_", MySqlDbType.DateTime);
            dutyDateParameter.Value = map["dutyDate_"];
            dutyDateParameter.Direction = ParameterDirection.Input;
            parameters.Add(dutyDateParameter);

            MySqlParameter beginDateParameter = new MySqlParameter("@beginDate_", MySqlDbType.DateTime);
            beginDateParameter.Value = map["beginDate_"];
            beginDateParameter.Direction = ParameterDirection.Input;
            parameters.Add(beginDateParameter);

            MySqlParameter endDateParameter = new MySqlParameter("@endDate_", MySqlDbType.DateTime);
            endDateParameter.Value = map["endDate_"];
            endDateParameter.Direction = ParameterDirection.Input;
            parameters.Add(endDateParameter);

            MySqlParameter revenueArrayParameter = new MySqlParameter("@revenueArray_", MySqlDbType.VarChar, 1000);
            revenueArrayParameter.Value = map["revenueArray_"];
            revenueArrayParameter.Direction = ParameterDirection.Input;
            parameters.Add(revenueArrayParameter);

            MySqlParameter payWayArrayParameter = new MySqlParameter("@payWayArray_", MySqlDbType.VarChar, 1000);
            payWayArrayParameter.Value = map["payWayArray_"];
            payWayArrayParameter.Direction = ParameterDirection.Input;
            parameters.Add(payWayArrayParameter);

            MySqlParameter problemArrayParameter = new MySqlParameter("@problemArray_", MySqlDbType.VarChar, 1000);
            problemArrayParameter.Value = map["problemArray_"];
            problemArrayParameter.Direction = ParameterDirection.Input;
            parameters.Add(problemArrayParameter);

            MySqlParameter currentnetBarUserIDParameter = new MySqlParameter("@currentnetBarUserID_", MySqlDbType.Int32);
            currentnetBarUserIDParameter.Value = map["currentnetBarUserID_"];
            currentnetBarUserIDParameter.Direction = ParameterDirection.Input;
            parameters.Add(currentnetBarUserIDParameter);

            MySqlParameter nextnetBarUserIDParameter = new MySqlParameter("@nextnetBarUserID_", MySqlDbType.Int32);
            nextnetBarUserIDParameter.Value = map["nextnetBarUserID_"];
            nextnetBarUserIDParameter.Direction = ParameterDirection.Input;
            parameters.Add(nextnetBarUserIDParameter);

            MySqlParameter shiftIDParameter = new MySqlParameter("@shiftID_", MySqlDbType.Int32);
            shiftIDParameter.Value = map["shiftID_"];
            shiftIDParameter.Direction = ParameterDirection.Input;
            parameters.Add(shiftIDParameter);

            MySqlParameter nextShiftIDParameter = new MySqlParameter("@nextShiftID_", MySqlDbType.Int32);
            nextShiftIDParameter.Value = map["nextShiftID_"];
            nextShiftIDParameter.Direction = ParameterDirection.Input;
            parameters.Add(nextShiftIDParameter);

            MySqlParameter dutyIDParameter = new MySqlParameter("@dutyID_", MySqlDbType.Int32);
            dutyIDParameter.Value = map["dutyID_"];
            dutyIDParameter.Direction = ParameterDirection.Input;
            parameters.Add(dutyIDParameter);

            MySqlParameter gidParameter = new MySqlParameter("@gid_", MySqlDbType.Int32);
            gidParameter.Value = map["gid_"];
            gidParameter.Direction = ParameterDirection.Input;
            parameters.Add(gidParameter); 

        }

        public void DutyDataSave(List<MySqlParameter> parameters, ref IDictionary<string, object> root)
        {

            CDbMysql mysql = getConnection();

            try
            {

                MySqlParameter result = new MySqlParameter("@result", MySqlDbType.Int32);
                result.Value = -1;
                result.Direction = ParameterDirection.Output;
                parameters.Add(result);

                //DataTable dt = mysql.ExcProcedure("dutyDataSave", parameters.ToArray());
                DataTable dt = mysql.ExcProcedure("dutyDataSaveNew", parameters.ToArray()); 
                if (result.Value != null)
                {
                    if (Convert.ToInt32(result.Value) == 1)
                    {

                        root["state"] = 0;
                        root["info"] = "成功";
                        return;
                    }

                    if (Convert.ToInt32(result.Value) == 10635)
                    {
                        root["state"] = 10635;
                        root["info"] = ErrDefine.DUTY_RESP_DB_UPDATE_REPEAT_ERROR;
                        return;
                    }
                    else
                    {
                        LogHelper.WriteLog("call getDutyDate error:\n获取到数据长度" + dt.Rows.Count);

                        root["state"] = 1003;
                        root["info"] = ErrDefine.DUTY_RESP_DB_UPDATE_ERROR;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("DutyDataSave error:", ex);
            }
            finally
            {
                this.releaseConnection(mysql);
            }
        }

        private void ConvertCupListJsonToString(IDictionary<string, object> goodsInfo, ref string strCupList)
        {

            if (!goodsInfo.ContainsKey("cupList"))
            {
                return ;
            }

            JArray cupArr = JArray.FromObject(goodsInfo["cupList"]);
            List<IDictionary<string, object>> cupList = cupArr.ToObject<List<IDictionary<string, object>>>();
            int uCount = cupList.Count;

            if (uCount == 0)
            {
                return;
            }

            IDictionary<string, object> element;
            List<string> ssCupList = new List<string>();

            for (int i = 0; i < uCount; i++)
            {
                element = cupList[i];
                if (element == null)
                {
                    continue;
                }

                int nCupID = Convert.ToInt32(element["cupID"]);
                int nTheNum = Convert.ToInt32(element["theNum"]);  //报废数量

                ssCupList.Add(nCupID + ":" + nTheNum);
            }

            if (ssCupList.Count == 0)
            {
                strCupList = "";
            }
            else
            {
                strCupList = string.Join(",", ssCupList.ToArray());
            }
        }

        private void ConvertstrProblemListJsonToString(IDictionary<string, object> balanceInfo, ref string strProblemList)
        {
            if (!balanceInfo.ContainsKey("problemList"))
            {
                return;
            }

            JArray problemArr = JArray.FromObject(balanceInfo["problemList"]);
            List<IDictionary<string, object>> problemList = problemArr.ToObject<List<IDictionary<string, object>>>();

            int uCount = problemList.Count;

            if (uCount == 0)
            {
                return ;
            }

            IDictionary<string, object> element;
            List<string> ssProblemList = new List<string>();

            string strCurrentTime;
            strCurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            for (int i = 0; i < uCount; i++)
            {
                element = problemList[i];
                if (element == null)
                {
                    continue ;
                }

                int nProblemID = Convert.ToInt32(element["problemID"]);
                int nState = Convert.ToInt32(element["state"]);

                string strDescription = element["description"].ToString();
                int nCreatorID = Convert.ToInt32(element["creatorID"]); 
                int nActorID = Convert.ToInt32(element["actorID"]); 
                string strRemark = element["remark"].ToString();
                string strCreateTime = "";
                string strUpdateTime = "";
                if (nProblemID == -1)  //新增的待办事项 才填写创建时间
                {
                    strCreateTime = strCurrentTime;
                }
                else if (nState == 1)  //处理过才填写处理时间
                {
                    strUpdateTime = strCurrentTime;
                }

                ssProblemList.Add(nProblemID + ":" + strDescription + ":" + nState + ":" + nCreatorID
                             + ":" + nActorID + ":" + strRemark);
            }

            if (ssProblemList.Count == 0)
            {
                strProblemList = "";
            }
            else
            {
                strProblemList =  string.Join(",", ssProblemList.ToArray());
            }
        }

        private void GetPayWayDetailStringFromJson(IDictionary<string, object> balanceInfo, ref string strPayWayDetail)
        {
            //现金，银联，支付宝，微信，卡扣，优惠券抵扣
            decimal dCurrentCash = Convert.ToDecimal(balanceInfo["currentCash"]);
            decimal dUnionPay = Convert.ToDecimal(balanceInfo["unionPay"]);
            decimal dAlipay = Convert.ToDecimal(balanceInfo["alipay"]);
            decimal dWeChat = Convert.ToDecimal(balanceInfo["weChat"]);
            decimal dCardAccount = Convert.ToDecimal(balanceInfo["cardAccount"]);
            decimal dCouponDeductionSum = Convert.ToDecimal(balanceInfo["couponDeductionSum"]);

            string ssPayWayDetail;
            ssPayWayDetail = dCurrentCash + ":" + dUnionPay + ":" + dAlipay + ":" + dWeChat + ":" + dCardAccount + ":" + dCouponDeductionSum;
            strPayWayDetail = ssPayWayDetail.ToString();
        }

        public void GetIncomeDetailStringFromJson(IDictionary<string, object> balanceInfo, ref string strIncomeDetail)
        {
            //本班总现金，上班预留，本班预留，会员充值，临上结算，临上押金，商品，水吧，其他收入，其他支出，商品支出，网费赠送支出

            decimal dCurrentCash = Convert.ToDecimal(balanceInfo["currentCash"]);
            decimal dLastReserve = Convert.ToDecimal(balanceInfo["lastReserve"]); 
            decimal dCurrentReserve = Convert.ToDecimal(balanceInfo["currentReserve"]); 
            decimal dMemberCharge = Convert.ToDecimal(balanceInfo["memberCharge"]); 
            decimal dTempFinishMoney = Convert.ToDecimal(balanceInfo["tempFinishMoney"]); 
            decimal dTempDepositMoney = Convert.ToDecimal(balanceInfo["tempDepositMoney"]); 
            decimal dGoodsSaleMoney = Convert.ToDecimal(balanceInfo["goodsSaleMoney"]); 
            decimal dDrinkSaleMoney = Convert.ToDecimal(balanceInfo["drinkSaleMoney"]); 
            decimal dExtraIncome = Convert.ToDecimal(balanceInfo["extraIncome"]);
            decimal dExtraExpend = Convert.ToDecimal(balanceInfo["extraExpend"]);
            decimal dGoodsOutFee = Convert.ToDecimal(balanceInfo["goodsOutFee"]); 
            decimal dNetOutFee = Convert.ToDecimal(balanceInfo["netOutFee"]); 

            string ssIncomeDetail;
            ssIncomeDetail = dCurrentCash + ":" + dLastReserve + ":" + dCurrentReserve + ":" + dMemberCharge
                 + ":" + dTempFinishMoney + ":" + dTempDepositMoney + ":" + dGoodsSaleMoney + ":" + dDrinkSaleMoney
                 + ":" + dExtraIncome + ":" + dExtraExpend + ":" + dGoodsOutFee + ":" + dNetOutFee;

            strIncomeDetail = ssIncomeDetail.ToString();
        }

        private int CreateDutyID()
        {
            CDbMysql mysql = getConnection();
            try
            {
                List<MySqlParameter> parameters = new List<MySqlParameter>();

                MySqlParameter tableNameParameter = new MySqlParameter("@tableName_", MySqlDbType.VarChar, 30);
                tableNameParameter.Value = "seq_duty";
                tableNameParameter.Direction = ParameterDirection.Input;
                parameters.Add(tableNameParameter);

                MySqlParameter result = new MySqlParameter("@result", MySqlDbType.Int32);
                result.Direction = ParameterDirection.Output;
                parameters.Add(result);

                DataTable dt = mysql.ExcProcedure("getSeq", parameters.ToArray());
                if ((result.Value) != null && (Convert.ToInt32(result.Value) == 1))
                {
                    return Convert.ToInt32(dt.Rows[0]["id"] == null ? "0" : dt.Rows[0]["id"].ToString());
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("create duty id:", ex);
            }
            finally
            {
                this.releaseConnection(mysql);
            }
            return 0;
        }

        private void FormatDutyInfoJson(int gid, DataTableCollection tables, ref IDictionary<string, object> data)
        {
            IDictionary<string, object> balanceInfo = new Dictionary<string, object>();
            IDictionary<string, object> goodsInfo = new Dictionary<string, object>();

            DataTable goodsInfoSet = tables[0];
            DataTable cupInfoSet = tables[1];
            DataTable cupLinkGoodsSet = tables[2];
            DataTable extraExpendSet = tables[3];
            DataTable problemListSet = tables[4];
            DataTable balanceInfoSet = tables[5];

            FormatGoodsInfoJson(goodsInfoSet, cupInfoSet, cupLinkGoodsSet, ref goodsInfo);
            FormatBalanceInfoJson(gid, balanceInfoSet, problemListSet, extraExpendSet, ref balanceInfo);

            data["balanceInfo"] = balanceInfo;
            data["goodsInfo"] = goodsInfo;
        }

        /// <summary>
        /// 资金信息
        /// </summary>
        /// <param name="balanceInfoSet"></param>
        /// <param name="problemListSet"></param>
        /// <param name="extraExpendSet"></param>
        /// <param name="balanceInfo"></param>
        private void FormatBalanceInfoJson(int gid, DataTable balanceInfoSet, DataTable problemListSet, DataTable extraExpendSet, ref IDictionary<string, object> balanceInfo)
        {
            List<IDictionary<string, object>> extraExpendDetail = new List<IDictionary<string, object>>();
            List<IDictionary<string, object>> problemList = new List<IDictionary<string, object>>();

            DataRow info = balanceInfoSet.Rows[0];

            string strBeginTime = info["beginDate"].ToString();
            if (string.IsNullOrEmpty(strBeginTime))
            {
                strBeginTime = "";  //第一次交接班,开始时间为空
            }
            else
            {
                strBeginTime = DateUtil.dtToFormat((DateTime)info["beginDate"]);
            }

            balanceInfo["dutyBeginTime"] = strBeginTime;
            balanceInfo["dutyEndTime"] = DateUtil.dtToFormat((DateTime)info["endTime"]);
            decimal dMemberCharge = Convert.ToDecimal(info["memberCharge"]);
            decimal dTempFinishMoney = Convert.ToDecimal(info["tmpFee"]);
            decimal dTempDepositMoney = Convert.ToDecimal(info["tmpDeposit"]);
            decimal dGoodsSaleMoney = Convert.ToDecimal(info["goodsFee"]);
            decimal dDrinkSaleMoney = Convert.ToDecimal(info["waterBarFee"]);
            decimal dExtraIncome = Convert.ToDecimal(info["otherIn"]);
            decimal dExtraExpend = Convert.ToDecimal(info["otherOut"]);
            balanceInfo["currentCash"] = Convert.ToDecimal(info["cashFee"]); 
            balanceInfo["unionPay"] = Convert.ToDecimal(info["bankFee"]);
            balanceInfo["alipay"] = Convert.ToDecimal(info["alipayFee"]);
            balanceInfo["weChat"] = Convert.ToDecimal(info["weixinFee"]);
            balanceInfo["cardAccount"] = Convert.ToDecimal(info["acount_"]);
            balanceInfo["couponDeductionSum"] = Convert.ToDecimal(info["couponDeductionSUM_"]);
            balanceInfo["cashReserveSum"] = Convert.ToDecimal(info["cashReserveSUM"]);
            decimal dLastReserve = Convert.ToDecimal(info["lastReserve_"]);
            balanceInfo["reserveCash"] = Convert.ToDecimal(info["rerservCash"]);
            balanceInfo["goodsOutFee"] = Convert.ToDecimal(info["goodsOutFee"]);
            balanceInfo["netOutFee"] = Convert.ToDecimal(info["netOutFee"]);

            balanceInfo["lastReserve"] = dLastReserve;
            balanceInfo["memberCharge"] = dMemberCharge;
            balanceInfo["tempFinishMoney"] = dTempFinishMoney;
            balanceInfo["tempDepositMoney"] = dTempDepositMoney;
            balanceInfo["goodsSaleMoney"] = dGoodsSaleMoney;
            balanceInfo["drinkSaleMoney"] = dDrinkSaleMoney;
            balanceInfo["extraIncome"] = dExtraIncome;
            balanceInfo["extraExpend"] = dExtraExpend;

            balanceInfo["currentSum"] = dMemberCharge + dTempFinishMoney + dTempDepositMoney
                                            + dGoodsSaleMoney + dDrinkSaleMoney + dExtraIncome - dExtraExpend; //本班总金额

            FormatProblemListJsonForChangeDuty(problemListSet, ref problemList);
            FormatExtraExpendJson(extraExpendSet, ref extraExpendDetail);

            balanceInfo["extraExpendDetail"] = extraExpendDetail;
            balanceInfo["problemList"] = problemList;

            string strDutyDate = "";
            GetDutyDate(gid, ref strDutyDate);
            balanceInfo["dutyDate"] = strDutyDate;
        }

        private void GetDutyDate(int gid, ref string strDutyDate)
        {
            CDbMysql mysql = getConnection();
            try
            {
                List<MySqlParameter> parameters = new List<MySqlParameter>();

                MySqlParameter gidParameter = new MySqlParameter("@gid_", MySqlDbType.Int32);
                gidParameter.Value = gid;
                gidParameter.Direction = ParameterDirection.Input;
                parameters.Add(gidParameter);

                MySqlParameter result = new MySqlParameter("@result", MySqlDbType.Int32);
                result.Value = -1;
                result.Direction = ParameterDirection.Output;
                parameters.Add(result);

                DataTable dt = mysql.ExcProcedure("getDutyDate", parameters.ToArray());
                if (result.Value != null && Convert.ToInt32(result.Value) == 1)
                {

                    if (dt.Rows.Count < 1)
                    {
                        LogHelper.WriteLog("call getDutyDate error:\n获取到数据长度" + dt.Rows.Count);
                        return;
                    }

                    strDutyDate = dt.Rows[0]["theDutyDate_"] == null ? "" : dt.Rows[0]["theDutyDate_"].ToString();
                }
                else
                {
                    LogHelper.WriteLog("call getDutyDate error ,错误码:\n" + Convert.ToInt32(null == result.Value ? "-1" : result.Value));
                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("call getDutyDate error :\n", ex);
            }
            finally
            {
                this.releaseConnection(mysql);
            }
        }

        /// <summary>
        /// 额外支出明细
        /// </summary>
        /// <param name="extraExpendSet"></param>
        /// <param name="extraExpendDetail"></param>
        private void FormatExtraExpendJson(DataTable extraExpendSet, ref List<IDictionary<string, object>> extraExpendDetail)
        {
            int size = extraExpendSet.Rows.Count;

            for (int i = 0; i < size; ++i)
            {
                IDictionary<string, object> extraElement = new Dictionary<string, object>();

                DataRow row = extraExpendSet.Rows[i];

                extraElement["fee"] = Convert.ToDecimal(row["fee"]);
                extraElement["extraFeeCategory"] = Convert.ToInt64(row["extra"]);
                extraElement["reason"] = row["reason"].ToString();

                extraExpendDetail.Add(extraElement);
            }
        }

        /// <summary>
        /// 待处理事项，查询交接班数据时返回
        /// </summary>
        /// <param name="problemListSet"></param>
        /// <param name="problemList"></param>
        private void FormatProblemListJsonForChangeDuty(DataTable problemListSet, ref List<IDictionary<string, object>> problemList)
        {

            int size = problemListSet.Rows.Count;

            for (int i = 0; i < size; ++i)
            {
                IDictionary<string, object> problemElement = new Dictionary<string, object>();

                DataRow row = problemListSet.Rows[i];

                problemElement["problemID"] = Convert.ToInt64(row["problemID"]);
                problemElement["description"] = row["description"].ToString();

                problemList.Add(problemElement);
            }

        }

        /// <summary>
        /// 商品、杯型信息
        /// </summary>
        /// <param name="goodsInfoSet"></param>
        /// <param name="cupInfoSet"></param>
        /// <param name="cupLinkGoodsSet"></param>
        /// <param name="goodsInfo"></param>
        private void FormatGoodsInfoJson(DataTable goodsInfoSet, DataTable cupInfoSet, DataTable cupLinkGoodsSet, ref IDictionary<string, object> goodsInfo)
        {
            List<IDictionary<string, object>> goodsList = new List<IDictionary<string, object>>();
            List<IDictionary<string, object>> cupList = new List<IDictionary<string, object>>();

            FormatGoodsListJson(goodsInfoSet, ref goodsList);
            FormatCupListJson(cupInfoSet, cupLinkGoodsSet, ref cupList);

            goodsInfo["goodsList"] = goodsList;
            goodsInfo["cupList"] = cupList;

        }

        private void FormatCupListJson(DataTable cupInfoSet, DataTable cupLinkGoodsSet, ref List<IDictionary<string, object>> cupList)
        {
            int size = cupInfoSet.Rows.Count;
            for (int i = 0; i < size; i++)
            {
                IDictionary<string, object> cupElement = new Dictionary<string, object>();

                DataRow row = cupInfoSet.Rows[i];

                int nCupID = Convert.ToInt32(row["cupID"]);
                cupElement["cupID"] = nCupID;
                cupElement["cupName"] = row["cupName"].ToString();
                cupElement["totalNum"] = Convert.ToInt64(row["SUMNum"]);
                cupElement["saleNum"] = Convert.ToInt64(row["saleNum"]);
                long nRemainNum = Convert.ToInt64(row["storeNum"]);
                cupElement["shouldNum"] = nRemainNum;     //应剩余数量
                cupElement["factNum"] = nRemainNum;       //实际剩余数量
                cupElement["theNum"] = 0;                 //报废数量
                cupElement["linkGoodsInfo"] = (Array)null;

                int len = cupLinkGoodsSet.Rows.Count;
                for (int j = 0; j < len; j++)
                {
                    DataRow lRow = cupLinkGoodsSet.Rows[j];
                    int nLinkCupID = Convert.ToInt32(lRow["cupID"]);
                    if (nLinkCupID != nCupID)
                    {
                        continue;
                    }

                    IDictionary<string, object> linkElement = new Dictionary<string, object>();

                    linkElement["goodsID"] = Convert.ToInt64(lRow["netbarGoodsID"]);
                    linkElement["goodsName"] = lRow["goodsName"].ToString();
                    linkElement["goodsNum"] = Convert.ToInt64(lRow["saleNum"]);

                    cupElement["linkGoodsInfo"] = linkElement;
                }

                cupList.Add(cupElement);
            }

        }

        private void FormatGoodsListJson(DataTable goodsInfoSet, ref List<IDictionary<string, object>> goodsList)
        {
            int size = goodsInfoSet.Rows.Count;
            for (int i = 0; i < size; i++)
            {
                IDictionary<string, object> goodsElement = new Dictionary<string, object>();

                DataRow row = goodsInfoSet.Rows[i];

                goodsElement["goodsID"] = Convert.ToInt64(row["netbarGoodsID"]);
                goodsElement["goodsName"] = row["goodsName"].ToString();
                goodsElement["totalNum"] = Convert.ToInt64(row["SUMNum"]);
                goodsElement["saleNum"] = Convert.ToInt64(row["saleNum"]);
                goodsElement["rightRemainNum"] = Convert.ToInt64(row["storeNum"]);
                goodsElement["presentNum"] = Convert.ToInt64(row["presentNum"]);

                goodsList.Add(goodsElement);
            }
        }
    }



}
