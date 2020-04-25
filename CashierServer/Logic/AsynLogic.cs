using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CashierServer.Util;
using System.IO;
using Newtonsoft.Json.Linq;
using CashierLibrary.Util;
using CashierServer.DAO;
using CashierServer.Model;
using System.Data;

namespace CashierServer.Logic
{
    public class AsynLogic : BaseLogic
    {

        private DateTime updateDateTime = System.DateTime.Now;


        /// <summary>
        /// 同步线程
        /// </summary>
        public override void tick()
        {
            object obj = new object();
            Monitor.Enter(obj);

            int tick = 0;

            while (true)
            {

                Monitor.Wait(obj, 5000, false);
                
                try
                {

                    this.initMysql();
                    tick++;
                    if (tick % 12 == 0)
                    {
                        this.uploadOnlineInfo();
                        continue;
                    }
                    

                    IDictionary<String, Object> requestBody = new Dictionary<String, Object>();

                    IDictionary<string, string> checkParams = HttpUtil.initParams();
                    foreach (KeyValuePair<string, string> kv in checkParams)
                    {
                        requestBody.Add(kv.Key, kv.Value);
                    }

                    List<IDictionary<String, Object>> orderList = this.syncRechargeOrder();
                    requestBody.Add("rechargeOrderList", orderList);

                    List<IDictionary<string, Object>> onlineList = this.syncOnline();
                    requestBody.Add("onlineList", onlineList);

                    this.updateDateTime = this.readLastUpdateTime();

                    List<IDictionary<string, Object>> memberList = this.syncMember(updateDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    requestBody.Add("memberList", memberList);
                    List<IDictionary<String, Object>> memberAccountList = this.syncMemberAccount(updateDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    requestBody.Add("memberAccountList", memberAccountList);
                    List<GoodsOrderDTO> goodsOrderList = this.GetGoodsOrderList();

                    requestBody.Add("goodsOrderList", goodsOrderList);
                    
                    List<IDictionary<String, Object>> dutyList = this.syncDuty();
                    requestBody.Add("dutyList", dutyList);

                    List<IDictionary<String, Object>> billingList = new List<IDictionary<String, Object>>();
                    requestBody.Add("billingList", billingList);

                    List<IDictionary<String, Object>> onlineRoomList = new List<IDictionary<String, Object>>(); //this.syncOnlineRoom();
                    requestBody.Add("onlineRoomList", onlineRoomList);

                    if ((orderList == null || orderList.Count == 0) && (onlineList == null || onlineList.Count == 0) && (memberList == null || memberList.Count == 0) && (memberAccountList == null || memberAccountList.Count == 0) && (goodsOrderList == null || goodsOrderList.Count == 0) && (dutyList == null || dutyList.Count == 0))
                    {
                        //LogHelper.WriteLog("暂时无数据可上报");

                    }
                    else
                    {
                        String body = JsonUtil.SerializeObject(requestBody);

                        String response = HttpUtil.doPost(IniUtil.syncUrl(), body);
                        
                        JObject result = JObject.Parse(response);

                        if (Int32.Parse(result["code"].ToString()) == 0)
                        {
                            this.updateMemberSyncTime(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                            this.processRechargeResponse(result["data"]["rechargeOrderList"].ToString());
                            this.processOnlineListReponse(result["data"]["syncOnlineList"].ToString());
                            this.processDutyListResponse(result["data"]["syncDutyList"].ToString());
                            this.ProcessGoodsOrderList(result["data"]["syncGoodsOrderList"].ToString());
                            
                        }
                        else
                        {
                            LogHelper.WriteLog("sync error, message:" + result["message"].ToString());
                        }

                    }

                }

                catch (Exception ex)
                {
                    LogHelper.WriteLog("同步错误，错误信息为:", ex);
                }
                finally
                {
                    this.closeMysql();
                }

            }

        }


        
        private List<IDictionary<string, Object>> syncMember(string date)
        {
            List<IDictionary<String, Object>> list = null;

            String readSql = "select ";
            foreach (String item in IniUtil.member())
            {
                readSql += item + ",";
            }
            
            readSql = readSql.Trim(',');
            readSql += " from netbar_member where lastUpdateDate >= '" + date + "'";

            list = this.readList2(readSql, IniUtil.member());

            return list;
        }


        private List<IDictionary<string, object>> syncMember(HashSet<String> memberIdList)
        {
            if (memberIdList.Count <= 0)
            {
                return new List<IDictionary<string, object>>();
            }

            List<IDictionary<string, Object>> list = new List<IDictionary<string, object>>();

            String inStr = "(";
            foreach (String memberID in memberIdList)
            {
                inStr += memberID + ",";
            }
            inStr = inStr.Trim(',');
            inStr += ")";

            String readSql = "select ";
            foreach (string item in IniUtil.member())
            {
                readSql += item + ",";
            }
            readSql = readSql.Trim(',');
            readSql += " from netbar_member where memberID in ";
            readSql += inStr;

            list = this.readList(readSql, IniUtil.member());

            return list;
        }

        private List<IDictionary<string, Object>> syncMemberAccount(string date)
        {
            List<IDictionary<String, Object>> list = null;

            String readSql = "select ";
            foreach (String item in IniUtil.memberAccount())
            {
                readSql += item + ",";
            }
            readSql = readSql.Trim(',');
            readSql += " from netbar_member_account where lastUpdateTime >= '" + date + "'";

            list = this.readList2(readSql, IniUtil.memberAccount());
            if (list.Count > 0)
            {
                return list;
            }
            return list;

        }

        /// <summary>
        /// 获取消费订单列表
        /// </summary>
        /// <returns>获取数据</returns>
        public List<GoodsOrderDTO> GetGoodsOrderList()
        {
            try
            {
                List<GoodsOrderDTO> list = null;

                String sql = "SELECT t.goodsOrderID,t.memberID,t.goodsSum,t.orderType,t.orderFee,t.couponDiscoutFee,t.paySum,t.basePayFee,t.state,t.createTime,t.source,t.areaId,t.machineID,t.actorUser,t.actTime,t.reason,t.returnActorUse,t.returnTime,t.authorise,t.dataVersion,t.gid,t.payWay,t.creator,t.goodsOrderDesc " +
                    "from netbar_goods_order t " +
                    "left join sync_netbar_goods_order s on t.goodsOrderID=s.goodsOrderID " +
                    "where s.goodsOrderID is NULL limit 10";

                DataTable dt = this.mysql.GetDataTable(sql, "order");
                if (null != dt && dt.Rows.Count > 0)
                {
                    list = DataConvertUtil<GoodsOrderDTO>.FillModel(dt);
                }
                return list;

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("查询同步列表出错,错误：\n", ex);
                return new List<GoodsOrderDTO>();
            }
        }

        /// <summary>
        /// 插入同步数据
        /// </summary>
        /// <param name="list">goodsOrderId</param>
        /// <returns>返回成功条数</returns>
        public int InsertSyncOrder(List<String> list)
        {
            if (list == null || list.Count <= 0)
            {
                //LogHelper.WriteLog("云没有同步数据返回");
                return -1;
            } 
            try
            {
                String strSync = "";
                int nCount = list.Count;
                for (int i = 0; i < nCount; i++)
                {
                    if (i == (nCount - 1))
                    {
                        strSync += " (" + list[i] + ",1) ";
                    }
                    else
                    {
                        strSync += " (" + list[i] + ",1),";
                    }
                }
                String sql = "insert into sync_netbar_goods_order (goodsOrderID,sync) VALUES " + strSync;
                int nRet = this.mysql.ExecuteSql(sql);

                if (nRet > 0)
                {
                    LogHelper.WriteLog("sql:" + sql + " execute success");
                }
                else
                {
                    LogHelper.WriteLog("sql:" + sql + " execute fail");
                }
                return nRet;

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("插入同步数据出错,错误；/n", ex);
            }

            return 0;
        }

        private List<IDictionary<String, Object>> syncRechargeOrder()
        {
            String readSql = "select ";
            foreach (String item in IniUtil.rechargeOrder())
            {
                readSql += "n." + item + ",";
            }
            readSql = readSql.Trim(',');
            readSql += " from netbar_recharge_order n left join sync_recharge_order s on n.rechargeOrderID=s.rechargeOrderID where s.sync is NULL";

            List<IDictionary<String, Object>> list = this.readList(readSql, IniUtil.rechargeOrder());
            return list;

        }

        private void processRechargeResponse(String rechargeOrderList)
        {
            List<String> list = JsonUtil.DeserializeJsonToList<String>(rechargeOrderList);

            foreach (String orderId in list)
            {
                this.addRechargeOrder(Int64.Parse(orderId));
            }

        }

        private void addRechargeOrder(Int64 rechargeOrderID)
        {
            String sql = string.Format("insert into sync_recharge_order(rechargeOrderID,sync) values({0},{1})", rechargeOrderID, 1);
            int result = this.mysql.ExecuteSql(sql);
            if (result > 0)
            {
                LogHelper.WriteLog(sql + " add sync succeess");
            }
            else
            {
                LogHelper.WriteLog(sql + " add sync fail ");
            }
        }

        private List<IDictionary<String, Object>> syncDuty()
        {
            String readSql = "select ";
            foreach (String item in IniUtil.duty())
            {
                readSql += "d." + item + ",";
            }

            readSql = readSql.Trim(',');
            readSql += " from netbar_duty d left join sync_duty s on d.dutyID=s.dutyID where s.sync is NULL";

            List<IDictionary<String, Object>> list = this.readList(readSql, IniUtil.duty());

            foreach (IDictionary<String, Object> item in list)
            {

                item.Add("dutyFundsList", this.selectDutyFunds(Int32.Parse(item["dutyID"].ToString())));
                item.Add("dutyFundsPaytypeList", this.selectDutyFundsPayType(Int32.Parse(item["dutyID"].ToString())));
                item.Add("dutyConsumeList", this.selectDutyConsumes(Int32.Parse(item["dutyID"].ToString())));

            }

            return list;
        }

        private List<IDictionary<String, Object>> selectDutyFunds(Int32 dutyId)
        {

            String readSql = "select ";

            foreach (String item in IniUtil.dutyFund())
            {
                readSql += item + ",";
            }

            readSql = readSql.Trim(',');
            readSql += " from netbar_duty_funds where dutyId=" + dutyId;
            List<IDictionary<String, Object>> list = this.readList(readSql, IniUtil.dutyFund());
            return list;
        }

        private List<IDictionary<String, Object>> selectDutyFundsPayType(Int32 dutyId)
        {
            String readSql = "select ";

            foreach (String item in IniUtil.dutyPayFund())
            {
                readSql += item + ",";
            }

            readSql = readSql.Trim(',');
            readSql += " from netbar_duty_funds_paytype where dutyID=" + dutyId;
            List<IDictionary<String, Object>> list = this.readList(readSql, IniUtil.dutyPayFund());
            return list;
        }

        private List<IDictionary<string, Object>> selectDutyConsumes(Int32 dutyId)
        {
            string readSql = "select ";

            foreach (String item in IniUtil.dutyConsume())
            {
                readSql += item + ",";
            }

            readSql = readSql.Trim(',');
            readSql += " from netbar_duty_consume where dutyID=" + dutyId;

            List<IDictionary<string, Object>> list = this.readList(readSql, IniUtil.dutyConsume());
            return list;
        }


        private List<IDictionary<String, Object>> syncOnline()
        {
            String selectSql = "select ";
            foreach (String item in IniUtil.online())
            {
                selectSql += "o." + item + ",";
            }
            selectSql = selectSql.Trim(',');
            selectSql += " from netbar_online o left join sync_online s on o.onlineID=s.onlineID where o.state=2 and s.sync is NULL";

            List<IDictionary<String, Object>> list = this.readList(selectSql, IniUtil.online());
            return list;
        }

        private List<IDictionary<String, Object>> syncOnlineRoom()
        {
            String selectSql = "select ";

            foreach (String item in IniUtil.onlineRoom())
            {
                selectSql += "o." + item + ",";
            }

            selectSql = selectSql.Trim(',');
            selectSql += " from netbar_online_room o left join sync_online_room s on o.onlineRoomID=s.onlineRoomID where s.sync is NULL";

            List<IDictionary<String, Object>> list = this.readList(selectSql, IniUtil.onlineRoom());
            return list;
        }



        private List<IDictionary<String, Object>> syncBilling()
        {
            String readSql = "select ";
            foreach (String item in IniUtil.billing())
            {
                readSql += "b." + item + ",";
            }

            readSql = readSql.Trim(',');

            readSql += " from netbar_billing b left join sync_billing s on b.billingID=s.billingID where s.sync is NULL limit 10";

            List<IDictionary<String, Object>> list = this.readList(readSql, IniUtil.billing());

            return list;
        }

        private List<IDictionary<String, Object>> syncGoodsOrder(DateTime date)
        {
            List<String> columns = new List<String>();
            columns.Add("goodsOrderID");
            columns.Add("memberID");

            String readSql = "select ";
            foreach (String item in columns)
            {
                readSql += item + ",";
            }
            readSql = readSql.Trim(',');
            readSql += " from netbar_goods_order where createTime >= '" + date.ToString() + "'";

            List<IDictionary<String, Object>> list = this.readList(readSql, columns);
            return list;
        }

        public DateTime readLastUpdateTime()
        {
            String readSql = "select id,updateDate from sync_member_update_date where id=0";

            List<String> columns = new List<String>();
            columns.Add("id");
            columns.Add("updateDate");

            IDictionary<String, Object> resultMap = this.readMap(readSql, columns);
            return DateTime.Parse(resultMap["updateDate"].ToString());
        }

        public void updateMemberSyncTime(string time)
        {
            string sql = string.Format("update sync_member_update_date set updateDate='{0}' where id={1}", time, 0);
            int result = this.mysql.ExecuteSql(sql);
            if (result > 0)
            {
                //LogHelper.WriteLog(sql + " success");
            }
            else
            {
                LogHelper.WriteLog(sql + " fail");
            }
        }




        private void processBillingResponse(String billingIDList)
        {
            List<String> list = JsonUtil.DeserializeJsonToList<String>(billingIDList);
            foreach (String billingID in list)
            {
                this.addBillingSync(Int64.Parse(billingID));
            }
        }


        private void addBillingSync(Int64 billingID)
        {
            String sql = string.Format("insert into sync_billing(billingID,sync) values({0},{1})", billingID, 1);
            int result = this.mysql.ExecuteSql(sql);
            if (result > 0)
            {
                LogHelper.WriteLog(sql + " add billing sync success");
            }
            else
            {
                LogHelper.WriteLog(sql + " add billing sync fail");
            }

        }


        private void processMemberListResponse(String memberIDList)
        {
            List<string> list = JsonUtil.DeserializeJsonToList<string>(memberIDList);
            foreach (string memberID in list)
            {
                this.addMemberSync(Int64.Parse(memberID));
            }
        }

        private void addMemberSync(Int64 memberID)
        {
            String sql = string.Format("insert into sync_member(memberID,sync) values({0},{1})", memberID, 1);
            int result = this.mysql.ExecuteSql(sql);
            if (result > 0)
            {
                LogHelper.WriteLog(sql + " add member sync succss");
            }
            else
            {
                LogHelper.WriteLog(sql + " add member sync fail");
            }
        }

        private void processOnlineRoomListResponse(String onlineRoomIDList)
        {
            List<String> list = JsonUtil.DeserializeJsonToList<string>(onlineRoomIDList);
            foreach (string onlineRoomID in list)
            {
                this.addOnlineRoomSync(Int64.Parse(onlineRoomID));
            }
        }

        private void addOnlineRoomSync(Int64 onlineRoomID)
        {
            string sql = string.Format("insert into sync_online_room(onlineRoomID,sync) values({0},{1})", onlineRoomID, 1);
            int result = this.mysql.ExecuteSql(sql);
            if (result > 0)
            {
                LogHelper.WriteLog(sql + " add sync succs");
            }
            else
            {
                LogHelper.WriteLog(sql + " add sync fail");
            }
        }

        private void processOnlineListReponse(String onlineIDList)
        {
            List<string> list = JsonUtil.DeserializeJsonToList<string>(onlineIDList);
            foreach (string onlineID in list)
            {
                this.addOnlineSync(Int64.Parse(onlineID));
            }
        }

        private void addOnlineSync(Int64 onlineID)
        {
            string sql = string.Format("insert into sync_online(onlineID,sync) values({0},{1})", onlineID, 1);
            int result = this.mysql.ExecuteSql(sql);
            if (result > 0)
            {
                LogHelper.WriteLog(sql + " add sync 成功");
            }
            else
            {
                LogHelper.WriteLog(sql + " add sync 失败");
            }
        }


        private void processDutyListResponse(string dutyList)
        {
            List<string> list = JsonUtil.DeserializeJsonToList<string>(dutyList);
            foreach (string dutyID in list)
            {
                this.addDutySync(Int64.Parse(dutyID));
            }
        }

        private void ProcessGoodsOrderList(String goodsList)
        {
            if (!String.IsNullOrEmpty(goodsList))
            {
                List<String> list = JsonUtil.DeserializeJsonToList<String>(goodsList);
                
                this.InsertSyncOrder(list);

            }
        }

        private void addDutySync(Int64 dutyID)
        {
            string sql = string.Format("insert into sync_duty(dutyID,sync) values({0},{1})",dutyID,1);
            int result = this.mysql.ExecuteSql(sql);
            if (result > 0)
            {
                LogHelper.WriteLog(sql + " add sync 成功");
            }
            else
            {
                LogHelper.WriteLog(sql + " add sync 失败");
            }
        }

        private void uploadOnlineInfo()
        {
            try
            {
                IDictionary<String, Object> requestBody = new Dictionary<String, Object>();

                List<IDictionary<String, Object>> data = this.getOnlineMember();
                IDictionary<string, string> checkParams = HttpUtil.initParams();
                foreach (KeyValuePair<string, string> kv in checkParams)
                {
                    requestBody.Add(kv.Key, kv.Value);
                }

                requestBody.Add("online", data);

                String body = JsonUtil.SerializeObject(requestBody);
                HttpUtil.doPost(IniUtil.upOnlineUrl(), body);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("uploadOnlineInfo ex", ex);
            }

        }

        private List<IDictionary<String, Object>> getOnlineMember()
        {
            List<string> list = new List<string>();

            list.Add("memberID");
            list.Add("machineName");
            list.Add("areaName");
            list.Add("gid");

            String selectSql = "select memberID,machineName,areaName,gid from netbar_online where state=1";

            List<IDictionary<String, Object>> data = this.readList2(selectSql, list);
            return data;
        }

    }
}
