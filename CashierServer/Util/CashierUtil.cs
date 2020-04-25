using CashierLibrary.Util;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CashierServer.Util
{
    class CashierUtil
    {

        private static CDbMysql mysql;

        public static void initMysql()
        {
            mysql = new CDbMysql(IniUtil.dbHost(), AesUtil.Decrypt(IniUtil.user()), AesUtil.Decrypt(IniUtil.password()), IniUtil.dbName(), IniUtil.dbPort());
        }

        public static void closeMysql()
        {
            mysql.DbClose();
        }

        private static string password = "aida87014999";

        public static void changeMysqlPsd()
        {
            IniUtil.setUser(AesUtil.Encrypt("root"));
            IniUtil.setPassword(AesUtil.Encrypt(password));
        }
        
        /// <summary>
        /// 检测系统是否存在mysql服务,不存在则创建
        /// </summary>
        public static void InstallMysql()
        {
            var services = ServiceController.GetServices();
            var server = services.FirstOrDefault(s => s.ServiceName.Equals("mysqlaida"));
            if (null == server)
            {
                using (Process proc = new Process())
                {
                    System.Diagnostics.Process mysqld = new System.Diagnostics.Process();
                    string dir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                    DirectoryInfo info = Directory.GetParent(dir);
                    info = Directory.GetParent(info.FullName);
                    String cmdFile = info.FullName + "\\mysql\\" + "InstallMysql.bat";
                    proc.StartInfo.FileName = cmdFile;
                    //proc.StartInfo.Arguments = string.Format("10");//this is argument
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.Start();
                    proc.WaitForExit();
                }

            }
        }



        public static string GetGateWay()
        {
            string strGateway = "";

            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var netWork in nics)
            {
                IPInterfaceProperties ip = netWork.GetIPProperties();
                
                GatewayIPAddressInformationCollection gateways = ip.GatewayAddresses;
                foreach (var gateWay in gateways)
                {
                    
                    if (CashierUtil.IsValidIp(gateWay.Address.ToString()))
                    {
                        strGateway = gateWay.Address.ToString();
                        break;
                    }
                }

                if (strGateway.Length > 0)
                {
                    break;
                }
            }

            return strGateway;
        }


        public static bool IsValidIp(string ip)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(ip, "[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}"))
            {
                string[] ips = ip.Split('.');
                if (ips.Length == 4)
                {
                    if (System.Int32.Parse(ips[0]) < 256 && System.Int32.Parse(ips[1]) < 256 & System.Int32.Parse(ips[2]) < 256 & System.Int32.Parse(ips[3]) < 256)
                        return true;
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }
            else
                return false;

        }



        /// <summary>
        /// 判断mysql服务是否运行
        /// </summary>
        /// <returns></returns>
        public static bool BisStartSqlService()
        {
            var services = ServiceController.GetServices();

            var server = services.FirstOrDefault(s => s.ServiceName.Equals("mysqlaida"));
            if (null != server)
            {
                if (!server.Status.Equals(ServiceControllerStatus.Running))
                {
                    server.Start();
                    server.WaitForStatus(ServiceControllerStatus.Running);
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// 测试数据连接
        /// </summary>
        /// <returns></returns>
        public static bool TestConn()
		{
			return mysql.TestConn();
		}

        public static string loadConfig()
        {

            IDictionary<string, string> configParams = HttpUtil.initParams();
            configParams.Add("shopid", ServerUtil.currentUser.shopid + "");
            String config = HttpUtil.doPost(IniUtil.configUrl(), configParams);

            JObject json = JObject.Parse(config);
            
            CashierUtil.updateAreaList(JArray.Parse(json["data"]["areaConfig"].ToString()));

            CashierUtil.updateMemberTypeList(JArray.Parse(json["data"]["memberTypes"].ToString()));

            CashierUtil.updateRechargeCompaignList(JArray.Parse(json["data"]["rechargeCompaignList"].ToString()));
            
            return config;
        }

        public static void CheckIfExistsTableSyncNetbarGoodsOrder()
        {
            string readSql = "SELECT COUNT(*) FROM information_schema.`TABLES` WHERE TABLE_SCHEMA = 'pos' AND TABLE_NAME ='sync_netbar_goods_order'";
            int count = CashierUtil.mysql.GetCount(readSql);
            if (count == 0)
            {
                LogHelper.WriteLog("check table sync_netbar_goods_order does not exist!");

                String sql = @"CREATE TABLE `sync_netbar_goods_order` (

                                `goodsOrderID` bigint(20) NOT NULL,
    
                                `sync` int(11) DEFAULT NULL ,
    
                                PRIMARY KEY (`goodsOrderID`)

                            ) ENGINE=InnoDB DEFAULT CHARSET=utf8";

                int result = CashierUtil.mysql.ExecuteSql(sql);
                if (result > 0)
                {
                    LogHelper.WriteLog("CREATE TABLE sync_netbar_goods_order succss");
                }
                else
                {
                    LogHelper.WriteLog("CREATE TABLE sync_netbar_goods_order fail");
                }
            }

        }

        /// <summary>
        /// 检查数据库是否更新，有异常则返回真
        /// </summary>
        /// <returns>更新是否有错误</returns>
        public static bool CheckIfUpdateMysql()
        {
            // db版本号对比
            int dbVersion = int.Parse(IniUtil.getDbVersion());
            if (dbVersion == 0)
            {
                // 第一版本DB更新
                string[] proc = { "addOnlineNew", "dutyDataSaveNew", "updateOnlineByCrossArea", "clearOnlineHistory" };

                bool error = false;

                for (int i = 0; i < proc.Length; i++)
                {
                    string procName = proc[i];
                    bool ifCreate = false;
                    if (procName == "clearOnlineHistory")
                    {
                        if (CashierUtil.mysql.ExecuteSql("DROP PROCEDURE IF EXISTS `clearOnlineHistory`;") >= 0)
                        {
                            LogHelper.WriteLog("drop proc " + procName + " success");
                            ifCreate = true;
                        }
                    }
                    else
                    {
                        if (CashierUtil.mysql.GetCount("SELECT COUNT(*) FROM mysql.proc WHERE db = 'pos' AND name = '" + procName + "';") == 0)
                        {
                            LogHelper.WriteLog("check proc " + procName + " does not exist!");
                            ifCreate = true;
                        }
                    }

                    if (ifCreate == true)
                    {
                        try
                        {
                            string sql = DBVersionUtil.procGain(procName);
                            if (string.IsNullOrEmpty(sql.Trim()))
                            {
                                continue;
                            }
                            if (CashierUtil.mysql.ExecuteSql(sql) >= 0)
                            {
                                LogHelper.WriteLog("CREATE PROC " + procName + " succss");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteLog("CREATE PROC " + procName + " error", ex);
                            error = true;
                        }
                    }
                }

                // 检查索引
                string checkSql = "SELECT COUNT(*) FROM mysql.innodb_index_stats WHERE database_name = 'pos' AND table_name = 'netbar_billing' AND stat_description = 'onlineID,memberID';";
                if (CashierUtil.mysql.GetCount(checkSql) == 0)
                {
                    LogHelper.WriteLog("check index idx_onlineID_memberID does not exist!");

                    try
                    {
                        if (CashierUtil.mysql.ExecuteSql("ALTER TABLE `netbar_billing` ADD INDEX `idx_onlineID_memberID` (`onlineID`, `memberID`) USING BTREE;") >= 0)
                        {
                            LogHelper.WriteLog("ADD INDEX idx_onlineID_memberID succss");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLog("ADD INDEX idx_onlineID_memberID error", ex);
                        error = true;
                    }
                }

                if (error == true)
                {
                    LogHelper.WriteLog("db version upgrade, db version is " + dbVersion);
                }
                else
                {
                    dbVersion = 1;
                    IniUtil.setDbVersion(dbVersion.ToString());
                    LogHelper.WriteLog("db version upgrade, db version is " + dbVersion);
                }

                return error;
            }

            return false;
        }

        public static void pullRechargeCompaignList()
        {
            IDictionary<string, string> shopparams = HttpUtil.initParams();
            shopparams.Add("shopid", ServerUtil.currentUser.shopid + "");
            String config = HttpUtil.doPost(IniUtil.rechargeConfigUrl(), shopparams);

            JObject json = JObject.Parse(config);

            CashierUtil.updateRechargeCompaignList(JArray.Parse(json["data"]["rechargeCompaignList"].ToString()));

        }

        public static void syncAccount(long memberId)
        {
            Int64 tm = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
            IDictionary<string, object> parameters = new Dictionary<string, object>();

            parameters.Add("fn", "serverUserSync");
            parameters.Add("ver", "1.0");
            parameters.Add("tm", tm);
            parameters.Add("token", generateToken("serverUserSync", tm + ""));

            IDictionary<string, long> data = new Dictionary<string, long>();

            data.Add("memberId", memberId);

            parameters.Add("data", data);

            String body = JsonUtil.SerializeObject(parameters);

            try
            {

                String response = HttpUtil.doPost("http://127.0.0.1:18000/cashier/surfchannel", body);
                LogHelper.WriteLog("sync member:" + memberId + ",response:" + response);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("sync error",ex);
            }
        }

        public static void resetRate(JArray pcList)
        {
            Int64 tm = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
            IDictionary<string, object> parameters = new Dictionary<string, object>();

            parameters.Add("fn", "serverGidLogin");
            parameters.Add("ver", "1.0");
            parameters.Add("tm", tm);
            parameters.Add("token", generateToken("serverGidLogin", tm + ""));

            IDictionary<string, object> data = new Dictionary<string, object>();

            data.Add("gid",ServerUtil.currentUser.shopid);
            data.Add("list",pcList);
            
            parameters.Add("data",data);

            String body = JsonUtil.SerializeObject(parameters);

            LogHelper.WriteLog("reset rate request :" + body);

            String response = HttpUtil.doPost("http://127.0.0.1:18000/cashier/surfchannel", body);

            LogHelper.WriteLog("reset rate response:" + response);

        }

        private static string generateToken(string fn, string tm)
        {
            string result = MD5Util.EncryptWithMd5(fn + tm + IniUtil.key());
            return result;
        }

        private static void updateAreaList(JArray arealist)
        {
            int shopid = ServerUtil.currentUser.shopid;
            
            foreach (var item in arealist)
            {
                JObject area = JObject.Parse(item.ToString());

                Int32 areaId = Int32.Parse(area["areaId"].ToString());
                string areaName = area["areaName"].ToString();
                Int32 tariffRoomType = Int32.Parse(area["tariffRoomType"].ToString());

                CashierUtil.insertOrUpdateArea(areaId, areaName, tariffRoomType, ServerUtil.currentUser.shopid);
            }
        }
        
        private static void insertOrUpdateArea(Int32 areaId, string areaName, Int32 tariffRoomType, Int32 shopid)
        {

            string updateSql = string.Format("update netbar_area set areaName='{0}',ifRoom={1} where areaID={2}",
                    areaName,tariffRoomType, areaId);

            int result = CashierUtil.mysql.ExecuteSql(updateSql);
            if (result > 0)
            {
                //LogHelper.WriteLog(updateSql + " execute success");
            }
            else
            {
                updateSql = string.Format("update netbar_area set areaID={0},ifRoom={1} where areaName='{2}'", areaId,tariffRoomType, areaName);
                result = CashierUtil.mysql.ExecuteSql(updateSql);
                if (result > 0)
                {
                    return;
                }
                else
                {
                    string insertSql = string.Format("insert into netbar_area(areaID,areaName,areaType,num,ifRoom,description,state,dataVersion,memberTypeList,gid) values({0},'{1}',{2},0,{3},'{4}',0,0,'',{5})",
                        areaId, areaName, 0, tariffRoomType, areaName, shopid);

                    result = CashierUtil.mysql.ExecuteSql(insertSql);
                    if (result > 0)
                    {
                        //LogHelper.WriteLog(insertSql + " execute success");
                    }
                    else
                    {
                        LogHelper.WriteLog(insertSql + " execute fail");
                    }
                }
            }
            
        }
        
        public static string getConfigByNet()
        {
            IDictionary<string, string> configParams = HttpUtil.initParams();
            configParams.Add("shopid", ServerUtil.currentUser.shopid + "");
            String config = HttpUtil.doPost(IniUtil.configUrl(), configParams);
            return config;
        }
        
        public static JArray processPcList()
        {
            int shopid = ServerUtil.currentUser.shopid;
            LogHelper.WriteLog("load pc List config start");

            IDictionary<string, string> configParams = HttpUtil.initParams();
            configParams.Add("shopid", ServerUtil.currentUser.shopid + "");
            String machineList = HttpUtil.doPost(IniUtil.machineUrl(), configParams);
            JObject jObject = JObject.Parse(machineList);

            JArray pcList = JArray.Parse(jObject.GetValue("data").ToString());

            CashierUtil.disableAllPc();

            foreach (var item in pcList)
            {
                JObject pc = JObject.Parse(item.ToString()); //pcList.ElementAt<JObject>(i);

                int machineId = Int32.Parse(pc["machineId"].ToString());
                string machineName = pc["machineName"].ToString();

                int areaId = Int32.Parse(pc["areaId"].ToString());

                CashierUtil.insertOrUpdatePc(machineId, machineName, areaId, shopid);
            }

            LogHelper.WriteLog("load pc List config end");

            return pcList;
        }



        private static void initTheMemberTypePlan(Int32 shopid)
        {
            string updateSql = string.Format("update netbar_membertype_plan set gid={0}", shopid);
            int result = CashierUtil.mysql.ExecuteSql(updateSql);

            if (result > 0)
            {
                return;
            }

            string insertSql = string.Format("insert into netbar_membertype_plan(memberTypePlanID,state,dataVersion,gid,theOrder) values(1,1,0,{0},0)", shopid);
            result = CashierUtil.mysql.ExecuteSql(insertSql);
            if (result > 0)
            {
                //LogHelper.WriteLog(insertSql + " execute success");
            }
            else
            {
                LogHelper.WriteLog(insertSql + " execute fail");
            }

        }


        public static void updateRechargeCompaignList(JArray compaignList)
        {

            string deleteSql = "delete from netbar_recharge_compaign";
            CashierUtil.mysql.ExecuteSql(deleteSql);

            foreach (JObject item in compaignList)
            {
                int rechargeCompaignID = Int32.Parse(item["rechargeCompaignID"].ToString());
                int memberType = Int32.Parse(item["memberType"].ToString());
                int rechargeFee = Int32.Parse(item["rechargeFee"].ToString());
                int additionalFee = Int32.Parse(item["additionalFee"].ToString());
                int state = Int32.Parse(item["state"].ToString());
                int gid = Int32.Parse(item["gid"].ToString());
                int tid = Int32.Parse(item["tid"].ToString());

                string insertSql = string.Format("insert into netbar_recharge_compaign(rechargeCompaignID,memberType,rechargeFee,additionalFee,state,gid,tid) values({0},{1},{2},{3},{4},{5},{6})",
                            rechargeCompaignID, memberType, rechargeFee, additionalFee, state, gid, tid);
                int result = CashierUtil.mysql.ExecuteSql(insertSql);
                if (result > 0)
                {
                    //LogHelper.WriteLog(insertSql + " execute success");
                }
                else
                {
                    LogHelper.WriteLog(insertSql + " execute fail");
                }
            }
        }

        private static void updateMemberTypeList(JArray typeList)
        {
            int shopid = ServerUtil.currentUser.shopid;
            CashierUtil.initTheMemberTypePlan(shopid);
            foreach (JObject item in typeList)
            {

                int memberTypeId = Int32.Parse(item["memberTypeId"].ToString());
                string memberTypeName = item["memberTypeName"].ToString();
                CashierUtil.insertOrUpdateMemberType(memberTypeId, memberTypeName, shopid);

            }
        }
        
        private static void insertOrUpdateMemberType(Int32 memberTypeId, string memberTypeName, Int32 shopid)
        {
            string updateSql = string.Format("update netbar_membertype set parasTypeName='{0}' where memberTypeID={1}",memberTypeName,memberTypeId);
            int result = CashierUtil.mysql.ExecuteSql(updateSql);

            if(result > 0)
            {
                return;
            }

            string insertSql = string.Format("insert into netbar_membertype(memberTypePlanID,memberTypeID,parasTypeName,dataVersion,gid,theOrder) values(1,{0},'{1}',0,{2},0)",
                memberTypeId, memberTypeName, shopid);
            result = CashierUtil.mysql.ExecuteSql(insertSql);
            if (result > 0)
            {
                //LogHelper.WriteLog(insertSql + " execute success");
            }
            else
            {
                LogHelper.WriteLog(insertSql + " execute fail");
            }

        }
        
        private static void disableAllPc()
        {

            string updatesql = "update netbar_machine set state=0";
            int result = CashierUtil.mysql.ExecuteSql(updatesql);
            if (result > 0)
            {
                //LogHelper.WriteLog(updatesql + " execute success");
            }
            else
            {
                LogHelper.WriteLog(updatesql + " execute fail");
            }

        }
        
        private static void insertOrUpdatePc(Int32 machineId, string machineName, Int32 areaId, Int32 shopid)
        {

            string updateSql = string.Format("update netbar_machine set areaId={0},state=1 where machineName='{1}'",areaId, machineName);

            int result = CashierUtil.mysql.ExecuteSql(updateSql);
            if (result > 0)
            {
                //LogHelper.WriteLog(updateSql + " execute success");
            }
            else
            {
                LogHelper.WriteLog(updateSql + " execute fail");
                string insertSql = string.Format("insert into netbar_machine(machineID,machineName,areaId,state,gid,ip) values({0},'{1}',{2},1,{3},'{4}')",
                            machineId, machineName, areaId, shopid, "");
                result = CashierUtil.mysql.ExecuteSql(insertSql);
                if (result > 0)
                {
                    //LogHelper.WriteLog(insertSql + " execute success");
                }
                else
                {
                    LogHelper.WriteLog(insertSql + " execute fail");
                }
            }
        }
        

    }
}
