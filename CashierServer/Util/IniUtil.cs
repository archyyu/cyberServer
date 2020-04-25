using CashierLibrary.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashierServer.Util
{
    class IniUtil
    {
        public static string file = AppDomain.CurrentDomain.BaseDirectory + "/Config/NetbarConfig.ini";
        
        public static string dbHost()
        {
            string dbHost = IniHelper.INIGetStringValue(IniUtil.file, "DBInfo", "DbHost", "");
            return dbHost.Trim();
        }

        public static string dbPort()
        {
            string dbPort = IniHelper.INIGetStringValue(IniUtil.file, "DBInfo", "DbPort", "");
            return dbPort.Trim();
        }

        public static string user()
        {
            string user = IniHelper.INIGetStringValue(IniUtil.file, "DBInfo", "UserId", "");
            return user.Trim();
        }

        public static void setUser(string user)
        {
            IniHelper.INIWriteValue(IniUtil.file,"DBInfo","UserId",user);
        }

        public static string password()
        {
            string password = IniHelper.INIGetStringValue(IniUtil.file, "DBInfo", "UserPwd", "");
            return password.Trim();
        }

        public static void setPassword(string password)
        {
            IniHelper.INIWriteValue(IniUtil.file,"DBInfo","UserPwd",password);
        }
        

        public static string dbName()
        {
            string dbName = IniHelper.INIGetStringValue(IniUtil.file, "DBInfo", "DbName", "");
            return dbName.Trim();
        }

        public static void setDbVersion(string version)
        {
            IniHelper.INIWriteValue(IniUtil.file, "DBInfo", "DbVersion", version);
        }

        
        public static string getDbVersion()
        {
            string dbVersion = IniHelper.INIGetStringValue(IniUtil.file, "DBInfo", "DbVersion", "0");
            return dbVersion.Trim();
        }

        public static void setLastTick(UInt32 tick)
        {
            IniHelper.INIWriteValue(IniUtil.file,"AIDA","tick",tick + "");
        }

        public static UInt32 getLastTick()
        {
            string tick = IniHelper.INIGetStringValue(IniUtil.file, "AIDA", "tick", "0");
            return UInt32.Parse(tick);
        }

        #region 同步url

        /// <summary>
        /// 会员相关同步
        /// </summary>
        /// <returns></returns>
        public static string syncMemberUrl()
        {
            return ServerUtil.syncNewUrl + "/syncmember";
        }

        /// <summary>
        /// 订单相关同步
        /// </summary>
        /// <returns></returns>
        public static string syncOrderUrl()
        {
            return ServerUtil.syncNewUrl + "/syncorder";
        }

        /// <summary>
        /// 上机交班相关
        /// </summary>
        /// <returns></returns>
        public static string syncOnlineUrl()
        {
            return ServerUtil.syncNewUrl + "/synconline";
        }

        public static string syncOmitMemberUrl()
        {
            return ServerUtil.syncNewUrl + "/syncomitmember";
        }
        

        /// <summary>
        /// 在线信息相关
        /// </summary>
        /// <returns></returns>
        public static string updateOnlineUrl()
        {
           return ServerUtil.syncNewUrl + "/uponline";
        }

        #endregion

        public static string syncUrl()
        {
            //string syncUrl = IniHelper.INIGetStringValue(IniUtil.file, "AIDA", "syncUrl", "http://yun1.aida58.com/cashier/sync/sync");
            string syncUrl = "http://yun.aida58.com/cashier/sync/sync";
            return syncUrl;
        }

        public static string upOnlineUrl()
        {
            //string upOnlineUrl = IniHelper.INIGetStringValue(IniUtil.file,"AIDA","upOnlineUrl", "http://yun1.aida58.com/cashier/sync/uponline");
            string upOnlineUrl = "http://yun.aida58.com/cashier/sync/uponline";
            return upOnlineUrl;
        }

        public static string configUrl()
        {
            //string syncUrl = IniHelper.INIGetStringValue(IniUtil.file, "AIDA", "configUrl", "http://yun1.aida58.com/api/pulltariff");
            string syncUrl = "http://yun.aida58.com/cashier/init/pulltariff";
            return syncUrl;
        }

		public static string RatConfigUrl()
		{
			//string str_url = IniHelper.INIGetStringValue(IniUtil.file, "AIDA", "ratConfigUrl", "http://yun1.aida58.com/cashier/sync/ifupdatecashierconfig");
            string str_url = "http://yun.aida58.com/cashier/sync/ifupdatecashierconfig";
            return str_url;
		}

        public static string UpdateConfigUrl()
        {
            //string str_url = IniHelper.INIGetStringValue(IniUtil.file, "AIDA", "updateConfigUrl", "http://yun1.aida58.com/cashier/sync/lastservertime");
            string str_url = "http://yun.aida58.com/cashier/sync/lastservertime";
            return str_url;
        }

        public static string rechargeConfigUrl()
        {
            //string rechargeConfigUrl = IniHelper.INIGetStringValue(IniUtil.file, "AIDA", "rechargeConfigUrl", "http://yun1.aida58.com/api/pullrechargecompaign");
            string rechargeConfigUrl = "http://yun.aida58.com/cashier/init/pullrechargecompaign";
            return rechargeConfigUrl;
        }

        public static string machineUrl()
        {
            //string machineUrl = IniHelper.INIGetStringValue(IniUtil.file,"AIDA", "machineUrl", "http://yun1.aida58.com/api/barrgionlist");
            string machineUrl = "http://yun.aida58.com/cashier/init/barrgionlist";
            return machineUrl;
        }

        public static string key()
        {
            string key = "654321";
            //string key = IniHelper.INIGetStringValue(IniUtil.file, "AIDA", "key", "123456");
            return key;
        }

        public static List<string> rechargeOrder()
        {
            //string rechargeOrderStr = IniHelper.INIGetStringValue(IniUtil.file,"AIDA", "rechargeOrder","");
            string rechargeOrderStr = "rechargeOrderID,rechargeCompaignID,memberID,rechargeWay,rechargeType,deposit,rechargeFee,adwardFee,state,posAccount,rechargeDate,dataVersion,rechargeSource,gid";
            string[] rechargeOrders = rechargeOrderStr.Split(',');

            List<String> list = IniUtil.arrayToList(rechargeOrders);
            return list;
        }

        public static List<string> goodsOrder()
        {
            //string rechargeOrderStr = IniHelper.INIGetStringValue(IniUtil.file,"AIDA", "rechargeOrder","");
            string column = "goodsOrderID,memberID,goodsSum,orderType,orderFee,couponDiscoutFee,paySum,basePayFee,state,createTime,source,areaId,machineID,actorUser,actTime,reason,returnActorUse,returnTime,authorise,dataVersion,gid,payWay,creator,goodsOrderDesc";
            string[] columns = column.Split(',');

            return IniUtil.arrayToList(columns);
        }

        public static List<string> billing()
        {
            //string billingStr = IniHelper.INIGetStringValue(IniUtil.file,"AIDA", "billing", "");
            string billingStr = "billingID,gid,tariffConfigID,ruleID,startTime,endTime,theDate,tariffDataVersion,tariffType,ratioCostBase,ratioCostAward,discount,periodStartTime,periodEndTime,currentCostBase,currentCostAward,currentCostTemp,allHadCost,ignoreTime,startPrice,hourPrice,wholeTimestamp,startCost,checkStart,periodOrder,memberID,roomOwner,additionalFee,onlineID";
            string[] billings = billingStr.Split(',');


            List<string> list = IniUtil.arrayToList(billings);
            return list;
        }

        public static List<string> duty()
        {
            //string dutyStr = IniHelper.INIGetStringValue(IniUtil.file,"AIDA","duty","");
            string dutyStr = "dutyID,shiftID,currentnetBarUserID,nextnetBarUserID,currentSum,currentCash,currentDeliver,currentReserve,dutyBeginTime,dutyEndTime,state,remark,dataVersion,gid,generateFrom,submitTime,dutyDate,totalIncome,totalConsume,totalAttendance,newMemberNum,turnOverRatio,onlineTimes,onlineMembers,internetTimes,adwardTotal";
            string[] dutys = dutyStr.Split(',');

            List<string> list = IniUtil.arrayToList(dutys);
            return list;
        }

        public static List<string> dutyConsume()
        {
            //string dutyConsumeStr = IniHelper.INIGetStringValue(IniUtil.file,"AIDA","dutyConsume","");
            string dutyConsumeStr = "consumeID,dutyID,revenueType,fee,dataVersion,gid,shiftID,dutyDate,dutyBeginTime,dutyEndTime,revenueTypeName,shiftName";
            string[] dutyConsumes = dutyConsumeStr.Split(',');

            List<string> list = IniUtil.arrayToList(dutyConsumes);
            return list;
        }

        public static List<string> dutyFund()
        {
            //string dutyFundStr = IniHelper.INIGetStringValue(IniUtil.file,"AIDA","dutyFund","");
            string dutyFundStr = "fundsID,revenueType,dutyId,fee,dataVersion,gid";
            string[] dutyFunds = dutyFundStr.Split(',');

            List<string> list = IniUtil.arrayToList(dutyFunds);
            return list;
        }

        public static List<string> dutyPayFund()
        {
            //string dutyPayFundStr = IniHelper.INIGetStringValue(IniUtil.file, "AIDA", "dutyPayFund","");
            string dutyPayFundStr = "fundsID,dutyID,payWay,fee,dataVersion,gid";
            string[] dutyPayFunds = dutyPayFundStr.Split(',');

            List<string> list = IniUtil.arrayToList(dutyPayFunds);
            return list;
        }

        public static List<string> online()
        {
            //string onlineStr = IniHelper.INIGetStringValue(IniUtil.file,"AIDA","online","");
            string onlineStr = "onlineID,tariffConfigID,memberID,onlineRoomID,machineID,areaID,memberType,ruleID,tariffType,ifRoomOwner,startUser,startCardTime,onlineStartTime,offLineTime,internetTime,onlineFee,state,gid,dataVersion,theDate,baseBalance,awardBalance,couponDeduction,awardReserve,baseReserve,payWay,endWay,actorUser,actTime,authoriseUser,tariffDataVersion,areaName,machineName";
            string[] onlines = onlineStr.Split(',');

            List<string> list = IniUtil.arrayToList(onlines);
            return list;
        }

        public static List<string> onlineRoom()
        {
            //string onlineRoomStr = IniHelper.INIGetStringValue(IniUtil.file,"AIDA", "onlineRoom","");
            string onlineRoomStr = "onlineRoomID,areaID,areaName,ruleID,state,payWay,startTime,endTime,startUser,endUser,gid,dataVersion,tariffType";
            string[] onlineRooms = onlineRoomStr.Split(',');

            List<string> list = IniUtil.arrayToList(onlineRooms);
            return list;
        }

        public static List<string> member()
        {
            //string memberStr = IniHelper.INIGetStringValue(IniUtil.file,"AIDA","member","");
            string memberStr = "memberID,account,uid,userId,memberName,birthday,sex,phone,qq,openID,memberType,state,creator,gid,certificateType,certificateNum,address,password,createDate";
            string[] members = memberStr.Split(',');

            List<string> list = IniUtil.arrayToList(members);
            return list;
        }

        public static List<string> memberAccount()
        {
            //string memberAccountStr = IniHelper.INIGetStringValue(IniUtil.file, "AIDA", "memberAccount", "");
            string memberAccountStr = "memberID,account,gid,userrPoint,totalBaseBalance,totalAwardBalance,baseBalance,awardBalance,deposit,lastUpdateTime";
            string[] memberAccounts = memberAccountStr.Split(',');

            List<string> list = IniUtil.arrayToList(memberAccounts);
            return list;
        }

        public static List<string> omitmember()
        {
            string memberStr = "memberID,account,uid,userId,memberName,birthday,sex,phone,qq,openID,memberType,state,creator,gid,certificateType,certificateNum,address,password,createDate,userrPoint,totalBaseBalance,totalAwardBalance,baseBalance,awardBalance,deposit,lastUpdateTime";
            string[] members = memberStr.Split(',');

            List<string> list = IniUtil.arrayToList(members);
            return list;
        }

        private static List<string> arrayToList(string[] arr)
        {
            List<string> list = new List<string>();
            if (arr == null)
            {
                return list;
            }

            foreach (string item in arr)
            {
                list.Add(item);
            }

            return list;
        }

    }
}
