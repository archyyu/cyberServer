using CashierLibrary.Model.Bill;
using CashierLibrary.Util;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CashierServer.DAO
{
    public class OnlineDao : BaseDao
    {


        public List<SurfPc> loadAllPc(UInt32 gid)
        {
            List<SurfPc> pcList = new List<SurfPc>();

            string sql = "select machineID,machineName,areaId,state,mac,ip,ipMask,gid from netbar_machine where state=1 and gid=" + gid;
            
            List<IDictionary<string, object>> list = this.selectList(sql);


            foreach (IDictionary<string, object> item in list)
            {
                SurfPc pc = new SurfPc();

                pc.AreaTypeId = UInt32.Parse(item["areaId"].ToString());
                pc.PcName = item["machineName"].ToString();
                pc.PcInfo = new PcInfo();
                pc.PcInfo.AreaId = int.Parse(item["areaId"].ToString());
                pc.PcInfo.Gid = gid;
                pc.PcInfo.Ip = item["ip"].ToString();
                pc.PcInfo.IpMask = item["ipMask"].ToString();
                pc.PcInfo.MachineId = int.Parse(item["machineID"].ToString());
                pc.PcInfo.MachineName = item["machineName"].ToString();
                pc.PcInfo.State = 1;
                pc.PcState = PcState.PC_ON_NOUSER;
                pc.PcHeartTime = 0;

                pcList.Add(pc);

            }

            return pcList;

        }
        

        public int updatePc(string pcName, string ip)
        {
            string sql = "update netbar_machine set ip='" + ip + "' where machineName='"+pcName+"'";
            return this.execute(sql);
        }


        public void updateOnlineId(Int64 onlineId)
        {
            //TODO
            string sql = "update netbar_online";
            this.execute(sql);
        }

        public int GetSeqRoomId()
        {
            List<MySqlParameter> parameters = new List<MySqlParameter>();

            MySqlParameter memberNameParameter = new MySqlParameter("@tableName_", MySqlDbType.VarChar, 30);
            memberNameParameter.Value = "seq_online_room";
            memberNameParameter.Direction = ParameterDirection.Input;
            parameters.Add(memberNameParameter);

            MySqlParameter result = new MySqlParameter("@result", MySqlDbType.Int32);
            result.Direction = ParameterDirection.Output;
            parameters.Add(result);

            CDbMysql mysql = this.getConnection();

            try
            {
                DataTable dt = mysql.ExcProcedure("getSeq", parameters.ToArray());
                if ((result.Value) != null && (Convert.ToInt32(result.Value) == 1))
                {
                    //dto.MemberId = Convert.ToDouble(dt.Rows[0]["memberID_"] == null ?"0": dt.Rows[0]["memberID_"].ToString());
                    return Convert.ToInt32(dt.Rows[0]["id"] == null ? "0" : dt.Rows[0]["id"].ToString());
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("GetSeqRoomId err:",ex);
            }
            finally
            {
                this.releaseConnection(mysql);
            }
            return 0;
        }

        public List<IDictionary<string,object>> loadAllOnlineUser(UInt32 gid)
        {
            try
            {
string sql = @"select 

    base.memberName,

    base.account,

    base.memberType,

    base.machineID,

    base.machineName,

    base.areaID,

    base.areaName,

    base.tariffType,

    base.onlineStartTime as onlineStartTime,

    base.onlineFee,

    base.deposit,

    base.baseBalance,

    base.awardBalance,

	nb.billingID, -- 计费ID

	nb.gid, -- 网吧GID

	nb.tariffConfigID, -- 网吧费率ID

	base.ruleID, -- 计费规则ID

    nb.tariffDataVersion, -- 费率版本号

	UNIX_TIMESTAMP(nb.startTime) as startTime, -- 开始时间

	nb.endTime, -- 结束时间

	nb.theDate, -- 所在日期

	nb.ratioCostBase, -- 本金扣费占比(数字5代表5:5；数字3代表3:7)

	nb.ratioCostAward, -- 充送扣费占比(数字5代表5:5；数字3代表3:7)

	nb.discount, -- 折扣

	nb.periodStartTime, -- 包时段起始时间

	nb.periodEndTime, -- 包时段结束时间

	UNIX_TIMESTAMP(nb.lastCostTimestamp) as lastCostTimestamp, -- 上次扣费时间

	UNIX_TIMESTAMP(nb.nextCostTimestamp) as nextCostTimestamp, -- 下次扣费时间

	UNIX_TIMESTAMP(nb.maxEndTimestamp) as maxEndTimestamp, -- 最大可上机时间

	nb.allHadCost, -- 本次上机累积扣费总额

	nb.ignoreTime, -- 忽略时间

	nb.startPrice, -- 起步价

	nb.hourPrice, -- 标准单价

	nb.wholeTimestamp, -- 整点标记

	nb.startCost, -- 已扣起步价

	nb.checkStart, -- 是否检查已扣起步价

	nb.periodOrder, -- 是否预约包时，缺省 FALSE

	base.memberID, -- 会员ID

	nb.roomOwner, -- 是否是包房主扣卡用户

	nb.onlineID, -- 上机id

    base.onlineRoomID

    from

    (select

    nme.memberID, -- 会员卡号

    nme.memberName, -- 会员姓名

    nme.account, -- 会员账号

    nme.memberType, -- 会员类型

    no.machineID, -- 开卡上机机器id

    no.machineName,-- 使用机器

    no.areaID, -- 开卡上机区域ID

    no.ruleID,

    no.areaName, -- 区域名称

    no.tariffType, -- 费率类型(1-标准计费，2-包时段，3-包时长，4-包间标准，5-包间包时段，6-包间包时长,)）

    UNIX_TIMESTAMP(no.onlineStartTime) as onlineStartTime, -- 上机时间

    no.internetTime, -- 上网时长

    no.onlineFee, -- 上网消费金额

    no.onlineRoomID,

	  no.onlineID,

    nmac.deposit, -- 临时卡显示押金余额

    nmac.baseBalance, -- 会员卡显示剩余本金余额

    nmac.awardBalance -- 会员卡显示剩余充送金余额

    from netbar_member nme,netbar_online no,netbar_member_account nmac 

    where nme.memberID = no.memberID and nmac.memberID = nme.memberID and no.offLineTime is null and no.gid = gid_) as base left join netbar_billing nb

    on base.memberID = nb.memberID and base.onlineID=nb.onlineID and nb.billingID = (select billingID from netbar_billing where memberID=base.memberID  and endTime is null  order by billingID desc limit 1)

    and nb.endTime is null and nb.gid =gid_";

                sql = sql.Replace("gid_",gid+"");
                
                List<IDictionary<string,object>> list = this.selectList(sql);
                return list;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("fetal loadAllOnlineUser error",ex);
                System.Environment.Exit(System.Environment.ExitCode);
            }
            return new List<IDictionary<string, object>>();
        }

        public bool UpdateOnline(SurfUser surfUser)
        {
            CDbMysql mysql = getConnection();
            try
            {
                
                List<MySqlParameter> parameters = new List<MySqlParameter>();

                MySqlParameter memberIdParameter = new MySqlParameter("@memberID_", MySqlDbType.UInt64);
                memberIdParameter.Value = surfUser.MemberId;
                memberIdParameter.Direction = ParameterDirection.Input;
                parameters.Add(memberIdParameter);

                MySqlParameter onlineStartTime_ = new MySqlParameter(@"onlineStartTime_", MySqlDbType.Int64);
                onlineStartTime_.Value = surfUser.LogonTimestamp;
                onlineStartTime_.Direction = ParameterDirection.Input;
                parameters.Add(onlineStartTime_);

                MySqlParameter offLineTime_ = new MySqlParameter("@offLineTime_", MySqlDbType.Int64);
                offLineTime_.Value = surfUser.LogoffTime;
                offLineTime_.Direction = ParameterDirection.Input;
                parameters.Add(offLineTime_);

                MySqlParameter onlineFee_ = new MySqlParameter("@onlineFee_", MySqlDbType.Float);
                onlineFee_.Value = surfUser.AllHadCost;
                onlineFee_.Direction = ParameterDirection.Input;
                parameters.Add(onlineFee_);

                MySqlParameter machineName_ = new MySqlParameter("@machineName_", MySqlDbType.VarChar, 30);
                machineName_.Value = surfUser.PcName;
                machineName_.Direction = ParameterDirection.Input;
                parameters.Add(machineName_);

                MySqlParameter state_ = new MySqlParameter("@state_", MySqlDbType.UInt16);
                state_.Value = surfUser.LogoffTime == 0 ? 1 : 2;
                state_.Direction = ParameterDirection.Input;
                parameters.Add(state_);

                MySqlParameter tariffConfigID = new MySqlParameter("@tariffConfigID_", MySqlDbType.Int32);
                tariffConfigID.Value = 0;
                tariffConfigID.Direction = ParameterDirection.Input;
                parameters.Add(tariffConfigID);

                MySqlParameter ruleID_ = new MySqlParameter("@ruleID_", MySqlDbType.Int32);
                ruleID_.Value = surfUser.RuleId;
                ruleID_.Direction = ParameterDirection.Input;
                parameters.Add(ruleID_);

                MySqlParameter tariffDataVersion = new MySqlParameter("@tariffDataVersion_", MySqlDbType.Int32);
                tariffDataVersion.Value = 0;
                tariffDataVersion.Direction = ParameterDirection.Input;
                parameters.Add(tariffDataVersion);

                MySqlParameter areaID = new MySqlParameter("@areaID_", MySqlDbType.Int32);
                areaID.Value = surfUser.AreaTypeId;
                areaID.Direction = ParameterDirection.Input;
                parameters.Add(areaID);


                MySqlParameter tariffType = new MySqlParameter("@tariffType_", MySqlDbType.Int32);
                tariffType.Value = (int)surfUser.CostType;
                tariffType.Direction = ParameterDirection.Input;
                parameters.Add(tariffType);

                MySqlParameter idseq = new MySqlParameter("@idseq", MySqlDbType.UInt64);
                idseq.Value = surfUser.RoomId;
                idseq.Direction = ParameterDirection.Input;
                parameters.Add(idseq);

                MySqlParameter gidParameter = new MySqlParameter("@gid_", MySqlDbType.UInt32);
                gidParameter.Value = surfUser.Gid;
                gidParameter.Direction = ParameterDirection.Input;
                parameters.Add(gidParameter);

                MySqlParameter result = new MySqlParameter("@result", MySqlDbType.Int32);
                result.Value = -1;
                result.Direction = ParameterDirection.Output;
                parameters.Add(result);

                DataTable dt = mysql.ExcProcedure("updateOnline", parameters.ToArray());

                if ((result.Value) != null && (Convert.ToInt32(result.Value) == 1))
                {
                    return true;
                }
                else
                {
                    LogHelper.WriteLog("call updateOnline error:\n获取到错误码" + result.Value.ToString());
                    return false;
                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("update online error", ex);
            }
            finally
            {
                this.releaseConnection(mysql);
            }

            return false;
        }

        public bool AddOnline(SurfUser surfUser)
        {

            // 获取序列号
            int roomidseq = this.GetSeqRoomId();

            CDbMysql mysql = getConnection();
            try
            {
                List<MySqlParameter> parameters = new List<MySqlParameter>();

                MySqlParameter memberIdParameter = new MySqlParameter("@memberID_", MySqlDbType.UInt64);
                memberIdParameter.Value = surfUser.MemberId;
                memberIdParameter.Direction = ParameterDirection.Input;
                parameters.Add(memberIdParameter);

                MySqlParameter memberTypeParameter = new MySqlParameter("@memberType_", MySqlDbType.UInt16);
                memberTypeParameter.Value = surfUser.MemberTypeId;
                memberTypeParameter.Direction = ParameterDirection.Input;
                parameters.Add(memberTypeParameter);

                MySqlParameter onlineRoomIdParameter = new MySqlParameter("@onlineRoomID_", MySqlDbType.UInt32);
                onlineRoomIdParameter.Value = surfUser.lOnlineRoomSeq;
                onlineRoomIdParameter.Direction = ParameterDirection.Input;
                parameters.Add(onlineRoomIdParameter);

                MySqlParameter ifRoomOwnerParameter = new MySqlParameter("@ifRoomOwner_", MySqlDbType.UInt16);
                ifRoomOwnerParameter.Value = 0;
                ifRoomOwnerParameter.Direction = ParameterDirection.Input;
                parameters.Add(ifRoomOwnerParameter);

                MySqlParameter startUserParameter = new MySqlParameter("@startUser_", MySqlDbType.UInt32);
                startUserParameter.Value = surfUser.CashierID;
                startUserParameter.Direction = ParameterDirection.Input;
                parameters.Add(startUserParameter);

                MySqlParameter gidParameter = new MySqlParameter("@gid_", MySqlDbType.UInt32);
                gidParameter.Value = surfUser.Gid;
                gidParameter.Direction = ParameterDirection.Input;
                parameters.Add(gidParameter);

                MySqlParameter payWay = new MySqlParameter("@payWay_", MySqlDbType.UInt16);
                payWay.Value = surfUser.PayWay;
                payWay.Direction = ParameterDirection.Input;
                parameters.Add(payWay);

                MySqlParameter tariffConfigID = new MySqlParameter("@tariffConfigID_", MySqlDbType.Int32);
                tariffConfigID.Value = 0;
                tariffConfigID.Direction = ParameterDirection.Input;
                parameters.Add(tariffConfigID);

                MySqlParameter tariffDataVersion = new MySqlParameter("@tariffDataVersion_", MySqlDbType.Int32);
                tariffDataVersion.Value = 0;
                tariffDataVersion.Direction = ParameterDirection.Input;
                parameters.Add(tariffDataVersion);

                MySqlParameter authoriseUser = new MySqlParameter("@authoriseUser_", MySqlDbType.Int32);
                authoriseUser.Value = 0;
                authoriseUser.Direction = ParameterDirection.Input;
                parameters.Add(authoriseUser);

                MySqlParameter tariffType = new MySqlParameter("@tariffType_", MySqlDbType.Int32);
                tariffType.Value = (int)surfUser.CostType;
                tariffType.Direction = ParameterDirection.Input;
                parameters.Add(tariffType);

                MySqlParameter areaID = new MySqlParameter("@areaID_", MySqlDbType.Int32);
                areaID.Value = surfUser.AreaTypeId;
                areaID.Direction = ParameterDirection.Input;
                parameters.Add(areaID);


                MySqlParameter idseq = new MySqlParameter("@idseq", MySqlDbType.UInt64);
                idseq.Value = roomidseq;
                idseq.Direction = ParameterDirection.Input;
                parameters.Add(idseq);

                // 新版本服务器添加ruleID,默认0,标准计费
                MySqlParameter ruleID = new MySqlParameter("@ruleID_", MySqlDbType.Int32);
                ruleID.Value = surfUser.RuleId;
                ruleID.Direction = ParameterDirection.Input;
                parameters.Add(ruleID);

                MySqlParameter result = new MySqlParameter("@result", MySqlDbType.Int32);
                result.Value = 1;
                result.Direction = ParameterDirection.Output;
                parameters.Add(result);

                //DataTable dt = mysql.ExcProcedure("addOnline", parameters.ToArray());
                DataTable dt = mysql.ExcProcedure("addOnlineNew", parameters.ToArray());
                if ((result.Value) != null && (Convert.ToInt32(result.Value) == 1))
                {
                    return true;
                }
                else
                {
                    LogHelper.WriteLog("call addOnline error:\n获取到错误码" + result.Value.ToString());
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("add online error:", ex);
            }
            finally
            {
                this.releaseConnection(mysql);
            }
            
            return false;

        }

        public bool UpdateOnlineByCrossArea(SurfUser surfUser)
        {

            // 获取序列号
            int roomidseq = this.GetSeqRoomId();

            CDbMysql mysql = getConnection();
            try
            {
                List<MySqlParameter> parameters = new List<MySqlParameter>();

                MySqlParameter memberIdParameter = new MySqlParameter("@memberID_", MySqlDbType.UInt64);
                memberIdParameter.Value = surfUser.MemberId;
                memberIdParameter.Direction = ParameterDirection.Input;
                parameters.Add(memberIdParameter);

                MySqlParameter offLineTime_ = new MySqlParameter("@changePcTime_", MySqlDbType.Int64);
                offLineTime_.Value = surfUser.LogoffTime;
                offLineTime_.Direction = ParameterDirection.Input;
                parameters.Add(offLineTime_);

                MySqlParameter onlineFee_ = new MySqlParameter("@onlineFee_", MySqlDbType.Float);
                onlineFee_.Value = surfUser.AllHadCost;
                onlineFee_.Direction = ParameterDirection.Input;
                parameters.Add(onlineFee_);

                MySqlParameter machineName_ = new MySqlParameter("@machineName_", MySqlDbType.VarChar, 30);
                machineName_.Value = surfUser.PcName;
                machineName_.Direction = ParameterDirection.Input;
                parameters.Add(machineName_);


                MySqlParameter ruleID_ = new MySqlParameter("@ruleID_", MySqlDbType.Int32);
                ruleID_.Value = surfUser.RuleId;
                ruleID_.Direction = ParameterDirection.Input;
                parameters.Add(ruleID_);


                MySqlParameter areaID = new MySqlParameter("@areaID_", MySqlDbType.Int32);
                areaID.Value = surfUser.AreaTypeId;
                areaID.Direction = ParameterDirection.Input;
                parameters.Add(areaID);


                MySqlParameter tariffType = new MySqlParameter("@tariffType_", MySqlDbType.Int32);
                tariffType.Value = (int)surfUser.CostType;
                tariffType.Direction = ParameterDirection.Input;
                parameters.Add(tariffType);

                MySqlParameter idseq = new MySqlParameter("@idseq", MySqlDbType.UInt64);
                idseq.Value = roomidseq;
                idseq.Direction = ParameterDirection.Input;
                parameters.Add(idseq);

                MySqlParameter gidParameter = new MySqlParameter("@gid_", MySqlDbType.UInt32);
                gidParameter.Value = surfUser.Gid;
                gidParameter.Direction = ParameterDirection.Input;
                parameters.Add(gidParameter);

                MySqlParameter result = new MySqlParameter("@result", MySqlDbType.Int32);
                result.Value = -1;
                result.Direction = ParameterDirection.Output;
                parameters.Add(result);

                DataTable dt = mysql.ExcProcedure("updateOnlineByCrossArea", parameters.ToArray());

                if ((result.Value) != null && (Convert.ToInt32(result.Value) == 1))
                {
                    return true;
                }
                else
                {
                    LogHelper.WriteLog("call updateOnlineByCrossArea error:\n获取到错误码" + result.Value.ToString());
                    return false;
                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("call updateOnlineByCrossArea error", ex);
            }
            finally
            {
                this.releaseConnection(mysql);
            }

            return false;

        }

        public void loadOnlineBillList(SurfUser surfUser)
        {
            string selectSql = string.Format("select billingID,gid,UNIX_TIMESTAMP(lastCostTimestamp) as lastCostTimestamp,currentCostBase,currentCostAward,currentCostTemp from netbar_billing where onlineID={0} and memberID={1} order by billingID desc",surfUser.OnlineID,surfUser.MemberId);

            List<IDictionary<string, object>> list = this.selectList(selectSql);
            if (list is null || list.Count == 0)
            {
                LogHelper.WriteLog("loadOnlineBillList is null");
                return;
            }

            foreach (IDictionary<string, object> item in list)
            {
                Bill bill = new Bill();

                try
                {

                    bill.gid = surfUser.Gid;
                    bill.billingID = Int32.Parse(item["billingID"].ToString());
                    bill.lastCostTimestamp = (UInt32)double.Parse(item["lastCostTimestamp"].ToString());
                    bill.memberID = surfUser.MemberId;
                    bill.currentCostBase = float.Parse(item["currentCostBase"].ToString());
                    bill.currentCostAward = float.Parse(item["currentCostAward"].ToString());
                    bill.currentCostTemp = float.Parse(item["currentCostTemp"].ToString());

                    surfUser.addBill(bill);
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("err",ex);
                }
            }

        }

        public bool DoCost(SurfUser surfUser)
        {
            CDbMysql mysql = getConnection();
            try
            {

                List<MySqlParameter> parameters = new List<MySqlParameter>();

                MySqlParameter memberID_ = new MySqlParameter(@"memberID_", MySqlDbType.UInt64);
                memberID_.Value = surfUser.MemberId;
                memberID_.Direction = ParameterDirection.Input;
                parameters.Add(memberID_);

                MySqlParameter gidParameter = new MySqlParameter("@gid_", MySqlDbType.UInt32);
                gidParameter.Value = surfUser.Gid;
                gidParameter.Direction = ParameterDirection.Input;
                parameters.Add(gidParameter);

                MySqlParameter startTime_ = new MySqlParameter(@"startTime_", MySqlDbType.Int64);
                startTime_.Value = surfUser.LogonTimestamp;
                startTime_.Direction = ParameterDirection.Input;
                parameters.Add(startTime_);

                MySqlParameter endTime_ = new MySqlParameter(@"endTime_", MySqlDbType.Int64);
                endTime_.Value = null;
                endTime_.Direction = ParameterDirection.Input;
                parameters.Add(endTime_);

                MySqlParameter tariffType_ = new MySqlParameter(@"tariffType_", MySqlDbType.Int32);
                tariffType_.Value = (int)surfUser.CostType;
                tariffType_.Direction = ParameterDirection.Input;
                parameters.Add(tariffType_);

                MySqlParameter ratioCostBase_ = new MySqlParameter(@"ratioCostBase_", MySqlDbType.Int16);
                ratioCostBase_.Value = surfUser.RatioCostBase;
                ratioCostBase_.Direction = ParameterDirection.Input;
                parameters.Add(ratioCostBase_);

                MySqlParameter ratioCostAward_ = new MySqlParameter(@"ratioCostAward_", MySqlDbType.Int16);
                ratioCostAward_.Value = surfUser.RatioCostAward;
                ratioCostAward_.Direction = ParameterDirection.Input;
                parameters.Add(ratioCostAward_);

                MySqlParameter discount_ = new MySqlParameter(@"discount_", MySqlDbType.Float);
                discount_.Value = surfUser.Discount;
                discount_.Direction = ParameterDirection.Input;
                parameters.Add(discount_);

                MySqlParameter periodStartTime_ = new MySqlParameter(@"periodStartTime_", MySqlDbType.Float);
                periodStartTime_.Value = surfUser.PeriodStartTime;
                periodStartTime_.Direction = ParameterDirection.Input;
                parameters.Add(periodStartTime_);

                MySqlParameter periodEndTime_ = new MySqlParameter(@"periodEndTime_", MySqlDbType.Float);
                periodEndTime_.Value = surfUser.PeriodEndTime;
                periodEndTime_.Direction = ParameterDirection.Input;
                parameters.Add(periodEndTime_);

                MySqlParameter lastCostTimestamp_ = new MySqlParameter(@"lastCostTimestamp_", MySqlDbType.Int64);
                lastCostTimestamp_.Value = surfUser.LastCostTimestamp;
                lastCostTimestamp_.Direction = ParameterDirection.Input;
                parameters.Add(lastCostTimestamp_);

                MySqlParameter nextCostTimestamp_ = new MySqlParameter(@"nextCostTimestamp_", MySqlDbType.Int64);
                nextCostTimestamp_.Value = surfUser.NextCostTimestamp;
                nextCostTimestamp_.Direction = ParameterDirection.Input;
                parameters.Add(nextCostTimestamp_);

                MySqlParameter maxEndTimestamp_ = new MySqlParameter(@"maxEndTimestamp_", MySqlDbType.Int64);
                maxEndTimestamp_.Value = surfUser.MaxEndTimestamp;
                maxEndTimestamp_.Direction = ParameterDirection.Input;
                parameters.Add(maxEndTimestamp_);

                MySqlParameter currentCostBase_ = new MySqlParameter(@"currentCostBase_", MySqlDbType.Float);
                currentCostBase_.Value = surfUser.CurrentCostBase;
                currentCostBase_.Direction = ParameterDirection.Input;
                parameters.Add(currentCostBase_);

                MySqlParameter currentCostAward_ = new MySqlParameter(@"currentCostAward_", MySqlDbType.Float);
                currentCostAward_.Value = surfUser.CurrentCostAward;
                currentCostAward_.Direction = ParameterDirection.Input;
                parameters.Add(currentCostAward_);

                MySqlParameter currentCostTemp_ = new MySqlParameter(@"currentCostTemp_", MySqlDbType.Float);
                currentCostTemp_.Value = surfUser.CurrentCostTemp;
                currentCostTemp_.Direction = ParameterDirection.Input;
                parameters.Add(currentCostTemp_);

                MySqlParameter allHadCost_ = new MySqlParameter(@"allHadCost_", MySqlDbType.Float);
                allHadCost_.Value = surfUser.AllHadCost;
                allHadCost_.Direction = ParameterDirection.Input;
                parameters.Add(allHadCost_);

                MySqlParameter ignoreTime_ = new MySqlParameter(@"ignoreTime_", MySqlDbType.Int32);
                ignoreTime_.Value = surfUser.IgnoreTime;
                ignoreTime_.Direction = ParameterDirection.Input;
                parameters.Add(ignoreTime_);

                MySqlParameter startPrice_ = new MySqlParameter(@"startPrice_", MySqlDbType.Float);
                startPrice_.Value = surfUser.StartPrice;
                startPrice_.Direction = ParameterDirection.Input;
                parameters.Add(startPrice_);

                MySqlParameter hourPrice_ = new MySqlParameter(@"hourPrice_", MySqlDbType.Float);
                hourPrice_.Value = surfUser.HourPrice;
                hourPrice_.Direction = ParameterDirection.Input;
                parameters.Add(hourPrice_);

                MySqlParameter wholeTimestamp_ = new MySqlParameter(@"wholeTimestamp_", MySqlDbType.Int32);
                wholeTimestamp_.Value = surfUser.WholeTimestamp;
                wholeTimestamp_.Direction = ParameterDirection.Input;
                parameters.Add(wholeTimestamp_);

                MySqlParameter startCost_ = new MySqlParameter(@"startCost_", MySqlDbType.Float);
                startCost_.Value = surfUser.StartPrice;
                startCost_.Direction = ParameterDirection.Input;
                parameters.Add(startCost_);

                MySqlParameter checkStart_ = new MySqlParameter(@"checkStart_", MySqlDbType.Int16);
                checkStart_.Value = 0;
                checkStart_.Direction = ParameterDirection.Input;
                parameters.Add(checkStart_);

                MySqlParameter periodOrder_ = new MySqlParameter(@"periodOrder_", MySqlDbType.Int16);
                periodOrder_.Value = 0;
                periodOrder_.Direction = ParameterDirection.Input;
                parameters.Add(periodOrder_);

                MySqlParameter roomOwner_ = new MySqlParameter(@"roomOwner_", MySqlDbType.Int16);
                roomOwner_.Value = surfUser.RoomOwner ? 1 : 0;
                roomOwner_.Direction = ParameterDirection.Input;
                parameters.Add(roomOwner_);

                MySqlParameter additionalFee_ = new MySqlParameter(@"additionalFee_", MySqlDbType.Float);
                additionalFee_.Value = surfUser.ExtraCharge;
                additionalFee_.Direction = ParameterDirection.Input;
                parameters.Add(additionalFee_);

                MySqlParameter tariffConfigID_ = new MySqlParameter(@"tariffConfigID_", MySqlDbType.Int32);
                tariffConfigID_.Value = 0;
                tariffConfigID_.Direction = ParameterDirection.Input;
                parameters.Add(tariffConfigID_);

                MySqlParameter ruleID_ = new MySqlParameter(@"ruleID_", MySqlDbType.Int32);
                ruleID_.Value = surfUser.RuleId;
                ruleID_.Direction = ParameterDirection.Input;
                parameters.Add(ruleID_);

                MySqlParameter tariffDataVersion_ = new MySqlParameter(@"tariffDataVersion_", MySqlDbType.Int32);
                tariffDataVersion_.Value = 0;
                tariffDataVersion_.Direction = ParameterDirection.Input;
                parameters.Add(tariffDataVersion_);

                MySqlParameter result = new MySqlParameter(@"result", MySqlDbType.Int32);
                result.Value = 1;
                result.Direction = ParameterDirection.Output;
                parameters.Add(result);

                DataTable dt = mysql.ExcProcedure("addBilling", parameters.ToArray());
                if ((result.Value) != null && (Convert.ToInt32(result.Value) == 1))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("err", ex);
            }
            finally
            {
                this.releaseConnection(mysql);
            }

            return false;
        }


        public int UpdateBilling(SurfUser surfUser)
        {
            string sql = string.Format("update netbar_billing set nextCostTimestamp='{1}', maxEndTimestamp='{2}' where onlineID=(select onlineID from netbar_online where memberID={0} and offLineTime is null limit 1) and memberID={0} order by billingID desc limit 1", surfUser.MemberId, DateUtil.GetFormatByTime10(surfUser.NextCostTimestamp.ToString()), DateUtil.GetFormatByTime10(surfUser.MaxEndTimestamp.ToString()));
            LogHelper.WriteLog("update billing execute sql : " + sql);
            return this.execute(sql);
        }

    }
}
