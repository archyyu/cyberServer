using CashierLibrary.Util;
using CashierServer.Model;
using CashierServer.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CashierServer.Logic
{

    /// <summary>
    /// 新同步类
    /// </summary>
    class SyncLogic : BaseLogic
    {

        private DateTime updateDateTime = System.DateTime.Now;

        public void start()
        {
            // 会员同步线程
            new Thread(this.syncMemberTick).Start();
            LogHelper.WriteLog("会员同步线程开启");
            // 订单同步线程
            new Thread(this.syncOrderTick).Start();
            LogHelper.WriteLog("订单同步线程开启");
            // 上机同步线程
            new Thread(this.syncOnlineTick).Start();
            LogHelper.WriteLog("上机同步线程开启");

            new Thread(this.syncOmitMemberTick).Start();
            LogHelper.WriteLog("遗漏会员同步线程开启");
        }

        #region 同步线程

        /// <summary>
        /// 同步会员线程
        /// </summary>
        public override void syncMemberTick()
        {
            object obj = new object();
            Monitor.Enter(obj);

            while (true)
            {
                Monitor.Wait(obj, 5000, false);

                List<IDictionary<string, Object>> list = new List<IDictionary<string, Object>>();
                bool err = true;
                // 初始化数据库对象
                CDbMysql syncDB = this.getConnection();
                try
                {
                    IDictionary<String, Object> requestBody = new Dictionary<String, Object>();

                    IDictionary<string, string> checkParams = HttpUtil.initParams();
                    foreach (KeyValuePair<string, string> kv in checkParams)
                    {
                        requestBody.Add(kv.Key, kv.Value);
                    }

                    // 获取最后同步时间
                    this.updateDateTime = this.readLastUpdateTime(syncDB);
                    // 搜集信息
                    List<IDictionary<string, Object>> memberList = this.syncMember(updateDateTime.ToString("yyyy-MM-dd HH:mm:ss"), syncDB);
                    requestBody.Add("memberList", memberList);
                    List<IDictionary<String, Object>> memberAccountList = this.syncMemberAccount(updateDateTime.ToString("yyyy-MM-dd HH:mm:ss"), syncDB);
                    requestBody.Add("memberAccountList", memberAccountList);

                    if ((memberList == null || memberList.Count == 0) && (memberAccountList == null || memberAccountList.Count == 0))
                    {
                        // LogHelper.WriteLog("无数据上传");
                    }
                    else
                    {
                        long starttime = DateUtil.ConvertDateTimeToInt(DateTime.Now);

                        if (memberList != null && memberList.Count > 0)
                        {
                            list = memberList;
                        }

                        String body = JsonUtil.SerializeObject(requestBody);
                        String response = HttpUtil.doPost(IniUtil.syncMemberUrl(), body);
                        JObject result = JObject.Parse(response);
                        err = false;
                        long endtime = DateUtil.ConvertDateTimeToInt(DateTime.Now);

                        {
                            // 记录会员同步
                            int memberCount = (memberList == null) ? 0 : memberList.Count;
                            int accountCount = (memberAccountList == null) ? 0 : memberAccountList.Count;
                            long time = endtime - starttime;
                            LogHelper.WriteLog("本次同步数据长度:会员" + memberCount + "账户" + accountCount + "时长" + time);
                        }

                        if (Int32.Parse(result["code"].ToString()) == 0)
                        {
                            // 执行错误处理
                            this.processMemberIDResponse(result["data"]["memberList"].ToString(), syncDB);
                        }
                        else
                        {
                            LogHelper.WriteLog("sync error, message:" + result["message"].ToString());
                        }
                    }
                }

                catch (Exception ex)
                {
                    LogHelper.WriteLog("会员信息同步错误，错误信息为:", ex);
                    if (err)
                    {
                        // web错误处理
                        if (list != null && list.Count > 0)
                        {
                            this.errWebResponse(list, syncDB);
                        }
                    }
                }
                finally
                {
                    this.updateMemberSyncTime(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), syncDB);
                    this.releaseConnection(syncDB);
                }
            }
        }

        /// <summary>
        /// 同步订单线程
        /// </summary>
        public override void syncOrderTick()
        {
            object obj = new object();
            Monitor.Enter(obj);

            while (true)
            {
                Monitor.Wait(obj, 5000, false);
                // 初始化数据库对象
                CDbMysql syncDB = this.getConnection();

                try
                {
                    IDictionary<String, Object> requestBody = new Dictionary<String, Object>();

                    IDictionary<string, string> checkParams = HttpUtil.initParams();
                    foreach (KeyValuePair<string, string> kv in checkParams)
                    {
                        requestBody.Add(kv.Key, kv.Value);
                    }

                    // 搜集数据
                    List<IDictionary<String, Object>> orderList = this.syncRechargeOrder(syncDB);
                    requestBody.Add("rechargeOrderList", orderList);

                    List<IDictionary<String, Object>> goodsOrderList = this.syncGoodsOrderList(syncDB);
                    requestBody.Add("goodsOrderList", goodsOrderList);

                    if ((orderList == null || orderList.Count == 0) && (goodsOrderList == null || goodsOrderList.Count == 0))
                    {
                        // LogHelper.WriteLog("无数据上传");
                    }
                    else
                    {
                        String body = JsonUtil.SerializeObject(requestBody);
                        String response = HttpUtil.doPost(IniUtil.syncOrderUrl(), body);
                        JObject result = JObject.Parse(response);
                        if (Int32.Parse(result["code"].ToString()) == 0)
                        {
                            this.processRechargeResponse(result["data"]["rechargeOrderList"].ToString(), syncDB);
                            this.processGoodsOrderList(result["data"]["syncGoodsOrderList"].ToString(), syncDB);
                        }
                        else
                        {
                            LogHelper.WriteLog("sync error, message:" + result["message"].ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("订单信息同步错误，错误信息为:", ex);
                }
                finally
                {
                    this.releaseConnection(syncDB);
                }
            }
        }

        /// <summary>
        /// 同步上机线程
        /// </summary>
        public override void syncOnlineTick()
        {
            object obj = new object();
            Monitor.Enter(obj);
            int tick = 0;
            while (true)
            {
                Monitor.Wait(obj, 5000, false);
                // 初始化数据库对象
                CDbMysql syncDB = this.getConnection();

                try
                {
                    tick++;
                    if (tick % 12 == 0)
                    {
                        // 在线会员
                        this.uploadOnlineInfo(syncDB);
                        continue;
                    }

                    IDictionary<String, Object> requestBody = new Dictionary<String, Object>();

                    IDictionary<string, string> checkParams = HttpUtil.initParams();
                    foreach (KeyValuePair<string, string> kv in checkParams)
                    {
                        requestBody.Add(kv.Key, kv.Value);
                    }

                    // 搜集数据
                    List<IDictionary<string, Object>> onlineList = this.syncOnline(syncDB);
                    requestBody.Add("onlineList", onlineList);

                    List<IDictionary<String, Object>> dutyList = this.syncDuty(syncDB);
                    requestBody.Add("dutyList", dutyList);

                    if ((onlineList == null || onlineList.Count == 0) && (dutyList == null || dutyList.Count == 0))
                    {
                        // LogHelper.WriteLog("无数据上传");
                    }
                    else
                    {
                        String body = JsonUtil.SerializeObject(requestBody);
                        String response = HttpUtil.doPost(IniUtil.syncOnlineUrl(), body);
                        JObject result = JObject.Parse(response);
                        if (Int32.Parse(result["code"].ToString()) == 0)
                        {
                            this.processOnlineListReponse(result["data"]["syncOnlineList"].ToString(), syncDB);
                            this.processDutyListResponse(result["data"]["syncDutyList"].ToString(), syncDB);
                        }
                        else
                        {
                            LogHelper.WriteLog("sync error, message:" + result["message"].ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("订单信息同步错误，错误信息为:", ex);
                }
                finally
                {
                    this.releaseConnection(syncDB);
                }
            }
        }

        /// <summary>
        /// 同步遗漏会员线程
        /// </summary>
        public override void syncOmitMemberTick()
        {
            object obj = new object();
            Monitor.Enter(obj);
            while (true)
            {
                Monitor.Wait(obj, 5000, false);

                // 初始化数据库对象
                CDbMysql syncDB = this.getConnection();
                try
                {
                    IDictionary<String, Object> requestBody = new Dictionary<String, Object>();

                    IDictionary<string, string> checkParams = HttpUtil.initParams();
                    foreach (KeyValuePair<string, string> kv in checkParams)
                    {
                        requestBody.Add(kv.Key, kv.Value);
                    }

                    // 搜集信息
                    List<IDictionary<string, Object>> omitmemberList = this.syncOmitMember(syncDB);
                    requestBody.Add("omitmemberList", omitmemberList);

                    if (omitmemberList == null || omitmemberList.Count == 0)
                    {
                        // LogHelper.WriteLog("无数据上传");
                    }
                    else
                    {
                        String body = JsonUtil.SerializeObject(requestBody);
                        String response = HttpUtil.doPost(IniUtil.syncOmitMemberUrl(), body);
                        JObject result = JObject.Parse(response);

                        if (Int32.Parse(result["code"].ToString()) == 0)
                        {
                            // 处理omit member
                            this.dealMemberIDResponse(result["data"]["omitmemberList"].ToString(), syncDB);
                        }
                        else
                        {
                            LogHelper.WriteLog("sync error, message:" + result["message"].ToString());
                        }
                    }
                }

                catch (Exception ex)
                {
                    LogHelper.WriteLog("omit member信息同步错误，错误信息为:", ex);
                }
                finally
                {
                    this.releaseConnection(syncDB);
                }

            }
        }

        #endregion

        /// <summary>
        /// 获取最后同步时间
        /// </summary>
        /// <returns>最后同步时间</returns>
        public DateTime readLastUpdateTime(CDbMysql syncDB)
        {
            String readSql = "select id, updateDate from sync_member_update_date where id = 0";

            List<String> columns = new List<String>();
            columns.Add("id");
            columns.Add("updateDate");

            IDictionary<String, Object> resultMap = this.readMap(readSql, columns, syncDB);
            return DateTime.Parse(resultMap["updateDate"].ToString());
        }

        /// <summary>
        /// 上传在线信息
        /// </summary>
        private void uploadOnlineInfo(CDbMysql syncDB)
        {
            try
            {
                IDictionary<String, Object> requestBody = new Dictionary<String, Object>();

                List<IDictionary<String, Object>> data = this.getOnlineMember(syncDB);
                IDictionary<string, string> checkParams = HttpUtil.initParams();
                foreach (KeyValuePair<string, string> kv in checkParams)
                {
                    requestBody.Add(kv.Key, kv.Value);
                }
                requestBody.Add("online", data);

                String body = JsonUtil.SerializeObject(requestBody);
                HttpUtil.doPost(IniUtil.updateOnlineUrl(), body);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("uploadOnlineInfo ex", ex);
            }
        }

        /// <summary>
        /// 获取在线用户
        /// </summary>
        /// <returns>上机列表</returns>
        private List<IDictionary<String, Object>> getOnlineMember(CDbMysql syncDB)
        {
            List<string> list = new List<string>();
            list.Add("memberID");
            list.Add("machineName");
            list.Add("areaName");
            list.Add("gid");

            String selectSql = "select memberID,machineName,areaName,gid from netbar_online where state=1";
            return this.readList2(selectSql, list, syncDB);
        }

        #region 同步sql

        /// <summary>
        /// 会员信息同步
        /// </summary>
        /// <param name="date">最后同步时间</param>
        /// <returns>会员信息列表</returns>
        private List<IDictionary<string, Object>> syncMember(string date, CDbMysql syncDB)
        {
            String readSql = "select ";
            foreach (String item in IniUtil.member())
            {
                readSql += item + ",";
            }
            readSql = readSql.Trim(',');
            readSql += " from netbar_member where lastUpdateDate >= '" + date + "'";

            return this.readList2(readSql, IniUtil.member(), syncDB);
        }

        /// <summary>
        /// 会员账户信息同步
        /// </summary>
        /// <param name="date">最后同步时间</param>
        /// <returns>会员账户信息列表</returns>
        private List<IDictionary<string, Object>> syncMemberAccount(string date, CDbMysql syncDB)
        {
            String readSql = "select ";
            foreach (String item in IniUtil.memberAccount())
            {
                readSql += item + ",";
            }
            readSql = readSql.Trim(',');
            readSql += " from netbar_member_account where lastUpdateTime >= '" + date + "'";

            return this.readList2(readSql, IniUtil.memberAccount(), syncDB);
        }

        /// <summary>
        /// 充值订单信息同步
        /// </summary>
        /// <returns></returns>
        private List<IDictionary<String, Object>> syncRechargeOrder(CDbMysql syncDB)
        {
            String readSql = "select ";
            foreach (String item in IniUtil.rechargeOrder())
            {
                readSql += "n." + item + ",";
            }
            readSql = readSql.Trim(',');
            readSql += " from netbar_recharge_order n left join sync_recharge_order s on n.rechargeOrderID=s.rechargeOrderID where s.sync is NULL limit 10";

            return this.readList(readSql, IniUtil.rechargeOrder(), syncDB);
        }

        /// <summary>
        /// 卡扣订单信息同步
        /// </summary>
        /// <returns></returns>
        public List<IDictionary<String, Object>> syncGoodsOrderList(CDbMysql syncDB)
        {
            String readSql = "select ";
            foreach (String item in IniUtil.goodsOrder())
            {
                readSql += "n." + item + ",";
            }
            readSql = readSql.Trim(',');
            readSql += " from netbar_goods_order n left join sync_netbar_goods_order s on n.goodsOrderID=s.goodsOrderID where s.sync is NULL limit 10";

            return this.readList(readSql, IniUtil.goodsOrder(), syncDB);
        }

        /// <summary>
        /// 交班信息同步
        /// </summary>
        /// <returns>交班信息列表</returns>
        private List<IDictionary<String, Object>> syncDuty(CDbMysql syncDB)
        {
            String readSql = "select ";
            foreach (String item in IniUtil.duty())
            {
                readSql += "d." + item + ",";
            }
            readSql = readSql.Trim(',');
            readSql += " from netbar_duty d left join sync_duty s on d.dutyID=s.dutyID where s.sync is NULL limit 10";

            List<IDictionary<String, Object>> list = this.readList(readSql, IniUtil.duty(), syncDB);
            foreach (IDictionary<String, Object> item in list)
            {
                int dutyID = Int32.Parse(item["dutyID"].ToString());

                item.Add("dutyFundsList", this.selectDutyFunds(dutyID, syncDB));
                item.Add("dutyFundsPaytypeList", this.selectDutyFundsPayType(dutyID, syncDB));
                item.Add("dutyConsumeList", this.selectDutyConsumes(dutyID, syncDB));

            }
            return list;
        }

        /// <summary>
        /// 上机信息同步
        /// </summary>
        /// <returns>上机信息列表</returns>
        private List<IDictionary<String, Object>> syncOnline(CDbMysql syncDB)
        {
            String selectSql = "select ";
            foreach (String item in IniUtil.online())
            {
                selectSql += "o." + item + ",";
            }
            selectSql = selectSql.Trim(',');
            selectSql += " from netbar_online o left join sync_online s on o.onlineID=s.onlineID where o.state=2 and s.sync is NULL limit 20";

            return this.readList(selectSql, IniUtil.online(), syncDB);
        }

        private List<IDictionary<String, Object>> syncOmitMember(CDbMysql syncDB)
        {
            String selectSql = @"SELECT
	m.memberID,
	m.account,
	m.uid,
	m.userId,
	m.memberName,
	m.birthday,
	m.sex,
	m.phone,
	m.qq,
	m.openID,
	m.memberType,
	m.state,
	m.creator,
	m.gid,
	m.certificateType,
	m.certificateNum,
	m.address,
	m.password,
	m.createDate,
	a.userrPoint,
	a.totalBaseBalance,
	a.totalAwardBalance,
	a.baseBalance,
	a.awardBalance,
	a.deposit,
	a.lastUpdateTime
FROM sync_member s
LEFT JOIN netbar_member m ON m.memberID = s.memberID
LEFT JOIN netbar_member_account a ON a.memberID = m.memberID
WHERE s.sync = 0 LIMIT 10;";

            return this.readList2(selectSql, IniUtil.omitmember(), syncDB);
        }

        //private List<IDictionary<String, Object>> syncOmitMemberAccount(CDbMysql syncDB)
        //{
        //    String selectSql = "select ";
        //    foreach (String item in IniUtil.memberAccount())
        //    {
        //        selectSql += "a." + item + ",";
        //    }
        //    selectSql = selectSql.Trim(',');
        //    selectSql += " from sync_member s LEFT JOIN netbar_member_account a ON s.memberID = a.memberID WHERE s.sync = 0 LIMIT 10";

        //    return this.readList2(selectSql, IniUtil.memberAccount(), syncDB);
        //}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dutyId"></param>
        /// <returns></returns>
        private List<IDictionary<String, Object>> selectDutyFunds(Int32 dutyId, CDbMysql syncDB)
        {
            String readSql = "select ";
            foreach (String item in IniUtil.dutyFund())
            {
                readSql += item + ",";
            }
            readSql = readSql.Trim(',');
            readSql += " from netbar_duty_funds where dutyId=" + dutyId;

            return this.readList(readSql, IniUtil.dutyFund(), syncDB);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dutyId"></param>
        /// <returns></returns>
        private List<IDictionary<String, Object>> selectDutyFundsPayType(Int32 dutyId, CDbMysql syncDB)
        {
            String readSql = "select ";
            foreach (String item in IniUtil.dutyPayFund())
            {
                readSql += item + ",";
            }
            readSql = readSql.Trim(',');
            readSql += " from netbar_duty_funds_paytype where dutyID=" + dutyId;
            return this.readList(readSql, IniUtil.dutyPayFund(), syncDB);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dutyId"></param>
        /// <returns></returns>
        private List<IDictionary<string, Object>> selectDutyConsumes(Int32 dutyId, CDbMysql syncDB)
        {
            String readSql = "select ";
            foreach (String item in IniUtil.dutyConsume())
            {
                readSql += item + ",";
            }
            readSql = readSql.Trim(',');
            readSql += " from netbar_duty_consume where dutyID=" + dutyId;

            return this.readList(readSql, IniUtil.dutyConsume(), syncDB);
        }

        #endregion

        #region 同步回调

        /// <summary>
        /// 会员同步回调
        /// </summary>
        /// <param name="time">同步时间</param>
        public void updateMemberSyncTime(string time, CDbMysql syncDB)
        {
            string sql = string.Format("update sync_member_update_date set updateDate='{0}' where id={1}", time, 0);
            int result = syncDB.ExecuteSql(sql);
            if (result > 0)
            {
                //LogHelper.WriteLog(sql + " success");
            }
            else
            {
                LogHelper.WriteLog(sql + " fail");
            }
        }

        /// <summary>
        /// 充值订单同步回调
        /// </summary>
        /// <param name="rechargeOrderList">充值订单列表</param>
        private void processRechargeResponse(String rechargeOrderList, CDbMysql syncDB)
        {
            if (!String.IsNullOrEmpty(rechargeOrderList))
            {
                List<String> list = JsonUtil.DeserializeJsonToList<String>(rechargeOrderList);

                if (list == null || list.Count <= 0)
                {
                    // LogHelper.WriteLog("云没有同步数据返回");
                    return;
                }

                try
                {
                    String strSync = "";
                    for (int i = 0; i < list.Count; i++)
                    {
                        strSync += " (" + list[i] + ",1),";
                    }
                    strSync = strSync.Trim(',');

                    String sql = "insert into sync_recharge_order(rechargeOrderID,sync) values " + strSync;
                    if (syncDB.ExecuteSql(sql) > 0)
                    {
                        // LogHelper.WriteLog("sql:" + sql + " execute success");
                    }
                    else
                    {
                        LogHelper.WriteLog("sql:" + sql + " execute fail");
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("插入充值订单同步数据出错,错误；/n", ex);
                }
            }
        }

        /// <summary>
        /// 卡扣订单同步回调
        /// </summary>
        /// <param name="goodsList">卡扣订单列表</param>
        private void processGoodsOrderList(String goodsList, CDbMysql syncDB)
        {
            if (!String.IsNullOrEmpty(goodsList))
            {
                List<String> list = JsonUtil.DeserializeJsonToList<String>(goodsList);

                if (list == null || list.Count <= 0)
                {
                    // LogHelper.WriteLog("云没有同步数据返回");
                    return ;
                }

                try
                {
                    String strSync = "";
                    for (int i = 0; i < list.Count; i++)
                    {
                        strSync += " (" + list[i] + ",1),";
                    }
                    strSync = strSync.Trim(',');

                    String sql = "insert into sync_netbar_goods_order (goodsOrderID,sync) VALUES " + strSync;
                    if (syncDB.ExecuteSql(sql) > 0)
                    {
                        // LogHelper.WriteLog("sql:" + sql + " execute success");
                    }
                    else
                    {
                        LogHelper.WriteLog("sql:" + sql + " execute fail");
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("插入卡扣订单同步数据出错,错误；/n", ex);
                }
            }
        }

        /// <summary>
        /// 上机信息同步回调
        /// </summary>
        /// <param name="onlineIDList">上机列表</param>
        private void processOnlineListReponse(String onlineIDList, CDbMysql syncDB)
        {
            if (!String.IsNullOrEmpty(onlineIDList))
            {
                List<String> list = JsonUtil.DeserializeJsonToList<String>(onlineIDList);

                if (list == null || list.Count <= 0)
                {
                    // LogHelper.WriteLog("云没有同步数据返回");
                    return;
                }

                try
                {
                    String strSync = "";
                    for (int i = 0; i < list.Count; i++)
                    {
                        strSync += " (" + list[i] + ",1),";
                    }
                    strSync = strSync.Trim(',');

                    String sql = "insert into sync_online(onlineID,sync) values " + strSync;
                    if (syncDB.ExecuteSql(sql) > 0)
                    {
                        // LogHelper.WriteLog("sql:" + sql + " execute success");
                    }
                    else
                    {
                        LogHelper.WriteLog("sql:" + sql + " execute fail");
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("插入上机信息同步数据出错,错误；/n", ex);
                }
            }
        }

        /// <summary>
        /// 交班信息同步回调
        /// </summary>
        /// <param name="dutyList">交班列表</param>
        private void processDutyListResponse(String dutyList, CDbMysql syncDB)
        {
            if (!String.IsNullOrEmpty(dutyList))
            {
                List<String> list = JsonUtil.DeserializeJsonToList<String>(dutyList);

                if (list == null || list.Count <= 0)
                {
                    // LogHelper.WriteLog("云没有同步数据返回");
                    return;
                }

                try
                {
                    String strSync = "";
                    for (int i = 0; i < list.Count; i++)
                    {
                        strSync += " (" + list[i] + ",1),";
                    }
                    strSync = strSync.Trim(',');

                    String sql = "insert into sync_duty(dutyID,sync) VALUES " + strSync;
                    if (syncDB.ExecuteSql(sql) > 0)
                    {
                        // LogHelper.WriteLog("sql:" + sql + " execute success");
                    }
                    else
                    {
                        LogHelper.WriteLog("sql:" + sql + " execute fail");
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("插入交班信息同步数据出错,错误；/n", ex);
                }
            }
        }

        private void processMemberIDResponse(String memberList, CDbMysql syncDB)
        {
            if (!String.IsNullOrEmpty(memberList))
            {
                List<String> list = JsonUtil.DeserializeJsonToList<String>(memberList);

                if (list == null || list.Count <= 0)
                {
                    // LogHelper.WriteLog("云没有同步数据返回");
                    return;
                }

                // memberID去重
                list = list.Distinct().ToList();
                try
                {
                    String strSync = "";
                    for (int i = 0; i < list.Count; i++)
                    {
                        strSync += " (" + list[i] + ",0),";
                    }
                    strSync = strSync.Trim(',');

                    String sql = "insert into sync_member(memberID, sync) VALUES " + strSync;
                    if (syncDB.ExecuteSql(sql) > 0)
                    {
                        // LogHelper.WriteLog("sql:" + sql + " execute success");
                    }
                    else
                    {
                        LogHelper.WriteLog("sql:" + sql + " execute fail");
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("插入云端会员失败返回信息数据出错,错误；/n", ex);
                }
            }
        }

        /// <summary>
        /// 处理遗漏会员回调
        /// </summary>
        /// <param name="memberList">会员信息</param>
        /// <param name="syncDB"></param>
        private void dealMemberIDResponse(String memberList, CDbMysql syncDB)
        {
            if (!String.IsNullOrEmpty(memberList))
            {
                List<String> list = JsonUtil.DeserializeJsonToList<String>(memberList);

                if (list == null || list.Count <= 0)
                {
                    // LogHelper.WriteLog("云没有同步数据返回");
                    return;
                }
                
                try
                {
                    String strSync = " (" + string.Join(",", list.ToArray()) + ")"; 

                    LogHelper.WriteLog("处理omit member成功,memberID list ：" + strSync);

                    String sql = "delete from sync_member where memberID in " + strSync;
                    if (syncDB.ExecuteSql(sql) > 0)
                    {
                        // LogHelper.WriteLog("sql:" + sql + " execute success");
                    }
                    else
                    {
                        LogHelper.WriteLog("sql:" + sql + " execute fail");
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("处理omit member数据出错,错误；/n", ex);
                }
            }
        }

        /// <summary>
        /// web请求失败回调
        /// </summary>
        /// <param name="memberList">会员信息</param>
        /// <param name="syncDB"></param>
        private void errWebResponse(List<IDictionary<string, Object>> list, CDbMysql syncDB)
        {
            if (list == null || list.Count <= 0)
            {
                return;
            }

            try
            {
                String strSync = "";
                for (int i = 0; i < list.Count; i++)
                {
                    strSync += " (" + list[i]["memberID"].ToString() + ",0),";
                }
                strSync = strSync.Trim(',');

                String sql = "insert into sync_member(memberID, sync) VALUES " + strSync;
                if (syncDB.ExecuteSql(sql) > 0)
                {
                    // LogHelper.WriteLog("sql:" + sql + " execute success");
                }
                else
                {
                    LogHelper.WriteLog("sql:" + sql + " execute fail");
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("插入云端会员失败返回信息数据出错,错误；/n", ex);
            }
        }

        #endregion
    }
}
