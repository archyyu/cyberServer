using CashierLibrary.Model;
using CashierLibrary.Model.Bill;
using CashierLibrary.Util;
using CashierServer.DAO;
using CashierServer.Model;
using CashierServer.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace CashierServer.Logic.Surf
{
    public class SurfLogic
    {

        private IDictionary<UInt64, SurfUser> surfUserMap = new ConcurrentDictionary<UInt64, SurfUser>();

        private IDictionary<string, SurfPc> surfPcMap = new ConcurrentDictionary<string, SurfPc>();

        private IDictionary<int, string> memberTypeMap = new ConcurrentDictionary<int, string>();

        private IDictionary<UInt32, AreaItem> areaTypeMap = new ConcurrentDictionary<UInt32, AreaItem>();

        public BillingRate billingRate = new BillingRate();

        private MemberDao memberDao = new MemberDao();

        private RechargeOrderDao rechargeOrderDao = new RechargeOrderDao();

        private DutyDao dutyDao = new DutyDao();

        private OnlineDao onlineDao = new OnlineDao();

        private ZeroMqService zeroMqService = new ZeroMqService();
        
        public UInt32 gid { get; set; }

        public UInt32 state { get; set; } // 0: 正常模式  1：停电模式，不扣费

        public MainForm mainForm { get; set; }

        public SurfLogic()
        {

        }

        public bool InitSurfLogic(string rateInfo)
        {
            this.AnalyzeRateInfo(rateInfo);
            LogHelper.WriteLog("解析费率");
            this.loadAllPc();
            LogHelper.WriteLog("加载所有机器");
            this.loadOnlineUser();
            LogHelper.WriteLog("加载所有在线会员");
            return true;
        }

        private void AnalyzeRateInfo(string rateInfo)
        {
            try
            {
                JObject json = JObject.Parse(rateInfo);

                billingRate.gid = int.Parse(json["gid"].ToString());
                billingRate.GirlRate = float.Parse(json["data"]["girlDiscount"].ToString());
                billingRate.NeedActive = UInt32.Parse(json["data"]["needActive"].ToString());
                billingRate.SubmitAction = UInt32.Parse(json["data"]["shutdownOrRestart"].ToString());
                billingRate.DurationAction = UInt32.Parse(json["data"]["durationAction"].ToString());
                billingRate.LockTime = UInt32.Parse(json["data"]["lockTime"].ToString());
                billingRate.ActiveTime = UInt32.Parse(json["data"]["activeTime"].ToString());
                billingRate.ReLogin = UInt32.Parse(json["data"]["reLogin"].ToString());
                billingRate.isChain = Int32.Parse(json["data"]["isChain"].ToString());
                
                billingRate.name = json["data"]["shopName"].ToString();

                try
                {
                    billingRate.RatioBase = UInt16.Parse(json["data"]["deductionFee"]["ratioBase"].ToString());
                    billingRate.RatioAward = UInt16.Parse(json["data"]["deductionFee"]["ratioAward"].ToString());

                    billingRate.cashierflag = Int32.Parse(json["data"]["cashierflag"].ToString());
                }
                catch (Exception)
                {
                    billingRate.RatioBase = 1;
                    billingRate.RatioAward = 1;
                }

                try
                {

                    string memberId = json["data"]["memberID"].ToString();

                    memberId = memberId.Remove(0,memberId.Length - 7);

                    billingRate.memberId = Int64.Parse(memberId);
                    this.memberDao.updateMaxMemberID(billingRate.memberId);

                }
                catch (Exception)
                {

                }

                //try
                //{ 
                //    billingRate.onlineId = Int64.Parse(json["data"]["onlineID"].ToString());
                //    this.onlineDao.updateOnlineId(billingRate.onlineId);
                //}
                //catch (Exception)
                //{

                //}

                //try
                //{
                //    billingRate.dutyId = Int64.Parse(json["data"]["dutyID"].ToString());
                //    this.dutyDao.updateDutyId(billingRate.dutyId);
                //}
                //catch (Exception)
                //{

                //}

                //try
                //{
                //    string rechargeOrderId = json["data"]["rechargeOrderID"].ToString();

                //    rechargeOrderId = rechargeOrderId.Remove(0,rechargeOrderId.Length - 8);
                    
                //    billingRate.rechargeOrderId = Int64.Parse(rechargeOrderId);
                //    this.rechargeOrderDao.updateRechargeOnlineId(billingRate.rechargeOrderId);
                //}
                //catch (Exception)
                //{

                //}

               

                //会员等级

                this.memberTypeMap.Clear();
                {
                    JArray memberTypeArr = JArray.Parse(json["data"]["memberTypes"].ToString());
                    foreach (JObject item in memberTypeArr)
                    {
                        int memberTypeId = int.Parse(item["memberTypeId"].ToString());
                        string memberTypeName = item["memberTypeName"].ToString();
                        this.memberTypeMap[memberTypeId] = memberTypeName;
                    }
                }

                this.areaTypeMap.Clear();
                {
                    JArray areaConfig = JArray.Parse(json["data"]["areaConfig"].ToString());
                    foreach (JObject area in areaConfig)
                    {
                        AreaItem item = new AreaItem();
                        item.AreaId = UInt32.Parse(area["areaId"].ToString());
                        item.RoomType = UInt32.Parse(area["tariffRoomType"].ToString());
                        item.AreaName = area["areaName"].ToString();
                        item.memberTypeList = area["memberTypeList"].ToString();

                        this.areaTypeMap[item.AreaId] = item;
                    }
                }

                //weekprice
                {
                    JArray weekPrices = JArray.Parse(json["data"]["tariffConfig"]["standTariff"].ToString());

                    foreach (JObject item in weekPrices)
                    {
                        WeekPrice weekPrice = new WeekPrice();
                        weekPrice.Price = new float[7, 24];
                        weekPrice.RuleId = UInt32.Parse(item["ruleId"].ToString());
                        weekPrice.AreaId = UInt32.Parse(item["areaId"].ToString());
                        weekPrice.MemberType = UInt32.Parse(item["memberType"].ToString());

                        try
                        {
                            weekPrice.IgnoreTime = UInt32.Parse(item["ignoreTime"].ToString());
                        }
                        catch (Exception)
                        {
                            weekPrice.IgnoreTime = UInt32.MaxValue;
                        }

                        weekPrice.StartPrice = float.Parse(item["startPrice"].ToString());
                        weekPrice.MinCostPrice = float.Parse(item["minCostPrice"].ToString());

                        JArray week = JArray.Parse(item["price"].ToString());
                        for (int index = 0; index < week.Count; index++)
                        {
                            JArray day = JArray.Parse(week[index].ToString());
                            for (int j = 0; j < day.Count; j++)
                            {
                                weekPrice.Price[index, j] = float.Parse(day[j].ToString());
                            }
                        }

                        if (UInt32.Parse(item["tariffRoomType"].ToString()) == 2)
                        {
                            this.billingRate.weekPrices.Add(weekPrice);
                        }

                    }
                }

                //period price
                this.billingRate.PeriodHourPrices.Clear();
                {
                    JArray periodArr = JArray.Parse(json["data"]["tariffConfig"]["periodTariff"].ToString());

                    foreach (JObject item in periodArr)
                    {
                        PeriodPrice periodPrice = new PeriodPrice();

                        periodPrice.RuleId = UInt32.Parse(item["ruleId"].ToString());
                        periodPrice.AreaId = UInt32.Parse(item["areaId"].ToString());
                        periodPrice.MemberType = UInt32.Parse(item["memberType"].ToString());
                        periodPrice.Price = float.Parse(item["price"].ToString());
                        periodPrice.ByType = UInt32.Parse(item["byType"].ToString());
                        periodPrice.StartTime = UInt32.Parse(item["hsTime"].ToString()) + float.Parse(item["msTime"].ToString()) / 60;
                        periodPrice.EndTime = UInt32.Parse(item["heTime"].ToString()) + float.Parse(item["meTime"].ToString()) / 60;
                        periodPrice.PeriodTime = UInt32.Parse(item["durationTime"].ToString());

                        this.billingRate.PeriodHourPrices.Add(periodPrice);
                    }
                }

                //duration price
                {
                    this.billingRate.DurationHourPrices.Clear();
                    JArray durationArr = JArray.Parse(json["data"]["tariffConfig"]["durationTariff"].ToString());
                    foreach (JObject item in durationArr)
                    {
                        DurationPrice durationPrice = new DurationPrice();

                        durationPrice.RuleId = UInt32.Parse(item["ruleId"].ToString());
                        durationPrice.AreaId = UInt32.Parse(item["areaId"].ToString());
                        durationPrice.MemberType = UInt32.Parse(item["memberType"].ToString());
                        durationPrice.DurationTime = UInt32.Parse(item["durationTime"].ToString());
                        durationPrice.Price = float.Parse(item["price"].ToString());

                        this.billingRate.DurationHourPrices.Add(durationPrice);
                    }
                }

                //additional price
                {
                    this.billingRate.ExtraPrices.Clear();
                    JArray extraArr = JArray.Parse(json["data"]["tariffConfig"]["additionalTariff"].ToString());
                    foreach (JObject item in extraArr)
                    {
                        try
                        {
                            ExtraPrice extraPrice = new ExtraPrice();
                            extraPrice.RuleId = UInt32.Parse(item["ruleId"].ToString());
                            extraPrice.AreaTypeId = UInt32.Parse(item["areaId"].ToString());
                            extraPrice.MemberTypeId = UInt32.Parse(item["memberType"].ToString());
                            extraPrice.AdditionalPrice = float.Parse(item["additionalFee"].ToString());

                            this.billingRate.ExtraPrices.Add(extraPrice);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteLog("", ex);
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("analyze rate info error", ex);
            }

        }

        public void loadAllPc()
        {
            List<SurfPc> list = this.onlineDao.loadAllPc(this.gid);
            foreach (SurfPc pc in list)
            {
                this.surfPcMap.Add(pc.PcName, pc);
            }
        }

        public bool loadOnlineUser()
        {

            List<IDictionary<string, object>> list = this.onlineDao.loadAllOnlineUser(this.gid);
            UInt32 now = this.GetCurrentTimestamp();
            foreach (IDictionary<string, object> onlineItem in list)
            {
                SurfUser surfUser = new SurfUser();

                surfUser.Gid = (UInt32)this.billingRate.gid;
                surfUser.MemberId = UInt64.Parse(onlineItem["memberID"].ToString());
                surfUser.Account = onlineItem["account"].ToString();

                IDictionary<string, object> memberInfo = this.memberDao.queryMemberInfo(surfUser.MemberId);

                surfUser.Sex = UInt16.Parse(memberInfo["sex"].ToString());
                surfUser.ActiveTime = now;

                if (surfUser.Sex == SexType.female)
                {
                    surfUser.Discount = this.billingRate.GirlRate;
                }
                else
                {
                    surfUser.Discount = 1.0f;
                }

                this.resetSurfUser(surfUser, memberInfo);

                surfUser.MemberId = surfUser.MemberId;
                surfUser.RoomOwner = false;

                try
                {
                    surfUser.OnlineID = UInt32.Parse(onlineItem["onlineID"].ToString());
                }
                catch (Exception)
                {

                }

                try
                {
                    surfUser.LogonTimestamp = UInt32.Parse(onlineItem["onlineStartTime"].ToString());
                    surfUser.WholeTimestamp = surfUser.LogonTimestamp;
                }
                catch (Exception)
                {
                    surfUser.LogonTimestamp = 0;
                }

                try
                {
                    surfUser.LastCostTimestamp = (UInt32)double.Parse(onlineItem["lastCostTimestamp"].ToString());
                }
                catch (Exception)
                {
                    surfUser.LastCostTimestamp = 0;
                }

                try
                {
                    surfUser.NextCostTimestamp = (UInt32)double.Parse(onlineItem["nextCostTimestamp"].ToString());
                }
                catch (Exception)
                {
                    surfUser.NextCostTimestamp = 0;
                }

                try
                {
                    surfUser.MaxEndTimestamp = (UInt32)double.Parse(onlineItem["maxEndTimestamp"].ToString());
                }
                catch (Exception)
                {
                    surfUser.MaxEndTimestamp = 0;
                }

                try
                {
                    surfUser.AllHadCost = float.Parse(onlineItem["allHadCost"].ToString());
                }
                catch (Exception)
                {
                    surfUser.AllHadCost = 0;
                }

                try
                {
                    surfUser.IgnoreTime = UInt32.Parse(onlineItem["ignoreTime"].ToString());
                }
                catch (Exception)
                {
                    surfUser.IgnoreTime = 0;
                }

                try
                {
                    surfUser.StartPrice = float.Parse(onlineItem["startPrice"].ToString());
                }
                catch (Exception)
                {
                    surfUser.StartPrice = 0;
                }

                try
                {
                    surfUser.HourPrice = float.Parse(onlineItem["hourPrice"].ToString());
                }
                catch (Exception)
                {
                    surfUser.HourPrice = 0;
                }

                try
                {
                    surfUser.StartPrice = float.Parse(onlineItem["startCost"].ToString());
                }
                catch (Exception)
                {
                    surfUser.StartPrice = 0;
                }

                try
                {
                    surfUser.lOnlineRoomSeq = long.Parse(onlineItem["onlineRoomID"].ToString());
                }
                catch (Exception)
                {
                    surfUser.lOnlineRoomSeq = 0;
                }

                surfUser.AreaTypeId = UInt32.Parse(onlineItem["areaID"].ToString());

                SurfPc surfPc = this.FindPc(onlineItem["machineName"].ToString());
                if (surfPc != null)
                {
                    this.SurfUserBindPc(surfUser, surfPc);
                    surfPc.PcHeartTime = GetCurrentTimestamp();
                }

                surfUser.BaseBalance = float.Parse(memberInfo["baseBalance"].ToString());
                surfUser.AwardBalance = float.Parse(memberInfo["awardBalance"].ToString());
                surfUser.TempBalance = float.Parse(memberInfo["cashBalance"].ToString());

                try
                {
                    surfUser.RuleId = UInt32.Parse(onlineItem["ruleID"].ToString());
                }
                catch (Exception)
                {
                    surfUser.RuleId = 0;
                }
                surfUser.CostType = (CostType)(int.Parse(onlineItem["tariffType"].ToString()));

                surfUser.RatioCostBase = this.billingRate.RatioBase;
                surfUser.RatioCostAward = this.billingRate.RatioAward;



                if (surfUser.CostType == CostType.COST_TYPE_WEEK)
                {

                    WeekPrice weekPrice = this.FindWeekItem(surfUser.MemberTypeId, surfUser.AreaTypeId);
                    if (weekPrice != null)
                    {
                        surfUser.RuleId = weekPrice.RuleId;
                        surfUser.IgnoreTime = weekPrice.IgnoreTime;
                        surfUser.StartPrice = weekPrice.StartPrice;
                        surfUser.MinCostPrice = weekPrice.MinCostPrice;
                        surfUser.HourPrice = weekPrice.HourPrice(now);
                        surfUser.MaxEndTimestamp = this.CalculateMaxEndTime(surfUser);

                        ExtraPrice extraPrice = this.FindExtraPrice(surfUser.MemberTypeId, surfUser.AreaTypeId);
                        if (extraPrice != null)
                        {
                            surfUser.ExtraCharge = extraPrice.AdditionalPrice;
                        }

                    }
                }
                else if (surfUser.CostType == CostType.COST_TYPE_PERIOD)
                {
                    PeriodPrice periodPrice = this.FindPeriodItem(surfUser.RuleId);
                    if (periodPrice == null)
                    {
                        continue;
                    }

                    surfUser.RuleValue = periodPrice.Price;
                    surfUser.DurationTime = periodPrice.PeriodTime;

                }
                else if (surfUser.CostType == CostType.COST_TYPE_DURATION)
                {
                    DurationPrice durationPrice = this.FindDurationItem(surfUser.RuleId);
                    if (durationPrice == null)
                    {
                        continue;
                    }

                    surfUser.RuleValue = durationPrice.Price;
                    surfUser.DurationTime = durationPrice.DurationTime;

                }

                LogHelper.WriteLog("load user:" + surfUser.ToString());

                this.onlineDao.loadOnlineBillList(surfUser);

                this.AddSurfUser(surfUser);

            }

            return true;
        }

        public SurfUser ActiveUser(ActiveData activeData, ref string responsejson, bool notifyCashier)
        {
            IDictionary<string, object> root = new Dictionary<string, object>();
            bool activeNew = false;
            UInt32 now = this.GetCurrentTimestamp();
            SurfUser surfUser = this.FindSurfUser(activeData.MemberId);
            SurfPc surfPc = null;
            if (surfUser == null)
            {
                IDictionary<string, object> info = this.memberDao.queryMemberInfo(activeData.MemberId);

                if (info == null)
                {
                    root = this.GetErrMap(ErrDefine.STR_USER_NOTEXIST);
                    responsejson = JsonUtil.SerializeObject(root);
                    return null;
                }

                surfUser = new SurfUser();

                surfUser.MemberId = activeData.MemberId;
                surfUser.PcName = activeData.PcName;
                surfUser.CashierID = activeData.CashierId;
                surfUser.AreaTypeId = activeData.AreaTypeId;
                this.resetSurfUser(surfUser, info);

                surfUser.Gid = this.gid;
                surfUser.ActiveTime = this.GetCurrentTimestamp();


                surfUser.BaseBalance = float.Parse(info["baseBalance"].ToString());
                surfUser.AwardBalance = float.Parse(info["awardBalance"].ToString());
                surfUser.TempBalance = float.Parse(info["cashBalance"].ToString());
                surfUser.CostType = (CostType)activeData.CostType;

                surfUser.RuleId = activeData.RuleId;
                surfUser.RoomOwner = false;

                if (surfUser.Sex == 1)
                {
                    surfUser.Discount = this.billingRate.GirlRate;
                }
                else
                {
                    surfUser.Discount = 1;
                }

                surfUser.RatioCostBase = this.billingRate.RatioBase;
                surfUser.RatioCostAward = this.billingRate.RatioAward;

                activeNew = true;

                UInt32 curTime = this.GetCurrentTimestamp();

                if (activeData.CostType == CostType.COST_TYPE_WEEK)
                {

                }
                else if (activeData.CostType == CostType.COST_TYPE_PERIOD)
                {
                    PeriodPrice period = this.FindPeriodItem(surfUser.RuleId);
                    if (period == null)
                    {
                        responsejson = this.GetErrStr(ErrDefine.STR_ERROR_PARAMS);
                        return null;
                    }

                    if (period.isIn(now) == false)
                    {
                        responsejson = this.GetErrStr(ErrDefine.STR_ERROR_PERIODTIME);
                        return null;
                    }

                    if (surfUser.remain() < period.Price)
                    {
                        responsejson = this.GetErrStr(ErrDefine.STR_ERROR_NOMONEY);
                        return null;
                    }

                    if (surfUser.AreaTypeId != period.AreaId)
                    {
                        responsejson = this.GetErrStr(ErrDefine.STR_ERROR_PARAMS);
                        return null;
                    }

                    ExtraPrice extraPrice = this.FindExtraPrice(surfUser.MemberTypeId, period.AreaId);
                    if (extraPrice != null)
                    {
                        surfUser.ExtraCharge = extraPrice.AdditionalPrice;
                    }

                    surfUser.RuleValue = period.Price;
                    surfUser.PeriodStartTime = period.StartTime;
                    surfUser.PeriodEndTime = period.EndTime;
                    surfUser.DurationTime = period.PeriodTime;

                }
                else if (activeData.CostType == CostType.COST_TYPE_DURATION)
                {
                    DurationPrice duration = this.FindDurationItem(surfUser.RuleId);
                    if (duration == null)
                    {
                        responsejson = this.GetErrStr(ErrDefine.STR_ERROR_PARAMS);
                        return null;
                    }

                    if (surfUser.remain() < duration.Price)
                    {
                        responsejson = this.GetErrStr(ErrDefine.STR_ERROR_NOMONEY);
                        return null;
                    }

                    if (surfUser.AreaTypeId != duration.AreaId)
                    {
                        responsejson = this.GetErrStr(ErrDefine.STR_ERROR_PARAMS);
                        return null;
                    }

                    ExtraPrice extraPrice = this.FindExtraPrice(surfUser.MemberTypeId, duration.AreaId);
                    if (extraPrice != null)
                    {
                        surfUser.ExtraCharge = extraPrice.AdditionalPrice;
                    }
                    surfUser.RuleValue = duration.Price;
                    surfUser.DurationTime = duration.DurationTime;
                }
            }
            else
            {
                // 标准转包时
                if (surfUser.CostType == CostType.COST_TYPE_WEEK && activeData.CostType != CostType.COST_TYPE_WEEK)
                {
                    // 检测区域
                    if (surfUser.AreaTypeId != 0 && surfUser.AreaTypeId != activeData.AreaTypeId)
                    {
                        responsejson = this.GetErrStr(ErrDefine.STR_ERROR_AREA_MATCH);
                        return null;
                    }

                    // 激活状态
                    if (surfUser.AreaTypeId == 0)
                    {
                        this.WeekToPeriodOrDurationByActive(activeData, ref responsejson);
                    }
                    else
                    {
                        this.WeekToPeriodOrDuration(activeData, ref responsejson);
                    }
                    return null;
                }
            }

            bool result = false;

            if (activeNew == true)
            {
                result = this.onlineDao.AddOnline(surfUser);
            }
            else
            {
                result = this.onlineDao.UpdateOnline(surfUser);
            }

            if (activeData.PcName != "" && surfUser.CostType == CostType.COST_TYPE_WEEK)
            {

                surfPc = this.FindPc(activeData.PcName);
                if (surfPc == null)
                {
                    root = GetErrMap(ErrDefine.STR_PC_NOTFOUND);
                    responsejson = JsonUtil.SerializeObject(root);
                    return null;
                }
                WeekPrice weekPrice = this.FindWeekItem(surfUser.MemberTypeId, surfPc.AreaTypeId);
                if (weekPrice == null)
                {
                    root = GetErrMap(ErrDefine.STR_NO_BILLING);
                    responsejson = JsonUtil.SerializeObject(root);
                    return null;
                }

                weekPrice.resetSurfUser(surfUser, surfPc, now);
                this.SurfUserBindPc(surfUser, surfPc);
                surfUser.LogonTimestamp = now;
                surfUser.WholeTimestamp = now;
                surfUser.NextCostTimestamp = surfUser.LogonTimestamp + surfUser.IgnoreTime;

                this.onlineDao.UpdateOnline(surfUser);

            }


            if (result == true)
            {
                this.AddSurfUser(surfUser);

                root = GetSuccessMap();
                JObject userJson = new JObject();
                this.GetSurfUserJson(surfUser, surfPc, ref userJson);
                root["data"] = userJson;
                root["business"] = NotifyDefine.NOTIFY_UPT_USER;

                if (surfPc != null)
                {
                    root["subBn"] = NotifyDefine.NOTIFY_UPT_SN_USERLOGON;
                }
                else
                {
                    root["subBn"] = NotifyDefine.NOTIFY_UPT_SN_USERACTIVE;
                }

                this.PublishChangeInfo(JsonUtil.SerializeObject(root), surfUser, surfPc, false, true);

            }
            else
            {
                root = GetErrMap(ErrDefine.STR_DB_ERROR);
            }

            responsejson = JsonUtil.SerializeObject(root);

            LogHelper.WriteLog("active:" + responsejson);

            return surfUser;
        }

        private void resetSurfUser(SurfUser surfUser, IDictionary<string, object> info)
        {

            if (surfUser == null)
            {
                return;
            }

            try
            {

                surfUser.MemberId = UInt64.Parse(info["memberID"].ToString());
                surfUser.Account = info["account"].ToString();

                surfUser.Sex = UInt32.Parse(info["sex"].ToString());
                surfUser.Birthday = info["birthday"].ToString();
                surfUser.ProvinceId = 0;
                surfUser.CityId = 0;
                surfUser.DistrictID = 0;
                surfUser.Address = "未知";
                surfUser.MemberTypeId = UInt32.Parse(info["memberType"].ToString());
                surfUser.MemberTypeName = memberTypeMap[(int)surfUser.MemberTypeId];
                surfUser.Phone = info["phone"].ToString();
                surfUser.QQ = info["qq"].ToString();
                surfUser.OpenID = info["openID"].ToString();
                surfUser.MemberName = info["memberName"].ToString();

                surfUser.Password = info["password"].ToString();
                surfUser.BaseBalance = float.Parse(info["baseBalance"].ToString());
                surfUser.AwardBalance = float.Parse(info["awardBalance"].ToString());
                surfUser.TempBalance = float.Parse(info["cashBalance"].ToString());

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("reset surfUser error", ex);
            }


        }

        private string FindAreaName(UInt32 areaId)
        {
            AreaItem areaItem;
            this.areaTypeMap.TryGetValue(areaId, out areaItem);

            if (areaItem == null)
            {
                return "";
            }

            return areaItem.AreaName;

        }

        private List<PeriodPrice> FindSmartPeriod(UInt32 memberTypeId, UInt32 areaId)
        {
            List<PeriodPrice> list = new List<PeriodPrice>();
            foreach (PeriodPrice item in this.billingRate.PeriodHourPrices)
            {
                if (item.ByType == 2 && item.MemberType == memberTypeId && item.AreaId == areaId)
                {
                    list.Add(item);
                }
            }
            return list;
        }

        private SurfPc FindPc(string name)
        {
            if (name == null || name == "")
            {
                return null;
            }
            SurfPc surfPc = null;
            this.surfPcMap.TryGetValue(name, out surfPc);
            return surfPc;
        }

        private AreaItem FindDefaultArea()
        {
            foreach (AreaItem item in this.areaTypeMap.Values)
            {
                if (item.AreaName == "默认区域")
                {
                    return item;
                }
            }
            return null;
        }

        private SurfUser FindSurfUser(UInt64 memberId)
        {
            SurfUser surfUser = null;
            this.surfUserMap.TryGetValue(memberId, out surfUser);
            return surfUser;
        }

        private void AddSurfUser(SurfUser surfUser)
        {
            this.surfUserMap[surfUser.MemberId] = surfUser;
        }

        private void RemoveSurfUser(UInt64 memberId)
        {
            this.surfUserMap.Remove(memberId);
        }

        private UInt32 GetCurrentTimestamp()
        {
            return Convert.ToUInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
        }

        public void PcHeart(UInt64 memberId, SurfPc surfPc, ref string responseJson)
        {
            SurfPc pc = this.FindPc(surfPc.PcName);
            if (pc == null)
            {

                AreaItem item = this.FindDefaultArea();

                if (item != null)
                {
                    this.surfPcMap[surfPc.PcName] = surfPc;
                    pc = surfPc;
                    pc.AreaTypeId = item.AreaId;
                    pc.PcInfo.AreaId = (int)item.AreaId;
                }

                this.surfPcMap.Add(pc.PcName, pc);
            }
            else
            {
                if (pc.PcInfo.Ip != surfPc.PcInfo.Ip)
                {
                    this.onlineDao.updatePc(surfPc.PcName, surfPc.PcInfo.Ip);
                }

                pc.PcInfo.Ip = surfPc.PcInfo.Ip;
            }

            pc.PcHeartTime = this.GetCurrentTimestamp();
            UInt64 existMemberId = 0;

            IDictionary<string, object> root = GetSuccessMap();
            IDictionary<string, object> data = new Dictionary<string, object>();

            data["tm"] = pc.PcHeartTime;
            data["gid"] = this.billingRate.gid;
            data["barName"] = this.billingRate.name;
            data["existMemberId"] = existMemberId;
            data["lockTime"] = this.billingRate.LockTime;
            data["reLogin"] = this.billingRate.ReLogin;
            data["ShutdownOrRestart"] = this.billingRate.SubmitAction;

            root["data"] = data;
            responseJson = JsonUtil.SerializeObject(root);

        }

        private IDictionary<string, object> GetSuccessMap()
        {
            IDictionary<string, object> root = new Dictionary<string, object>();
            root["state"] = 0;
            root["info"] = "成功";
            return root;
        }

        private string GetSuccessStr()
        {
            return JsonUtil.SerializeObject(this.GetSuccessMap());
        }

        private IDictionary<string, object> GetErrMap(string err)
        {
            IDictionary<string, object> root = new Dictionary<string, object>();
            root["state"] = -1;
            root["info"] = err;
            return root;
        }

        private string GetErrStr(string err)
        {
            return JsonUtil.SerializeObject(this.GetErrMap(err));
        }

        public void GetSurfUserJson(SurfUser surfUser, SurfPc surfPc, ref JObject json)
        {
            if (surfUser != null)
            {
                json["account"] = surfUser.Account;
                json["activeTime"] = surfUser.ActiveTime;
                json["machineName"] = surfUser.PcName;
                json["memberTypeDesc"] = surfUser.MemberTypeName;
                json["memberName"] = surfUser.MemberName;
                json["sex"] = surfUser.Sex;
                json["phone"] = surfUser.Phone;
                json["identifyNum"] = surfUser.IdValue;
                json["identifyType"] = surfUser.IdType;
                json["birthday"] = surfUser.Birthday;
                json["provinceID"] = surfUser.ProvinceId;
                json["cityID"] = surfUser.CityId;
                json["districtID"] = surfUser.DistrictID;
                json["address"] = surfUser.Address;
                json["openID"] = surfUser.OpenID;
                json["offLineTime"] = surfUser.LogoffTime;
                json["qq"] = surfUser.QQ;
                json["memberID"] = surfUser.MemberId;

                json["memberType"] = surfUser.MemberTypeId;
                json["baseBalance"] = surfUser.BaseBalance;
                json["awardBalance"] = surfUser.AwardBalance;
                json["cashBalance"] = surfUser.TempBalance;

                json["costType"] = (int)surfUser.CostType;
                json["onlineFee"] = surfUser.AllHadCost;
                json["onlineStartTime"] = surfUser.LogonTimestamp;
                json["ruleId"] = surfUser.RuleId;
                json["ruleValue"] = surfUser.RuleValue;
                json["periodBegin"] = surfUser.PeriodStartTime;
                json["periodEnd"] = surfUser.PeriodEndTime;
                json["bRoomOwner"] = surfUser.RoomOwner ? "true" : "false";

                json["price"] = surfUser.HourPrice;
                json["startPrice"] = surfUser.StartPrice;
                json["ignoreTime"] = surfUser.IgnoreTime;
                json["maxEndTime"] = surfUser.MaxEndTimestamp;
                json["lastCostTime"] = surfUser.LastCostTimestamp;
                json["nextCostTime"] = surfUser.NextCostTimestamp;

                json["areaType"] = surfUser.AreaTypeId;
                json["areaName"] = this.FindAreaName(surfUser.AreaTypeId);

            }

            if (surfPc != null)
            {
                json["machineName"] = surfPc.PcName;
                json["machineState"] = (Int32)surfPc.PcState;
                json["areaType"] = surfPc.AreaTypeId;
                json["areaName"] = this.areaTypeMap[surfPc.AreaTypeId].AreaName;
            }

        }

        public void StopAllUserCost(ref string responseJson)
        {
            SurfUser[] list = this.surfUserMap.Values.ToArray();

            LogHelper.WriteLog("cashier logoff all user");

            foreach (SurfUser surfUser in list)
            {
                if (surfUser.MemberTypeId == Billing.DB_DEFAULT_USERTYPE_ID)
                {
                    continue;
                }
                this.LogOffUser(surfUser.MemberId, ref responseJson, true, false, true);
            }

        }

        public void QueryOnlineUserList(ref string responseJson)
        {
            JArray jArr = new JArray();

            List<SurfPc> pcList = this.surfPcMap.Values.ToList<SurfPc>();

            pcList.Sort(
                    delegate (SurfPc p1, SurfPc p2)
                    {
                        //return (p1.AreaTypeId == p2.AreaTypeId) ? p1.PcName.CompareTo(p2.PcName) : p1.AreaTypeId.CompareTo(p2.AreaTypeId);
                        return p1.PcName.CompareTo(p2.PcName);
                    }
                );

            foreach (SurfPc item in pcList)
            {
                JObject json = new JObject();
                this.GetSurfUserJson(item.lpSurfUser, item, ref json);
                jArr.Add(json);
            }

            foreach (KeyValuePair<UInt64, SurfUser> item in this.surfUserMap)
            {
                JObject json = new JObject();
                if (item.Value.PcName == "")
                {
                    this.GetSurfUserJson(item.Value, null, ref json);
                    jArr.Add(json);
                }
            }

            IDictionary<string, object> allList = new Dictionary<string, object>();
            allList["alllist"] = jArr;

            IDictionary<string, object> root = this.GetSuccessMap();
            root["data"] = allList;

            responseJson = JsonUtil.SerializeObject(root);

        }

        public void QueryUser(UInt64 memberId, ref string responseJson)
        {

            SurfUser surfUser = this.FindSurfUser(memberId);

            if (surfUser != null)
            {
                SurfPc surfPc = this.FindPc(surfUser.PcName);
                JObject json = new JObject();
                this.GetSurfUserJson(surfUser, surfPc, ref json);
                IDictionary<string, object> root = this.GetSuccessMap();
                root["data"] = json;
                responseJson = JsonUtil.SerializeObject(root);
            }
            else
            {
                IDictionary<string, object> root = this.GetErrMap(ErrDefine.STR_USER_NOTACTIVE);
                responseJson = JsonUtil.SerializeObject(root);
            }

        }

        private SurfUser AutoLoginUser(UInt64 memberId)
        {
            SurfUser surfUser;
            ActiveData activeData = new ActiveData();
            activeData.MemberId = memberId;
            activeData.CostType = CostType.COST_TYPE_WEEK;
            activeData.RoomOwner = false;

            string responsejson = "";
            surfUser = this.ActiveUser(activeData, ref responsejson, false);
            return surfUser;
        }

        public void PcLoginUser(LoginData loginData, ref string responseJson)
        {
            IDictionary<string, object> root;
            UInt64 memberId = loginData.MemberId;
            string pcName = loginData.PcName;

            SurfPc surfPc = this.FindPc(pcName);
            if (surfPc == null)
            {
                root = GetErrMap(ErrDefine.STR_PC_NOTFOUND);
                responseJson = JsonUtil.SerializeObject(root);
                return;
            }

            SurfUser surfUser = this.FindSurfUser(memberId);
            if (surfUser == null)
            {

                if (billingRate.NeedActive == 0)
                {
                    surfUser = this.AutoLoginUser(loginData.MemberId);
                }

                if (surfUser == null)
                {
                    responseJson = this.GetErrStr(ErrDefine.STR_USER_NOTACTIVE);
                    return;
                }
            }

            if (loginData.LoginType == Billing.LOGIN_TYPE_WX)
            {
                string pwd = MD5Util.EncryptWithMd5(memberId + Billing.WX_LOGIN_KEY + loginData.Timestamp);
                if (loginData.Password != pwd)
                {
                    responseJson = this.GetErrStr(ErrDefine.STR_ERROR_PASSWORD);
                    return;
                }
            }
            else
            {
                if (loginData.Password != surfUser.Password)
                {
                    responseJson = this.GetErrStr(ErrDefine.STR_ERROR_PASSWORD);
                    return;
                }
            }

            if (surfUser.CostType != CostType.COST_TYPE_WEEK &&
                surfPc.AreaTypeId != surfUser.AreaTypeId &&
                surfUser.PcName == "")
            {
                responseJson = this.GetErrStr(ErrDefine.STR_ERROR_WRONG_AREA);
                return;
            }

            if (this.CheckAreaAllowMemberType(surfPc.AreaTypeId, surfUser.MemberTypeId) == false)
            {
                responseJson = this.GetErrStr(ErrDefine.STR_ERROR_AREA_NOTALLOWED);
                return;
            }

            if (surfUser.PcName != "" && surfUser.PcName != loginData.PcName)
            {
                if (this.ChangePc(loginData.MemberId, loginData.PcName, (uint)surfPc.PcInfo.AreaId, ref responseJson) == false)
                {
                    // responseJson = this.GetErrStr(ErrDefine.STR_PC_EXISTING_USER);
                    return;
                }
                else
                {
                    //换机成功
                    return;
                }
            }

            if (surfUser.PcName == loginData.PcName || surfPc.lpSurfUser == surfUser)
            {
                //repeat login
                root = this.GetSuccessMap();
                JObject obj = new JObject();
                this.GetSurfUserJson(surfUser, surfPc, ref obj);
                root["data"] = obj;

                responseJson = JsonUtil.SerializeObject(root);
                return;


            }

            if (surfUser.PcName == "" && surfPc.lpSurfUser != null)
            {
                if (surfPc.lpSurfUser.CostType != CostType.COST_TYPE_WEEK)
                {
                    responseJson = this.GetErrStr(ErrDefine.STR_PC_EXISTING_USER);
                    return;
                }

                if (surfPc.lpSurfUser.MemberTypeId == Billing.DB_DEFAULT_USERTYPE_ID)
                {
                    responseJson = this.GetErrStr(ErrDefine.STR_PC_EXISTING_USER);
                    return;
                }

                LogHelper.WriteLog("try tick off memberId:" + surfPc.lpSurfUser.MemberId + ",by memberid:" + surfUser.MemberId + ",at pcName:" + loginData.PcName);

                if (this.LogOffUser(surfPc.lpSurfUser.MemberId, ref responseJson, false, true, false) == false)
                {
                    responseJson = this.GetErrStr(ErrDefine.STR_PC_EXISTING_USER);
                    return;
                }

            }

            UInt32 now = this.GetCurrentTimestamp();

            if (surfUser.CostType == CostType.COST_TYPE_WEEK)
            {

                surfUser.AreaTypeId = surfPc.AreaTypeId;

                WeekPrice weekPrice = this.FindWeekItem(surfUser.MemberTypeId, surfUser.AreaTypeId);
                if (weekPrice == null)
                {
                    responseJson = this.GetErrStr(ErrDefine.STR_NO_BILLING);
                    return;
                }

                weekPrice.resetSurfUser(surfUser, surfPc, now);

            }

            ExtraPrice extraPrice = this.FindExtraPrice(surfUser.MemberTypeId, surfUser.AreaTypeId);
            if (extraPrice != null)
            {
                surfUser.ExtraCharge = extraPrice.AdditionalPrice;
            }


            if (surfUser.LogonTimestamp == 0)
            {

                if (surfUser.remain() < surfUser.ExtraCharge + surfUser.RuleValue)
                {
                    surfUser.PcName = "";
                    responseJson = this.GetErrStr(ErrDefine.STR_ERROR_NOMONEY);
                    return;
                }

                surfUser.LogonTimestamp = now;
                surfUser.WholeTimestamp = now;
                surfUser.PcName = loginData.PcName;
                 if (surfUser.CostType == CostType.COST_TYPE_WEEK)
                {
                    surfUser.NextCostTimestamp = surfUser.LogonTimestamp + surfUser.IgnoreTime;
                    surfUser.MaxEndTimestamp = this.CalculateMaxEndTime(surfUser);
                    this.CostExtraFee(surfUser);
                }
                else
                {
                    surfUser.NextCostTimestamp = surfUser.LogonTimestamp;
                    surfUser.MaxEndTimestamp = this.CalculateMaxEndTime(surfUser);
                    this.CostExtraFee(surfUser);
                    if (this.UserCost(surfUser, now) == -1)
                    {
                        // 中断
                        surfUser.PcName = "";
                        responseJson = this.GetErrStr(ErrDefine.STR_ERROR_NOMONEY);
                        return ;
                    }
                }

                this.onlineDao.UpdateOnline(surfUser);
                this.AddSurfUser(surfUser);
            }



            surfPc.lpSurfUser = surfUser;
            surfPc.PcHeartTime = now;

            root = this.GetSuccessMap();
            JObject data = new JObject();
            this.GetSurfUserJson(surfUser, surfPc, ref data);
            root["data"] = data;
            root["business"] = NotifyDefine.NOTIFY_UPT_USER;
            root["subBn"] = NotifyDefine.NOTIFY_UPT_SN_USERLOGON;

            responseJson = JsonUtil.SerializeObject(root);

            LogHelper.WriteLog("loginUser:" + responseJson);

            this.PublishChangeInfo(responseJson, surfUser, surfPc, true, true);

        }

        #region 会员上机接口（新）

        /// <summary>
        /// 会员上机接口
        /// </summary>
        /// <param name="loginData"></param>
        /// <param name="responseJson"></param>
        public void PcLogonUser(LoginData loginData, ref string responseJson)
        {
            IDictionary<string, object> root;
            UInt64 memberId = loginData.MemberId;
            string pcName = loginData.PcName;

            // 机器名为空
            if (string.IsNullOrEmpty(pcName))
            {
                root = GetErrMap(ErrDefine.STR_PC_NOTFOUND);
                responseJson = JsonUtil.SerializeObject(root);
                return;
            }

            // 查找机器
            SurfPc surfPc = this.FindPc(pcName);
            if (surfPc == null)
            {
                root = GetErrMap(ErrDefine.STR_PC_NOTFOUND);
                responseJson = JsonUtil.SerializeObject(root);
                return;
            }

            // 查找会员
            SurfUser surfUser = this.FindSurfUser(memberId);
            if (surfUser == null)
            {
                // 是否需要激活
                if (billingRate.NeedActive == 0)
                {
                    // 自动激活
                    surfUser = this.AutoLoginUser(loginData.MemberId);
                }

                // 激活失败
                if (surfUser == null)
                {
                    responseJson = this.GetErrStr(ErrDefine.STR_USER_NOTACTIVE);
                    return;
                }
            }

            // 登陆方式
            if (loginData.LoginType == Billing.LOGIN_TYPE_WX)
            {
                string pwd = MD5Util.EncryptWithMd5(memberId + Billing.WX_LOGIN_KEY + loginData.Timestamp);
                if (loginData.Password != pwd)
                {
                    responseJson = this.GetErrStr(ErrDefine.STR_ERROR_PASSWORD);
                    return;
                }
            }
            else
            {
                if (loginData.Password != surfUser.Password)
                {
                    responseJson = this.GetErrStr(ErrDefine.STR_ERROR_PASSWORD);
                    return;
                }
            }

            // 包时用户区域不一致检测
            if (surfUser.CostType != CostType.COST_TYPE_WEEK &&
                surfPc.AreaTypeId != surfUser.AreaTypeId &&
                surfUser.PcName == "")
            {
                responseJson = this.GetErrStr(ErrDefine.STR_ERROR_WRONG_AREA);
                return;
            }

            // 区域是否允许该会员等级上机
            if (this.CheckAreaAllowMemberType(surfPc.AreaTypeId, surfUser.MemberTypeId) == false)
            {
                responseJson = this.GetErrStr(ErrDefine.STR_ERROR_AREA_NOTALLOWED);
                return;
            }

            // 会员重新登录
            if (surfUser.PcName == loginData.PcName || surfPc.lpSurfUser == surfUser)
            {
                //repeat login
                root = this.GetSuccessMap();
                JObject obj = new JObject();
                this.GetSurfUserJson(surfUser, surfPc, ref obj);
                root["data"] = obj;

                responseJson = JsonUtil.SerializeObject(root);
                return;
            }

            // 计算费率
            UInt32 now = this.GetCurrentTimestamp();
            // 单机计费计算费率
            if (surfUser.CostType == CostType.COST_TYPE_WEEK)
            {
                surfUser.AreaTypeId = surfPc.AreaTypeId;

                WeekPrice weekPrice = this.FindWeekItem(surfUser.MemberTypeId, surfUser.AreaTypeId);
                if (weekPrice == null)
                {
                    responseJson = this.GetErrStr(ErrDefine.STR_NO_BILLING);
                    return;
                }

                weekPrice.resetSurfUser(surfUser, surfPc, now);
            }

            // 附加费
            ExtraPrice extraPrice = this.FindExtraPrice(surfUser.MemberTypeId, surfUser.AreaTypeId);
            if (extraPrice != null)
            {
                surfUser.ExtraCharge = extraPrice.AdditionalPrice;
            }

            // 激活首次上机
            if (surfUser.LogonTimestamp == 0)
            {
                // 校验余额
                if (surfUser.remain() < surfUser.ExtraCharge + surfUser.RuleValue)
                {
                    surfUser.PcName = "";
                    responseJson = this.GetErrStr(ErrDefine.STR_ERROR_NOMONEY);
                    return;
                }

                // 采集信息
                surfUser.LogonTimestamp = now;
                surfUser.WholeTimestamp = now;
                surfUser.PcName = loginData.PcName;

                // 标准计费
                if (surfUser.CostType == CostType.COST_TYPE_WEEK)
                {
                    surfUser.NextCostTimestamp = surfUser.LogonTimestamp + surfUser.IgnoreTime;
                    surfUser.MaxEndTimestamp = this.CalculateMaxEndTime(surfUser);
                    this.CostExtraFee(surfUser);
                }
                else
                {
                    surfUser.NextCostTimestamp = surfUser.LogonTimestamp;
                    surfUser.MaxEndTimestamp = this.CalculateMaxEndTime(surfUser);
                    this.CostExtraFee(surfUser);
                    if (this.UserCost(surfUser, now) == -1)
                    {
                        // 中断
                        surfUser.PcName = "";
                        responseJson = this.GetErrStr(ErrDefine.STR_ERROR_NOMONEY);
                        return;
                    }
                }

                this.onlineDao.UpdateOnline(surfUser);
                this.AddSurfUser(surfUser);
            }





            // 是否属于换机操作
            if (surfUser.PcName != "" && surfUser.PcName != loginData.PcName)
            {
                if (this.ChangePc(loginData.MemberId, loginData.PcName, (uint)surfPc.PcInfo.AreaId, ref responseJson) == false)
                {
                    // responseJson = this.GetErrStr(ErrDefine.STR_PC_EXISTING_USER);
                    return;
                }
                else
                {
                    //换机成功
                    return;
                }
            }

            // 顶机操作
            if (surfUser.PcName == "" && surfPc.lpSurfUser != null)
            {
                if (surfPc.lpSurfUser.CostType != CostType.COST_TYPE_WEEK)
                {
                    responseJson = this.GetErrStr(ErrDefine.STR_PC_EXISTING_USER);
                    return;
                }

                if (surfPc.lpSurfUser.MemberTypeId == Billing.DB_DEFAULT_USERTYPE_ID)
                {
                    responseJson = this.GetErrStr(ErrDefine.STR_PC_EXISTING_USER);
                    return;
                }

                LogHelper.WriteLog("try tick off memberId:" + surfPc.lpSurfUser.MemberId + ",by memberid:" + surfUser.MemberId + ",at pcName:" + loginData.PcName);

                if (this.LogOffUser(surfPc.lpSurfUser.MemberId, ref responseJson, false, true, false) == false)
                {
                    responseJson = this.GetErrStr(ErrDefine.STR_PC_EXISTING_USER);
                    return;
                }
            }

            

            // 激活首次上机
            if (surfUser.LogonTimestamp == 0)
            {
                if (surfUser.remain() < surfUser.ExtraCharge + surfUser.RuleValue)
                {
                    surfUser.PcName = "";
                    responseJson = this.GetErrStr(ErrDefine.STR_ERROR_NOMONEY);
                    return;
                }

                surfUser.LogonTimestamp = now;
                surfUser.WholeTimestamp = now;
                surfUser.PcName = loginData.PcName;
                if (surfUser.CostType == CostType.COST_TYPE_WEEK)
                {
                    surfUser.NextCostTimestamp = surfUser.LogonTimestamp + surfUser.IgnoreTime;
                    surfUser.MaxEndTimestamp = this.CalculateMaxEndTime(surfUser);
                    this.CostExtraFee(surfUser);
                }
                else
                {
                    surfUser.NextCostTimestamp = surfUser.LogonTimestamp;
                    surfUser.MaxEndTimestamp = this.CalculateMaxEndTime(surfUser);
                    this.CostExtraFee(surfUser);
                    if (this.UserCost(surfUser, now) == -1)
                    {
                        // 中断
                        surfUser.PcName = "";
                        responseJson = this.GetErrStr(ErrDefine.STR_ERROR_NOMONEY);
                        return;
                    }
                }

                this.onlineDao.UpdateOnline(surfUser);
                this.AddSurfUser(surfUser);
            }

            surfPc.lpSurfUser = surfUser;
            surfPc.PcHeartTime = now;

            root = this.GetSuccessMap();
            JObject data = new JObject();
            this.GetSurfUserJson(surfUser, surfPc, ref data);
            root["data"] = data;
            root["business"] = NotifyDefine.NOTIFY_UPT_USER;
            root["subBn"] = NotifyDefine.NOTIFY_UPT_SN_USERLOGON;
            responseJson = JsonUtil.SerializeObject(root);
            LogHelper.WriteLog("loginUser:" + responseJson);
            this.PublishChangeInfo(responseJson, surfUser, surfPc, true, true);
        }

        #endregion

        public bool LogOffUser(UInt64 memberId, ref string responseJson, bool isCashier, bool isForce, bool isNoteClient)
        {

            SurfUser surfUser = this.FindSurfUser(memberId);
            if (surfUser == null)
            {
                responseJson = this.GetErrStr(ErrDefine.STR_USER_NOTACTIVE);
                return true;
            }

            SurfPc surfPc = this.FindPc(surfUser.PcName);


            if (isCashier == false && surfUser.MemberTypeId == Billing.DB_DEFAULT_USERTYPE_ID && surfUser.TempBalance > 0f)
            {
                responseJson = this.GetErrStr(ErrDefine.STR_ERROR_CLIENT_TEMPUSER);
                return false;
            }

            if (surfPc != null)
            {
                surfPc.lpSurfUser = null;
                surfPc.PcHeartTime = 0;
                surfPc.PcState = PcState.PC_ON_NOUSER;
            }

            this.RemoveSurfUser(memberId);

            surfUser.LogoffTime = this.GetCurrentTimestamp();

            this.onlineDao.UpdateOnline(surfUser);

            IDictionary<string, object> root = this.GetSuccessMap();
            JObject json = new JObject();
            this.GetSurfUserJson(surfUser, surfPc, ref json);
            root["data"] = json;
            root["business"] = NotifyDefine.NOTIFY_UPT_USER;
            root["subBn"] = NotifyDefine.NOTIFY_UPT_SN_USERLOGOFF;

            responseJson = JsonUtil.SerializeObject(root);

            LogHelper.WriteLog("logOff:" + responseJson);

            this.PublishChangeInfo(responseJson, surfUser, surfPc, true, true);
            surfUser.PcName = "";
            return true;
        }

        public bool ChangePc(UInt64 memberId, string pcName, UInt32 areaId, ref string responseJson)
        {

            if (memberId == 0 || pcName == "" || areaId == 0)
            {
                responseJson = GetErrStr(ErrDefine.STR_ERROR_PARAMS);
                return false;
            }

            SurfUser surfUser = this.FindSurfUser(memberId);
            if (surfUser == null)
            {
                responseJson = GetErrStr(ErrDefine.STR_USER_NOTACTIVE);
                return false;
            }

            if (this.CheckAreaAllowMemberType(areaId, surfUser.MemberTypeId) == false)
            {
                responseJson = GetErrStr(ErrDefine.STR_ERROR_AREA_NOTALLOWED);
                return false;
            }

            SurfPc surfPc = this.FindPc(pcName);
            if (surfPc == null)
            {
                responseJson = GetErrStr(ErrDefine.STR_PC_NOTFOUND);
                return false;
            }

            if (surfPc.lpSurfUser != null)
            {

                if (surfPc.lpSurfUser.MemberTypeId == Billing.DB_DEFAULT_USERTYPE_ID)
                {
                    responseJson = GetErrStr(ErrDefine.STR_ERROR_CLIENT_TEMPUSER);
                    return false;
                }

                if (surfPc.lpSurfUser.CostType != CostType.COST_TYPE_WEEK)
                {
                    responseJson = GetErrStr(ErrDefine.STR_PC_CHANGE_NOT_ALLOWED);
                    return false;
                }

                if (this.LogOffUser(surfPc.lpSurfUser.MemberId, ref responseJson, false, true, true) == false)
                {
                    responseJson = GetErrStr(ErrDefine.STR_PC_CHANGE_NOT_ALLOWED);
                    return false;
                }

            }

            // 跨区域预判校验
            if (surfUser.AreaTypeId != areaId)
            {
                //// 临时卡拒绝跨区域  
                //if (surfUser.MemberTypeId == Billing.DB_DEFAULT_USERTYPE_ID)
                //{
                //    LogHelper.WriteLog("temp user change pc by area of different denied");
                //    responseJson = GetErrStr(ErrDefine.STR_PC_CHANGE_NOT_ALLOWED_TEMPUSER);
                //    return false;
                //}

                // 包时期间拒绝跨区域
                if (surfUser.CostType != CostType.COST_TYPE_WEEK)
                {
                    LogHelper.WriteLog("period user change pc by area of different denied");
                    responseJson = GetErrStr(ErrDefine.STR_PC_CHANGE_NOT_ALLOWED_PERIODUSER);
                    return false;
                }
            }


            {
                SurfPc oldPc = this.FindPc(surfUser.PcName);
                if (oldPc != null)
                {
                    oldPc.PcHeartTime = 0; 
                    oldPc.lpSurfUser = null;
                    oldPc.PcState = PcState.PC_ON_NOUSER;

                    JObject obj = new JObject();
                    this.GetSurfUserJson(null, oldPc, ref obj);
                    IDictionary<string, object> data = this.GetSuccessMap();
                    data["business"] = NotifyDefine.NOTIFY_UPT_SN_CHANGEPC;
                    data["subBn"] = NotifyDefine.NOTIFY_UPT_SN_USERLOGOFF;
                    data["data"] = obj;
                    responseJson = JsonUtil.SerializeObject(data);

                    LogHelper.WriteLog("notify:" + oldPc.PcName + "," + responseJson);

                    this.PublishChangeInfo(responseJson, null, oldPc, true, true);
                }

            }

            UInt32 curTime = this.GetCurrentTimestamp();

            // 跨区域换机
            if (surfUser.AreaTypeId != areaId)
            {
                LogHelper.WriteLog("surf user by change areaId start");

                // 信息捕获
                surfUser.LogoffTime = curTime;
                surfUser.PcName = pcName;
                surfUser.AreaTypeId = areaId;

                // 费率重新计算
                WeekPrice item = this.FindWeekItem(surfUser.MemberTypeId, surfUser.AreaTypeId);
                if (item != null)
                {
                    item.resetSurfUser(surfUser, surfPc, curTime);
                }

                // 跨区域换机执行
                if (this.onlineDao.UpdateOnlineByCrossArea(surfUser) == false)
                {
                    // 跨区域换机失败返回
                    responseJson = GetErrStr(ErrDefine.STR_DB_ERROR);
                    return false;
                }

                surfUser.LogoffTime = 0;
                surfUser.LogonTimestamp = curTime;
                surfUser.WholeTimestamp = curTime;
                surfUser.NextCostTimestamp = surfUser.LogonTimestamp + surfUser.IgnoreTime;
                surfUser.MaxEndTimestamp = this.CalculateMaxEndTime(surfUser);
                surfUser.AllHadCost = 0;
                surfUser.emptyBill();

                /*  旧版跨区域换机
                 
                // 模拟结账
                
                if (this.onlineDao.UpdateOnline(surfUser) == false)
                {
                    LogHelper.WriteLog("surf user by change areaId : logoff error");
                    // 模拟结账失败返回
                    responseJson = GetErrStr(ErrDefine.STR_DB_ERROR);
                    return false;
                }

                // 更新会员机器信息
                surfUser.PcName = pcName;
                surfUser.AreaTypeId = areaId;
                WeekPrice item = this.FindWeekItem(surfUser.MemberTypeId, surfUser.AreaTypeId);
                if (item != null)
                {
                    item.resetSurfUser(surfUser, surfPc, curTime);
                }

                // 模拟激活
                if (this.onlineDao.AddOnline(surfUser) == false)
                {
                    LogHelper.WriteLog("surf user by change areaId : active error");
                    // 模拟激活失败返回
                    responseJson = GetErrStr(ErrDefine.STR_DB_ERROR);
                    return false;
                }
                
                surfUser.LogoffTime = 0;
                surfUser.LogonTimestamp = curTime;
                surfUser.WholeTimestamp = curTime;
                surfUser.NextCostTimestamp = surfUser.LogonTimestamp + surfUser.IgnoreTime;
                surfUser.MaxEndTimestamp = this.CalculateMaxEndTime(surfUser);
                surfUser.AllHadCost = 0;
                surfUser.emptyBill();
                
                // 模拟上机
                if (this.onlineDao.UpdateOnline(surfUser) == false)
                {
                    LogHelper.WriteLog("surf user by change areaId : logon error");
                    // 模拟上机失败返回
                    responseJson = GetErrStr(ErrDefine.STR_DB_ERROR);
                    return false;
                }  
                
                 旧版跨区域换机    */

                LogHelper.WriteLog("surf user by change areaId end");
            }
            else
            {
                // 更新会员机器信息
                surfUser.PcName = pcName;

                this.onlineDao.UpdateOnline(surfUser);
            }

            // 关联机器和用户
            this.SurfUserBindPc(surfUser, surfPc);

            LogHelper.WriteLog("changePc memberId:" + surfUser.MemberId + ",pcName:" + surfUser.PcName);

            LogHelper.WriteLog(surfUser.ToString());

            IDictionary<string, object> root = this.GetSuccessMap();
            JObject jobj = new JObject();
            this.GetSurfUserJson(surfUser, surfPc, ref jobj);
            root["data"] = jobj;
            root["business"] = NotifyDefine.NOTIFY_UPT_USER;
            root["subBn"] = NotifyDefine.NOTIFY_UPT_SN_USERLOGON;

            responseJson = JsonUtil.SerializeObject(root);
            this.PublishChangeInfo(responseJson, surfUser, surfPc, true, true);

            return true;
        }

        public void SurfUserBindPc(SurfUser surfUser, SurfPc surfPc)
        {
            if (surfUser == null || surfPc == null)
            {
                return;
            }
            surfUser.PcName = surfPc.PcName;
            surfPc.lpSurfUser = surfUser;
        }

        public bool changeToWeek(UInt64 memberId, ref string responseStr)
        {
            SurfUser surfUser = this.FindSurfUser(memberId);
            if (surfUser == null)
            {
                responseStr = this.GetErrStr(ErrDefine.STR_USER_NOTACTIVE);
                return false;
            }

            SurfPc surfPc = this.FindPc(surfUser.PcName);
            if (surfPc == null)
            {
                responseStr = this.GetErrStr(ErrDefine.STR_ERROR_PARAMS);
                return false;
            }

            if (surfUser.CostType == CostType.COST_TYPE_WEEK)
            {
                responseStr = this.GetErrStr(ErrDefine.STR_ERROR_PARAMS);
                return false;
            }

            surfUser.CostType = CostType.COST_TYPE_WEEK;
            UInt32 now = this.GetCurrentTimestamp();
            WeekPrice weekPrice = this.FindWeekItem(surfUser.MemberTypeId, surfUser.AreaTypeId);
            if (weekPrice != null)
            {
                surfUser.RuleId = weekPrice.RuleId;
                surfUser.IgnoreTime = weekPrice.IgnoreTime;
                surfUser.StartPrice = weekPrice.StartPrice;
                surfUser.MinCostPrice = weekPrice.MinCostPrice;
                surfUser.HourPrice = weekPrice.HourPrice(now);

                ExtraPrice extraPrice = this.FindExtraPrice(surfUser.MemberTypeId, surfUser.AreaTypeId);
                if (extraPrice != null)
                {
                    surfUser.ExtraCharge = extraPrice.AdditionalPrice;
                }

            }

            surfUser.WholeTimestamp = surfUser.MaxEndTimestamp;
            surfUser.NextCostTimestamp = surfUser.MaxEndTimestamp + surfUser.IgnoreTime;
            surfUser.MaxEndTimestamp = this.CalculateMaxEndTime(surfUser);

            this.onlineDao.UpdateOnline(surfUser);

            IDictionary<string, object> root = this.GetSuccessMap();
            JObject job = new JObject();
            this.GetSurfUserJson(surfUser, surfPc, ref job);
            root["data"] = job;
            root["business"] = NotifyDefine.NOTIFY_UPT_USER;
            root["subBn"] = "";
            responseStr = JsonUtil.SerializeObject(root);
            LogHelper.WriteLog("change to weekrate: " + responseStr);
            this.PublishChangeInfo(responseStr, surfUser, surfPc, true, true);
            return true;
        }

        public void SynUserInfo(UInt64 memberID, ref string responseJson)
        {

            SurfUser surfUser = this.FindSurfUser(memberID);

            if (surfUser != null && surfUser != null)
            {

                IDictionary<string, object> info = this.memberDao.queryMemberInfo(memberID);
                if (info != null)
                {
                    surfUser.Phone = info["phone"].ToString();
                    surfUser.OpenID = info["openID"].ToString();
                    surfUser.MemberTypeId = UInt32.Parse(info["memberType"].ToString());
                    surfUser.BaseBalance = float.Parse(info["baseBalance"].ToString());
                    surfUser.AwardBalance = float.Parse(info["awardBalance"].ToString());
                    surfUser.TempBalance = float.Parse(info["cashBalance"].ToString());
                    LogHelper.WriteLog("surfUser now:" + surfUser.ToString());
                }

                SurfPc surfPc = this.FindPc(surfUser.PcName);
                JObject json = new JObject();

                if (surfUser.CostType == CostType.COST_TYPE_WEEK)
                {
                    this.CalculateMaxEndTime(surfUser);
                }

                this.GetSurfUserJson(surfUser, surfPc, ref json);
                IDictionary<string, object> root = this.GetSuccessMap();
                root["data"] = json;
                root["business"] = NotifyDefine.NOTIFY_UPT_USER;
                root["subBn"] = NotifyDefine.NOTIFY_UPT_SN_SYNBALANCE;
                responseJson = JsonUtil.SerializeObject(root);
                this.PublishChangeInfo(responseJson, surfUser, surfPc, true, true);


            }

            responseJson = this.GetSuccessStr();

        }

        private bool CheckAreaAllowMemberType(UInt32 areaTypeId, UInt32 memberType)
        {

            AreaItem areaItem = null;
            this.areaTypeMap.TryGetValue(areaTypeId, out areaItem);
            if (areaItem == null)
            {
                return false;
            }

            if (areaItem.memberTypeList == null)
            {
                return true;
            }

            if (areaItem.memberTypeList == "")
            {
                return true;
            }

            if (areaItem.memberTypeList.Contains(memberType + ""))
            {
                return true;
            }

            return false;
        }

        public void payOrderByBaseBalance(UInt64 memberID, UInt64 orderId, float orderCost, float baseCost, ref string responseJson)
        {

            if (this.billingRate.cashierflag == 0)
            {
                responseJson = this.GetErrStr(ErrDefine.STR_ERROR_FEEPAY_NOTALLOW);
                return;
            }
            

            bool result = this.memberDao.PayOrderByBaseBalance(this.gid, memberID, orderId, orderCost, baseCost);
            if (result == true)
            {
                LogHelper.WriteLog("cost base pay order: memberID:" + memberID + ",orderId:" + orderId + ",orderCost:" + orderCost + ",baseCost:" + baseCost);
                this.SynUserInfo(memberID, ref responseJson);
                responseJson = this.GetSuccessStr();
            }
            else
            {
                responseJson = this.GetErrStr(ErrDefine.STR_DB_ERROR);
            }

        }


        private UInt32 CalculateNextCostTime(SurfUser surfUser)
        {
            if (surfUser.CostType != CostType.COST_TYPE_WEEK)
            {
                surfUser.NextCostTimestamp = surfUser.LogonTimestamp + surfUser.DurationTime;
                return 0;
            }
            float cost = (surfUser.CurrentCostTemp + surfUser.CurrentCostBase + surfUser.CurrentCostAward);
            surfUser.NextCostTimestamp = surfUser.LastCostTimestamp + (UInt32)(3600 * Math.Truncate(cost / surfUser.HourPrice)) + (UInt32)((60 * 60 - surfUser.IgnoreTime) * ((cost / surfUser.HourPrice) - Math.Truncate(cost / surfUser.HourPrice)));
            return 0;
        }

        private UInt32 CalculateMaxEndTime(SurfUser surfUser)
        {

            if (surfUser.CostType == CostType.COST_TYPE_WEEK)
            {
                if (surfUser.HourPrice <= 0)
                {
                    surfUser.MaxEndTimestamp = surfUser.NextCostTimestamp + 2 * 60 * 60;
                    return surfUser.MaxEndTimestamp;
                }

                surfUser.MaxEndTimestamp = surfUser.NextCostTimestamp + (UInt32)((surfUser.remain() / surfUser.HourPrice) * 60 * 60);
            }
            else if (surfUser.CostType == CostType.COST_TYPE_PERIOD)
            {
                surfUser.MaxEndTimestamp = surfUser.LogonTimestamp + surfUser.DurationTime;
            }
            else if (surfUser.CostType == CostType.COST_TYPE_DURATION)
            {
                surfUser.MaxEndTimestamp = surfUser.LogonTimestamp + surfUser.DurationTime;
            }
            return surfUser.MaxEndTimestamp;
        }


        private ExtraPrice FindExtraPrice(UInt32 memberTypeId, UInt32 areaId)
        {
            foreach (ExtraPrice item in this.billingRate.ExtraPrices)
            {
                if (item.AreaTypeId == areaId && item.MemberTypeId == memberTypeId)
                {
                    return item;
                }
            }

            return null;

        }


        private float CalculateSurfUserWeekPrice(SurfUser surfUser, UInt32 curTime)
        {

            float cost = 0;
            UInt32 totalSec = 60 * 60 - surfUser.IgnoreTime;

            if (surfUser.HourPrice <= 0)
            {
                //如果最小费率是0，则扣费为0
                surfUser.NextCostTimestamp = curTime + 60 * 60;

                WeekPrice item = this.FindWeekItem(surfUser.MemberTypeId, surfUser.AreaTypeId);
                if (item != null)
                {
                    surfUser.HourPrice = item.HourPrice(surfUser.NextCostTimestamp);
                }

                return 0;
            }

            //每秒价格=1小时的总时间（秒）/每小时的价格
            float secsPerPrice = (float)totalSec / surfUser.HourPrice;

            if (surfUser.AllHadCost <= surfUser.ExtraCharge && surfUser.StartPrice > 0)
            {
                if (surfUser.remain() >= surfUser.StartPrice)
                {
                    cost = surfUser.StartPrice;
                }
                else
                {
                    cost = surfUser.remain();
                }

                surfUser.NextCostTimestamp = curTime + (UInt32)(60 * 60 * Math.Truncate(cost / surfUser.HourPrice)) + (UInt32)(totalSec * ((cost / surfUser.HourPrice) - Math.Truncate(cost / surfUser.HourPrice)));
                return cost;
            }
            else
            {
                if (surfUser.remain() >= surfUser.MinCostPrice)
                {
                    cost = surfUser.MinCostPrice;
                }
                else
                {
                    cost = surfUser.remain();
                }
            }

            if (cost == 0)
            {
                surfUser.NextCostTimestamp = surfUser.LastCostTimestamp + 60 * 60;
                return cost;
            }

            if ((surfUser.NextCostTimestamp - surfUser.WholeTimestamp) % 3600 + (UInt32)(secsPerPrice * cost) < 3600)
            {
                surfUser.NextCostTimestamp = curTime + (UInt32)(secsPerPrice * cost);
                return cost;
            }
            else
            {
                surfUser.NextCostTimestamp = curTime + (UInt32)(3600 - (curTime - surfUser.WholeTimestamp) % 3600) + surfUser.IgnoreTime;
                cost = (float)Math.Round((double)(surfUser.NextCostTimestamp - curTime - surfUser.IgnoreTime) / secsPerPrice, 1);

                WeekPrice item = this.FindWeekItem(surfUser.MemberTypeId, surfUser.AreaTypeId);
                if (item != null)
                {
                    surfUser.HourPrice = item.HourPrice(curTime);
                }

            }

            return cost;
        }

        private int DivideUserCost(SurfUser surfUser, float price)
        {
            UInt16 radioBase = surfUser.RatioCostBase;
            UInt16 radioAward = surfUser.RatioCostAward;

            if (price > (surfUser.remain()))
            {
                return -1;
            }

            float result = 0;
            surfUser.CurrentCostTemp = 0;
            surfUser.CurrentCostBase = 0;
            surfUser.CurrentCostAward = 0;
            if (surfUser.TempBalance >= price)
            {
                surfUser.CurrentCostTemp = price;
                return 0;
            }

            surfUser.CurrentCostTemp = surfUser.TempBalance;
            result = price - surfUser.CurrentCostTemp;

            if (surfUser.CostType != CostType.COST_TYPE_WEEK)
            {
                if (surfUser.BaseBalance >= result)
                {
                    surfUser.CurrentCostBase = result;
                }
                else
                {
                    surfUser.CurrentCostBase = surfUser.BaseBalance;
                    result = result - surfUser.CurrentCostBase;
                    surfUser.CurrentCostAward = result;
                }
            }
            else
            {
                //float baseCost = result * ((float)radioBase / (radioBase + radioAward));
                float baseCost = (float)Math.Round(result * ((float)radioBase / (radioBase + radioAward)), 2);
                float awardCost = result - baseCost;

                if (surfUser.BaseBalance >= baseCost)
                {

                    surfUser.CurrentCostBase = baseCost;

                    if (surfUser.AwardBalance >= awardCost)
                    {
                        surfUser.CurrentCostAward = awardCost;
                    }
                    else
                    {
                        surfUser.CurrentCostAward = surfUser.AwardBalance;
                        surfUser.CurrentCostBase = baseCost + (awardCost - surfUser.AwardBalance);
                    } 

                }
                else
                {
                    surfUser.CurrentCostBase = surfUser.BaseBalance;
                    surfUser.CurrentCostAward = awardCost + (baseCost - surfUser.BaseBalance);
                }

            }

            return 0;

        }


        private void PublishChangeInfo(string content, SurfUser surfUser, SurfPc surfPc, bool isClient, bool isCashier)
        {
            try
            {

                List<string> list = new List<string>();
                list.Add(ZeroMqService.cashierSubject);

                if (isClient == true && surfPc != null && CashierUtil.IsValidIp(surfPc.PcInfo.Ip))
                {
                    try
                    {
                        list.Add(this.GetIpNumByIp(surfPc.PcInfo.Ip) + "");
                    }
                    catch (Exception exc)
                    {
                        LogHelper.WriteLog("GetIpNumByIp error", exc);
                    }
                }

                byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
                byte[] headerBytes = new byte[128];// header.toBytes();

                foreach (string channel in list)
                {
                    string str = string.Format("{0} {1}", channel, 1);
                    System.Text.Encoding.UTF8.GetBytes(str).CopyTo(headerBytes, 0);

                    byte[] msg = new byte[544 + contentBytes.Length];
                    headerBytes.CopyTo(msg, 0);
                    contentBytes.CopyTo(msg, 544);

                    this.zeroMqService.Publish(msg);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("publish error", ex);
            }
        }

        private UInt32 GetIpNumByIp(string ip)
        {
            try
            {
                return BitConverter.ToUInt32(IPAddress.Parse(ip).GetAddressBytes(), 0);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("AidaClient,转换Ip地址出错!", ex);
            }
            return 0;
        }

        private bool CostExtraFee(SurfUser surfUser)
        {
            // 无附加费
            if (surfUser.ExtraCharge <= 0)
            {
                return true;
            }

            // 有附加费则校验余额
            if (surfUser.remain() >= surfUser.ExtraCharge)
            {

                this.DivideUserCost(surfUser, surfUser.ExtraCharge);
                surfUser.BaseBalance = surfUser.BaseBalance - surfUser.CurrentCostBase;
                surfUser.AwardBalance = surfUser.AwardBalance - surfUser.CurrentCostAward;
                surfUser.TempBalance = surfUser.TempBalance - surfUser.CurrentCostTemp;
                surfUser.AllHadCost += (surfUser.CurrentCostTemp + surfUser.CurrentCostBase + surfUser.CurrentCostAward);
                LogHelper.WriteLog("cost memberid:" + surfUser.MemberId + ",account:" + surfUser.Account + ",ignore time:" + surfUser.IgnoreTime + ",extra fee:" + (surfUser.CurrentCostTemp + surfUser.CurrentCostBase + surfUser.CurrentCostAward));
                this.DoCost(surfUser);

            }
            return false;
        }

        private int UserCost(SurfUser surfUser, UInt32 now)
        {

            if (surfUser == null)
            {
                return -1;
            }

            if (surfUser.remain() <= 0)
            {
                string responseJson = "";
                LogHelper.WriteLog("member:" + surfUser.MemberId + ",money zero logoff");
                this.LogOffUser(surfUser.MemberId, ref responseJson, false, false, true);
                return -1;
            }

            if (surfUser.CostType == CostType.COST_TYPE_WEEK)
            {

                if ((surfUser.LogonTimestamp + surfUser.IgnoreTime) > now)
                {
                    surfUser.NextCostTimestamp = surfUser.LogonTimestamp + surfUser.IgnoreTime;
                    surfUser.MaxEndTimestamp = this.CalculateMaxEndTime(surfUser);

                    LogHelper.WriteLog(surfUser.ToString());

                    return 0;
                }

                float cost = this.CalculateSurfUserWeekPrice(surfUser, surfUser.NextCostTimestamp == 0 ? now : surfUser.NextCostTimestamp);
                this.DivideUserCost(surfUser, cost);
            }
            else if (surfUser.CostType == CostType.COST_TYPE_PERIOD)
            {
                int result = this.DivideUserCost(surfUser, surfUser.RuleValue);
                if (result == -1)
                {
                    string responseJson = "";

                    this.LogOffUser(surfUser.MemberId, ref responseJson, false, false, true);
                    return -1;
                }
                PeriodPrice period = this.FindPeriodItem(surfUser.RuleId);
                surfUser.NextCostTimestamp = period.generateMaxEndTime(now);
                surfUser.MaxEndTimestamp = surfUser.NextCostTimestamp;
            }
            else if (surfUser.CostType == CostType.COST_TYPE_DURATION)
            {
                int result = this.DivideUserCost(surfUser, surfUser.RuleValue);
                if (result == -1)
                {
                    string responseJson = "";
                    this.LogOffUser(surfUser.MemberId, ref responseJson, false, false, true);
                    return -1;
                }
                surfUser.NextCostTimestamp = now + surfUser.DurationTime;
                surfUser.MaxEndTimestamp = now + surfUser.DurationTime;
            }

            if (surfUser.CostType == CostType.COST_TYPE_WEEK)
            {
                this.DiscountCost(surfUser);
            }

            surfUser.BaseBalance = surfUser.BaseBalance - surfUser.CurrentCostBase;
            surfUser.AwardBalance = surfUser.AwardBalance - surfUser.CurrentCostAward;
            surfUser.TempBalance = surfUser.TempBalance - surfUser.CurrentCostTemp;

            surfUser.AllHadCost += (surfUser.CurrentCostBase + surfUser.CurrentCostAward + surfUser.CurrentCostTemp);

            if (surfUser.CostType == CostType.COST_TYPE_WEEK)
            {
                surfUser.MaxEndTimestamp = this.CalculateMaxEndTime(surfUser);
            }

            surfUser.LastCostTimestamp = now;

            LogHelper.WriteLog(surfUser.ToString());

            this.DoCost(surfUser);

            return 0;

        }

        private void DiscountCost(SurfUser surfUser)
        {

            surfUser.CurrentCostBase = surfUser.CurrentCostBase * surfUser.Discount;
            surfUser.CurrentCostAward = surfUser.CurrentCostAward * surfUser.Discount;

        }

        private void DoCost(SurfUser surfUser)
        {
            this.onlineDao.DoCost(surfUser);

            JObject jobj = new JObject();

            SurfPc surfPc = this.FindPc(surfUser.PcName);
            if (surfPc == null)
            {
                return;
            }

            this.GetSurfUserJson(surfUser, surfPc, ref jobj);
            IDictionary<string, object> root = this.GetSuccessMap();
            root["data"] = jobj;
            root["business"] = NotifyDefine.NOTIFY_UPT_USER;
            root["subBn"] = NotifyDefine.NOTIFY_UPT_SN_SYNBALANCE;

            Bill bill = new Bill();
            bill.currentCostBase = surfUser.CurrentCostBase;
            bill.currentCostAward = surfUser.CurrentCostAward;
            bill.currentCostTemp = surfUser.CurrentCostTemp;

            bill.memberID = surfUser.MemberId;
            bill.lastCostTimestamp = surfUser.LastCostTimestamp;

            surfUser.addBill(bill);
            this.checkSmartPeriod(surfUser);

            this.PublishChangeInfo(JsonUtil.SerializeObject(root), surfUser, surfPc, true, true);
        }

        private void checkSmartPeriod(SurfUser surfUser)
        {
            //临卡不享受智能包夜
            //if(surfUser.MemberTypeId == Billing.DB_DEFAULT_USERTYPE_ID)
            //{
            //    return ;
            //}

            if (surfUser.CostType != CostType.COST_TYPE_WEEK)
            {
                return ;
            }

            List<PeriodPrice> list = this.FindSmartPeriod(surfUser.MemberTypeId, surfUser.AreaTypeId);

            foreach (PeriodPrice item in list)
            {
                if (item.isIn(surfUser.LastCostTimestamp, true) == false)
                {
                    continue;
                }

                float costSum = surfUser.calculateCostSum(item.generateStartTime(surfUser.LastCostTimestamp));
                if (costSum < item.Price)
                {
                    return;
                }


                surfUser.CostType = CostType.COST_TYPE_PERIOD;
                surfUser.RuleId = item.RuleId;
                surfUser.RuleValue = item.Price;
                surfUser.NextCostTimestamp = item.generateMaxEndTime(surfUser.LastCostTimestamp);
                surfUser.MaxEndTimestamp = surfUser.NextCostTimestamp;

                LogHelper.WriteLog("change to smart period:" + surfUser.ToString());

                this.onlineDao.UpdateOnline(surfUser);

                // 修改netbar_billing表时间
                if (this.onlineDao.UpdateBilling(surfUser) < 0)
                {
                    LogHelper.WriteLog("update billing error : db execute error");
                }

                SurfPc surfPc = this.FindPc(surfUser.PcName);

                IDictionary<string, object> root = GetSuccessMap();
                JObject content = new JObject();
                this.GetSurfUserJson(surfUser, surfPc, ref content);
                root["data"] = content;
                root["business"] = NotifyDefine.NOTIFY_UPT_USER;
                root["subBn"] = "";

                string responseJson = JsonUtil.SerializeObject(root);

                this.PublishChangeInfo(responseJson, surfUser, surfPc, true, true);
                return;

            }

        }

        private void HandlePeriodEnd(SurfUser surfUser, UInt32 currTime)
        {

            if (surfUser == null)
            {
                return;
            }

            LogHelper.WriteLog("memberid:" + surfUser.MemberId + ",end of period or duration");

            if (this.billingRate.DurationAction == 2 || surfUser.MemberTypeId == Billing.DB_DEFAULT_USERTYPE_ID)
            {

                LogHelper.WriteLog("memberid:" + surfUser.MemberId + ",try to change to weekrate!");

                SurfPc surfPc = this.FindPc(surfUser.PcName);
                if (surfPc != null)
                {
                    if (surfUser.remain() > 0 && (currTime - surfPc.PcHeartTime) <= this.billingRate.ActiveTime)
                    {
                        string response = "";
                        bool result = this.changeToWeek(surfUser.MemberId, ref response);
                        if (result)
                        {
                            this.PublishChangeInfo(response, surfUser, surfPc, true, true);
                            return;
                        }
                    }
                }

            }

            string responseJson = "";
            LogHelper.WriteLog("end of period or duration,logoff memberid:" + surfUser.MemberId);
            this.LogOffUser(surfUser.MemberId, ref responseJson, false, false, true);

        }

        private WeekPrice FindWeekItem(UInt32 memberTypeId, UInt32 areaTypeId)
        {
            foreach (WeekPrice item in this.billingRate.weekPrices)
            {
                if (item.AreaId == areaTypeId && item.MemberType == memberTypeId)
                {
                    return item;
                }
            }
            return null;
        }

        private PeriodPrice FindPeriodItem(UInt32 ruleId)
        {
            foreach (PeriodPrice item in this.billingRate.PeriodHourPrices)
            {
                if (item.RuleId == ruleId)
                {
                    return item;
                }
            }

            return null;
        }

        private PeriodPrice FindPeriodItem(float startTime, float endTime, UInt32 memberType, UInt32 areaId, float ruleValue)
        {
            foreach (PeriodPrice item in this.billingRate.PeriodHourPrices)
            {
                if (item.StartTime == startTime && item.EndTime == endTime && item.MemberType == memberType && item.Price == ruleValue && item.AreaId == areaId)
                {
                    return item;
                }
            }
            return null;
        }

        private DurationPrice FindDurationItem(UInt32 ruleId)
        {
            foreach (DurationPrice item in this.billingRate.DurationHourPrices)
            {
                if (item.RuleId == ruleId)
                {
                    return item;
                }
            }
            return null;
        }

        private DurationPrice FindDurationItem(UInt32 areaId, UInt32 memberType, float ruleValue)
        {
            foreach (DurationPrice item in this.billingRate.DurationHourPrices)
            {
                if (item.MemberType == memberType && item.Price == ruleValue && item.AreaId == areaId)
                {
                    return item;
                }
            }
            return null;
        }

        public void check()
        {

            int count = 0;

            object obj = new object();
            Monitor.Enter(obj);
            while (true)
            {
                Monitor.Wait(obj, 30 * 1000, false);
                UInt32 currentTime = this.GetCurrentTimestamp();

                if (this.PingIP("www.baidu.com"))
                {
                    count = 0;
                }
                else
                {
                    count++;
                }

                if (count < 4)
                {
                    this.mainForm.setNetStateLbl("正常");
                }
                else
                {
                    this.mainForm.setNetStateLbl("异常，请联系技术检查网络连接");
                    List<SurfPc> list = this.surfPcMap.Values.ToList();
                    foreach (SurfPc surfPc in list)
                    {
                        if (surfPc.lpSurfUser == null)
                        {
                            continue;
                        }

                        if (surfPc.PcHeartTime == 0)
                        {
                            continue;
                        }
                        surfPc.PcHeartTime = currentTime;
                    }

                    continue;
                }

                List<SurfPc> pcList = this.surfPcMap.Values.ToList();
                foreach (SurfPc surfPc in pcList)
                {

                    try
                    {
                        if (surfPc.lpSurfUser == null)
                        {
                            continue;
                        }

                        if (this.PingIP(surfPc.PcInfo.Ip) == true)
                        {
                            continue;
                        }

                        if (surfPc.PcHeartTime == 0)
                        {
                            continue;
                        }

                        if (currentTime > surfPc.PcHeartTime)
                        {
                            UInt32 HeatLen = currentTime - surfPc.PcHeartTime;

                            if (HeatLen > this.billingRate.ActiveTime)
                            {
                                SurfUser surfUser = surfPc.lpSurfUser;

                                if (surfUser.CostType != CostType.COST_TYPE_WEEK)
                                {
                                    continue;
                                }
                                string UserContent = "";
                                this.LogOffUser(surfUser.MemberId, ref UserContent, false, true, true);
                                LogHelper.WriteLog("check thread:" + UserContent);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLog("check tick err", ex);
                    }

                }

                IniUtil.setLastTick(currentTime);

            }
        }

        private bool PingIP(string strIP)
        {
            try
            {

                System.Net.NetworkInformation.Ping psender = new System.Net.NetworkInformation.Ping();
                System.Net.NetworkInformation.PingReply prep = psender.Send(strIP, 500, Encoding.Default.GetBytes("123"));
                if (prep.Status == System.Net.NetworkInformation.IPStatus.Success)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("ping err", ex);
            }

            return false;
        }

        public void tick()
        {
            object obj = new object();
            Monitor.Enter(obj);
            while (true)
            {

                try
                {

                    Monitor.Wait(obj, 333, false);

                    if (state == 0)
                    {
                        UInt32 cur = this.GetCurrentTimestamp();
                        SurfUser[] list = this.surfUserMap.Values.ToArray();
                        foreach (SurfUser item in list)
                        {

                            try
                            {

                                if (item == null)
                                {
                                    continue;
                                }
                                if (item.LogonTimestamp == 0)
                                {
                                    continue;
                                }
                                else if (item.CostType != CostType.COST_TYPE_WEEK)
                                {
                                    if (item.MaxEndTimestamp <= cur && item.MaxEndTimestamp != 0)
                                    {
                                        this.HandlePeriodEnd(item, cur);
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    if (item.NextCostTimestamp <= cur)
                                    {
                                        this.UserCost(item, cur);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.WriteLog("surfUser:" + item.ToString(), ex);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("tick erro", ex);
                }
            }
        }



        //user channel


        public void QueryUserBaseInfoEx(string account, ref string responseJson)
        {
            IDictionary<string, object> root = new Dictionary<string, object>();

            if (this.billingRate.IsChain()) //连锁
            {

                IDictionary<string, string> parameters = HttpUtil.initParams();
                parameters.Add("gid", ServerUtil.currentUser.shopid + "");
                parameters.Add("account", account);

                String response = HttpUtil.doPost(ServerUtil.CashierMemberUrl + "/loadMember", parameters);

                JObject resp = JObject.Parse(response);

                if (Convert.ToInt32(resp["result"].ToString()) != 0)
                {
                    root = this.GetErrMap(ErrDefine.STR_NET_ERROR);
                }
                else
                {
                    MemberDTO chainMemberDto = JsonUtil.DeserializeJsonToObject<MemberDTO>(resp["chainMember"].ToString());
                    MemberDTO netbarMemberDto = JsonUtil.DeserializeJsonToObject<MemberDTO>(resp["netbarMember"].ToString());

                    if (chainMemberDto != null)
                    {
                        chainMemberDto.Gid = ServerUtil.currentUser.shopid;
                        chainMemberDto.BirthDay = DateUtil.GetFormatByTime13(chainMemberDto.BirthDay);

                        if (netbarMemberDto != null)
                        {
                            //chainMemberDto.Memberid = netbarMemberDto.Memberid;
                            netbarMemberDto.BirthDay = DateUtil.GetFormatByTime13(netbarMemberDto.BirthDay);
                        }

                        memberDao.updateMemberInfo(netbarMemberDto);
                        memberDao.updateMemberBaseBalance(chainMemberDto);

                        IDictionary<string, object> info = this.memberDao.queryMemberInfo(account);
                        if (info != null)
                        {
                            root = this.GetSuccessMap();
                            root["data"] = info;
                        }
                        else
                        {
                            memberDao.InsertMember(chainMemberDto);
                            memberDao.updateMemberBaseBalance(chainMemberDto);

                            info = this.memberDao.queryMemberInfo(account);
                            if (info != null)
                            {
                                root = this.GetSuccessMap();
                                root["data"] = info;
                            }
                            else
                            {
                                root = this.GetErrMap(ErrDefine.STR_USER_NOTEXIST);
                            }

                        }
                    }
                    else
                    {
                        IDictionary<string, object> info = this.memberDao.queryMemberInfo(account);
                        if (info != null)
                        {
                            root = this.GetSuccessMap();
                            root["data"] = info;
                        }
                        else
                        {
                            root = this.GetErrMap(ErrDefine.STR_USER_NOTEXIST);
                        }
                    }

                }
            }
            else //单店模式
            {
                IDictionary<string, object> info = this.memberDao.queryMemberInfo(account);
                if (info != null)
                {
                    root = this.GetSuccessMap();
                    root["data"] = info;
                }
                else
                {

                    IDictionary<string, string> parameters = HttpUtil.initParams();
                    parameters.Add("gid", ServerUtil.currentUser.shopid + "");
                    parameters.Add("account", account);

                    String response = HttpUtil.doPost(ServerUtil.CashierMemberUrl + "/loadNetbarMember", parameters);

                    JObject resp = JObject.Parse(response);

                    if (Convert.ToInt32(resp["result"].ToString()) == 0)
                    {
                        MemberDTO netbarMemberDto = JsonUtil.DeserializeJsonToObject<MemberDTO>(resp["netbarMember"].ToString());
                        netbarMemberDto.BirthDay = DateUtil.GetFormatByTime13(netbarMemberDto.BirthDay);
                        memberDao.InsertMember(netbarMemberDto);

                        info = this.memberDao.queryMemberInfo(account);
                        if (info != null)
                        {
                            root = this.GetSuccessMap();
                            root["data"] = info;
                        }
                        else
                        {
                            root = this.GetErrMap(ErrDefine.STR_TRY_LATER);
                        }

                    }
                    else
                    {
                        root = this.GetErrMap(ErrDefine.STR_USER_NOTEXIST);
                    }

                    
                }
            }
            root["version"] = 2;
            responseJson = JsonUtil.SerializeObject(root);

        }
        
        public void QueryUserBaseInfo(string account, ref string responseJson)
        {
            IDictionary<string, object> root = new Dictionary<string, object>();
            IDictionary<string, object> info = this.memberDao.queryMemberInfo(account);

            if (info != null)
            {
                root = this.GetSuccessMap();
                root["data"] = info;
            }
            else
            {
                root = this.GetErrMap(ErrDefine.STR_USER_NOTEXIST);
            }

            //// 加载会员最新信息
            
            //ResponseDTO dto = this.extraLogic.LoadLastMemberInfo(account);
            //if (dto.State.Equals("0"))
            //{
            //    root = this.GetSuccessMap();
            //    root["data"] = memberDao.queryMemberInfo(account);
            //}
            //else
            //{
            //    root["state"] = dto.State;
            //    root["info"] = dto.Info;
            //}
            

            responseJson = JsonUtil.SerializeObject(root);
        }



        public void QueryUserBaseInfo(UInt64 memberId, ref string responseJson)
        {
            IDictionary<string, object> info = this.memberDao.queryMemberInfo(memberId);
            IDictionary<string, object> root = null;
            if (info != null)
            {
                root = this.GetSuccessMap();
                root["data"] = info;
            }
            else
            {
                root = this.GetErrMap(ErrDefine.STR_USER_NOTEXIST);
            }

            responseJson = JsonUtil.SerializeObject(root);

        }

        public void AddUser(JObject data, ref string responseJson)
        {
            MemberDTO memberDto = JsonUtil.DeserializeJsonToObject<MemberDTO>(data.ToString());
            memberDto.Gid = (int)this.gid;
            IDictionary<string, object> root = new Dictionary<string, object>();

            string contype = "1";
            string code = memberDto.Account.ToString();
            string responseStr = "";

            long memberID = this.QueryMemberInfo(contype, code, ref responseStr);
            if (memberID == -1)
            {
                responseJson = responseStr;
            }
            else if (memberID == 0)
            {
                root = memberDao.AddMember(memberDto);
                responseJson = JsonUtil.SerializeObject(root);
            }
            else
            {
                root = this.GetSuccessMap();
                root["data"] = memberID;
                responseJson = JsonUtil.SerializeObject(root);
            }
        }

        public void UpdateUser(JObject data, ref string responseJson)
        {
            MemberDTO memberDto = JsonUtil.DeserializeJsonToObject<MemberDTO>(data.ToString());
            memberDto.Gid = (int)this.gid;

            IDictionary<string, object> root = memberDao.UpdateMember(memberDto);

            if (Int32.Parse(root["state"].ToString()) == 0)
            {

                SurfUser surfUser = this.FindSurfUser((ulong)memberDto.Memberid);
                if (surfUser != null)
                {
                    surfUser.OpenID = memberDto.OpenID;
                    surfUser.MemberTypeId = memberDto.MemberType;
                    surfUser.MemberName = memberDto.MemberName;
                    surfUser.Sex = (uint)memberDto.Sex;
                    surfUser.Phone = memberDto.Phone;
                    surfUser.Birthday = memberDto.BirthDay;

                    this.NotifySurfUserChangeInfo(surfUser, true, true);

                }

            }

            responseJson = JsonUtil.SerializeObject(root);

        }

        public void UpdateUserInfo(UInt64 memberID, string memberName, string account, string password, int memberType, int sex, string phone, string openID, string qq, string birthday, int cashierID, int provinceID, int cityID, int districtID, string address)
        {
            //
        }

        public void ChargeUser(JObject data, ref string responseJson)
        {
            //UInt64 memberID, int rechargeWay, int awardFeeType, int rechargeType, int state, int cashierID, int orderSource, int rechargeSource, int rechargeCompaignID, double rechargeFee, double awardFee, double cashBalance
            RechargeOrderDTO rechargeOrderDTO = JsonUtil.DeserializeJsonToObject<RechargeOrderDTO>(data.ToString());
            rechargeOrderDTO.gid = (int)this.gid;

            SurfUser surfUser = this.FindSurfUser((ulong)rechargeOrderDTO.memberID);

            IDictionary<string, object> root = rechargeOrderDao.AddRechargeOrder(rechargeOrderDTO);

            if (Int32.Parse(root["state"].ToString()) == 0)
            {
                LogHelper.WriteLog("rechargeUser:" + rechargeOrderDTO.ToString());
            }

            if (Int32.Parse(root["state"].ToString()) == 0 && surfUser != null)
            {

                surfUser.AwardBalance = float.Parse(((Dictionary<string, object>)root["data"])["awardBalance"].ToString());
                surfUser.BaseBalance = float.Parse(((Dictionary<string, object>)root["data"])["baseBalance"].ToString());
                surfUser.TempBalance = float.Parse(((Dictionary<string, object>)root["data"])["cashBalance"].ToString());

                if (surfUser.CostType == CostType.COST_TYPE_WEEK)
                {
                    this.CalculateMaxEndTime(surfUser);
                }

                SurfPc surfPc = this.FindPc(surfUser.PcName);

                IDictionary<string, object> subRoot = GetSuccessMap();
                JObject content = new JObject();
                this.GetSurfUserJson(surfUser, surfPc, ref content);
                subRoot["data"] = content;
                subRoot["business"] = NotifyDefine.NOTIFY_UPT_USER;
                subRoot["subBn"] = "";

                string responseJson1 = JsonUtil.SerializeObject(subRoot);


                this.PublishChangeInfo(responseJson1, surfUser, surfPc, true, false);

            }

            responseJson = JsonUtil.SerializeObject(root);

        }

        private void NotifySurfUserChangeInfo(SurfUser surfUser, bool isClient, bool isCashier)
        {
            if (surfUser == null)
            {
                return;
            }

            SurfPc surfPc = this.FindPc(surfUser.PcName);

            IDictionary<string, object> subRoot = GetSuccessMap();
            JObject content = new JObject();
            this.GetSurfUserJson(surfUser, surfPc, ref content);
            subRoot["data"] = content;
            subRoot["business"] = NotifyDefine.NOTIFY_UPT_USER;
            subRoot["subBn"] = "";

            string responseJson1 = JsonUtil.SerializeObject(subRoot);


            this.PublishChangeInfo(responseJson1, surfUser, surfPc, isCashier, isClient);
        }


        public void WeekToPeriodOrDurationByActive(ActiveData activeData, ref string responseJson)
        {
            SurfUser surfUser = this.FindSurfUser(activeData.MemberId);
            if (surfUser == null)
            {
                responseJson = this.GetErrStr(ErrDefine.STR_USER_NOTACTIVE);
                return;
            }

            if (surfUser.CostType != CostType.COST_TYPE_WEEK)
            {
                responseJson = this.GetErrStr(ErrDefine.STR_USER_HASACTIVE);
                return;
            }

            if (activeData.CostType == CostType.COST_TYPE_WEEK)
            {
                responseJson = this.GetErrStr(ErrDefine.STR_ERROR_PARAMS);
                return;
            }

            // 赋予激活包时用户区域
            surfUser.AreaTypeId = activeData.AreaTypeId;

            UInt32 curTime = this.GetCurrentTimestamp();

            if (activeData.CostType == CostType.COST_TYPE_PERIOD)
            {
                PeriodPrice period = this.FindPeriodItem(activeData.RuleId);
                if (period == null)
                {
                    //float startTime,float endTime,Int32 memberType,Int32 areaId,float ruleValue
                    period = this.FindPeriodItem(activeData.PeriodStartTime, activeData.PeriodEndTime, surfUser.MemberTypeId, activeData.AreaTypeId, activeData.RuleValue);

                    if (period == null)
                    {
                        responseJson = this.GetErrStr(ErrDefine.STR_ERROR_PARAMS);
                        return;
                    }

                }

                if (period.AreaId != activeData.AreaTypeId)
                {
                    responseJson = this.GetErrStr(ErrDefine.STR_ERROR_PARAMS);
                    return;
                }

                if (period.Price > surfUser.remain())
                {
                    responseJson = this.GetErrStr(ErrDefine.STR_ERROR_NOMONEY);
                    return;
                }

                surfUser.CostType = CostType.COST_TYPE_PERIOD;
                surfUser.RuleId = activeData.RuleId;
                surfUser.RuleValue = period.Price;
                surfUser.PeriodStartTime = period.StartTime;
                surfUser.PeriodEndTime = period.EndTime;
                surfUser.DurationTime = period.PeriodTime;
            }
            else
            {
                DurationPrice duration = this.FindDurationItem(activeData.RuleId);
                if (duration == null)
                {
                    //UInt32 areaId,UInt32 memberType,float ruleValue
                    duration = this.FindDurationItem(activeData.AreaTypeId, surfUser.MemberTypeId, activeData.RuleValue);

                    if (duration == null)
                    {
                        responseJson = this.GetErrStr(ErrDefine.STR_ERROR_PARAMS);
                        return;
                    }

                }
                if (duration.AreaId != activeData.AreaTypeId)
                {
                    responseJson = this.GetErrStr(ErrDefine.STR_ERROR_PARAMS);
                    return;
                }

                if (duration.Price > surfUser.remain())
                {
                    responseJson = this.GetErrStr(ErrDefine.STR_ERROR_NOMONEY);
                    return;
                }

                surfUser.CostType = CostType.COST_TYPE_DURATION;
                surfUser.RuleId = activeData.RuleId;
                surfUser.RuleValue = duration.Price;
                surfUser.NextCostTimestamp = GetCurrentTimestamp();
                surfUser.DurationTime = duration.DurationTime;

            }

            IDictionary<string, object> root = null;
            bool result = this.onlineDao.UpdateOnline(surfUser);
            if (result == true)
            {
                this.AddSurfUser(surfUser);

                root = GetSuccessMap();
                JObject userJson = new JObject();
                this.GetSurfUserJson(surfUser, null, ref userJson);
                root["data"] = userJson;
                root["business"] = NotifyDefine.NOTIFY_UPT_USER;
                root["subBn"] = NotifyDefine.NOTIFY_UPT_SN_USERACTIVE;

                this.PublishChangeInfo(JsonUtil.SerializeObject(root), surfUser, null, false, true);
            }
            else
            {
                root = GetErrMap(ErrDefine.STR_DB_ERROR);
            }

            responseJson = JsonUtil.SerializeObject(root);

            LogHelper.WriteLog("会员转包时段: " + responseJson);
            return;
        }

        public void WeekToPeriodOrDuration(ActiveData activeData, ref string responseJson)
        {
            SurfUser surfUser = this.FindSurfUser(activeData.MemberId);
            if (surfUser == null)
            {
                responseJson = this.GetErrStr(ErrDefine.STR_USER_NOTACTIVE);
                return;
            }

            SurfPc surfPc = this.FindPc(surfUser.PcName);

            if (surfUser.CostType != CostType.COST_TYPE_WEEK)
            {
                responseJson = this.GetErrStr(ErrDefine.STR_USER_HASACTIVE);
                return;
            }

            if (activeData.CostType == CostType.COST_TYPE_WEEK)
            {
                responseJson = this.GetErrStr(ErrDefine.STR_ERROR_PARAMS);
                return;
            }

            UInt32 oldAreaId = surfUser.AreaTypeId;

            UInt32 curTime = this.GetCurrentTimestamp();

            if (activeData.CostType == CostType.COST_TYPE_PERIOD)
            {
                PeriodPrice period = this.FindPeriodItem(activeData.RuleId);
                if (period == null)
                {
                    //float startTime,float endTime,Int32 memberType,Int32 areaId,float ruleValue
                    period = this.FindPeriodItem(activeData.PeriodStartTime, activeData.PeriodEndTime, surfUser.MemberTypeId, activeData.AreaTypeId, activeData.RuleValue);

                    if (period == null)
                    {
                        responseJson = this.GetErrStr(ErrDefine.STR_ERROR_PARAMS);
                        return;
                    }

                }

                if (period.AreaId != activeData.AreaTypeId)
                {
                    responseJson = this.GetErrStr(ErrDefine.STR_ERROR_PARAMS);
                    return;
                }

                if (period.Price > surfUser.remain())
                {
                    responseJson = this.GetErrStr(ErrDefine.STR_ERROR_NOMONEY);
                    return;
                }

                surfUser.CostType = CostType.COST_TYPE_PERIOD;
                surfUser.RuleId = activeData.RuleId;
                surfUser.RuleValue = period.Price;
                surfUser.PeriodStartTime = period.StartTime;
                surfUser.PeriodEndTime = period.EndTime;
                surfUser.DurationTime = period.PeriodTime;
            }
            else
            {
                DurationPrice duration = this.FindDurationItem(activeData.RuleId);
                if (duration == null)
                {
                    //UInt32 areaId,UInt32 memberType,float ruleValue
                    duration = this.FindDurationItem(activeData.AreaTypeId, surfUser.MemberTypeId, activeData.RuleValue);

                    if (duration == null)
                    {
                        responseJson = this.GetErrStr(ErrDefine.STR_ERROR_PARAMS);
                        return;
                    }

                }
                if (duration.AreaId != activeData.AreaTypeId)
                {
                    responseJson = this.GetErrStr(ErrDefine.STR_ERROR_PARAMS);
                    return;
                }

                if (duration.Price > surfUser.remain())
                {
                    responseJson = this.GetErrStr(ErrDefine.STR_ERROR_NOMONEY);
                    return;
                }

                surfUser.CostType = CostType.COST_TYPE_DURATION;
                surfUser.RuleId = activeData.RuleId;
                surfUser.RuleValue = duration.Price;
                surfUser.NextCostTimestamp = GetCurrentTimestamp();
                surfUser.DurationTime = duration.DurationTime;

            }


            if (surfUser.LogonTimestamp != 0)
            {
                this.UserCost(surfUser, curTime);
            }

            this.onlineDao.UpdateOnline(surfUser);

            IDictionary<string, object> root = GetSuccessMap();
            JObject content = new JObject();
            this.GetSurfUserJson(surfUser, surfPc, ref content);
            root["data"] = content;
            root["business"] = NotifyDefine.NOTIFY_UPT_USER;
            root["subBn"] = "";

            responseJson = JsonUtil.SerializeObject(root);

            this.PublishChangeInfo(responseJson, surfUser, surfPc, true, true);

            LogHelper.WriteLog("会员转包时段: " + responseJson);

            responseJson = GetSuccessStr();
            return;

        }

        public void GetMemberID(string account, ref string responseJson)
        {
            UInt64 memberID = 0;
            IDictionary<string, object> root;
            if (this.memberDao.queryMemberIDByAccount(account, ref memberID))
            {
                root = this.GetSuccessMap();
                IDictionary<string, UInt64> data = new Dictionary<string, UInt64>();
                data["memberID"] = memberID;
                root["data"] = data;
            }
            else
            {
                root = this.GetErrMap(ErrDefine.STR_USER_NOTEXIST);
            }

            responseJson = JsonUtil.SerializeObject(root);
        }

        public void UpdatePWD(UInt64 memberID, string newPWD, ref string responseJson)
        {
            IDictionary<string, object> root;
            if (this.memberDao.ChangeMemberPwd(memberID, newPWD))
            {

                SurfUser surfUser = this.FindSurfUser(memberID);
                if (surfUser != null)
                {
                    surfUser.Password = newPWD;

                    this.NotifySurfUserChangeInfo(surfUser, true, true);

                }

                root = this.GetSuccessMap();
            }
            else
            {
                root = this.GetErrMap(ErrDefine.STR_USER_NOTEXIST);
            }

            responseJson = JsonUtil.SerializeObject(root);
        }

        public void SearchUser(string query, ref string responseJson)
        {

            IDictionary<string, object> root = this.GetSuccessMap();
            root["data"] = this.memberDao.fuzzyQueryMember(query);
            responseJson = JsonUtil.SerializeObject(root);

        }


        //duty channel
        public void GetDutyData(JObject data, ref string responseJson)
        {
            IDictionary<string, object> root = this.dutyDao.QueryDutyCheckInfo((int)data["GID"]);
            responseJson = JsonUtil.SerializeObject(root);
        }

        public void SubmitDutyData(JObject data, ref string responseJson)
        {
            IDictionary<string, object> root = this.dutyDao.UpdateDutyCheckInfo(data, (int)this.gid);
            responseJson = JsonUtil.SerializeObject(root);
        }

        #region 统一会员查询接口

        public long QueryMemberInfo(string contype, string code, ref string responseJson)
        {
            IDictionary<string, object> root = new Dictionary<string, object>();
            MemberDTO netbarMember = null;
            MemberDTO chainMember = null;
            UInt64 memberID = 0;
            int gid = ServerUtil.currentUser.shopid;
            bool isChain = ServerUtil.currentUser.isShopInChain();

            // 查询云端信息
            try
            {
                IDictionary<string, string> parameters = HttpUtil.initParams();
                parameters.Add("gid", gid + "");
                parameters.Add("contype", contype);
                parameters.Add("code", code);

                String response = HttpUtil.doPost(ServerUtil.userUrl + "/querymemberinfonew", parameters);
                LogHelper.WriteLog("CashierServer: 获取云会员信息数据" + response);
                JObject resp = JObject.Parse(response);
                
                // 重置云端服务器网络错误次数
                ServerUtil.yunErrorCount = 0;
                // 数据处理区
                {
                    if (resp["result"].ToString() == "0")
                    {
                        if (isChain)
                        {
                            chainMember = JsonUtil.DeserializeJsonToObject<MemberDTO>(resp["chainMember"].ToString());
                            if (chainMember != null)
                            {
                                // 检测是否创建会员
                                if (memberDao.queryMemberInfoByCode("1", chainMember.Account.ToString(), ref memberID) && memberID != 0)
                                {
                                    chainMember.Gid = gid;
                                    chainMember.Memberid = (long)memberID;

                                    // 更新漫游余额
                                    if (!memberDao.updateMemberBaseBalance(chainMember))
                                    {
                                        // 更新漫游余额失败则返回
                                        root = this.GetErrMap(ErrDefine.STR_USER_UPDATE_ERROR);
                                        responseJson = JsonUtil.SerializeObject(root);
                                        return -1;
                                    }

                                    // 更新本地会员信息
                                    chainMember.OpenID = string.IsNullOrEmpty(chainMember.OpenID) ? "" : chainMember.OpenID;
                                    chainMember.MemberType = 0;
                                    chainMember.lastUpdateDate = string.IsNullOrEmpty(chainMember.lastUpdateDate) ? "1970-01-01 00:00:00" : chainMember.lastUpdateDate;

                                    netbarMember = JsonUtil.DeserializeJsonToObject<MemberDTO>(resp["netbarMember"].ToString());
                                    if (netbarMember != null)
                                    {
                                        // 会员等级是否更新
                                        chainMember.MemberType = (netbarMember.MemberType > 1) ? netbarMember.MemberType : (byte)3;
                                    }

                                    // 更新本地会员信息
                                    if (memberDao.updateMemberInfoNew(chainMember))
                                    {
                                        LogHelper.WriteLog("update member info by yun success,memberID=" + chainMember.Memberid);
                                    }
                                    else
                                    {
                                        LogHelper.WriteLog("update member info by yun error,memberID=" + chainMember.Memberid);
                                    }
                                }
                                else
                                {
                                    // 自动创建会员
                                    chainMember.Gid = gid;
                                    chainMember.MemberType = 3;
                                    chainMember.AwardBalance = 0;
                                    if (!memberDao.InsertMember(chainMember))
                                    {
                                        // 连锁自动创建会员失败则返回
                                        root = this.GetErrMap(ErrDefine.STR_USER_CREATE_ERROR);
                                        responseJson = JsonUtil.SerializeObject(root);
                                        return -1;
                                    }
                                    else
                                    {
                                        memberDao.queryMemberInfoByCode("1", chainMember.Account.ToString(), ref memberID);
                                    }
                                }
                            }
                            else
                            {
                                // 连锁店非会员
                                netbarMember = JsonUtil.DeserializeJsonToObject<MemberDTO>(resp["netbarMember"].ToString());
                                if (netbarMember != null)
                                {
                                    netbarMember.OpenID = string.IsNullOrEmpty(netbarMember.OpenID) ? "" : netbarMember.OpenID;
                                    netbarMember.lastUpdateDate = string.IsNullOrEmpty(netbarMember.lastUpdateDate) ? "1970-01-01 00:00:00" : netbarMember.lastUpdateDate;
                                    // 更新本地会员信息
                                    if (memberDao.updateMemberInfoNew(netbarMember))
                                    {
                                        LogHelper.WriteLog("update member info by yun success,memberID=" + netbarMember.Memberid);
                                    }
                                    else
                                    {
                                        LogHelper.WriteLog("update member info by yun error,memberID=" + netbarMember.Memberid);
                                    }
                                    memberID = (ulong)netbarMember.Memberid;
                                }
                                else
                                {
                                    memberDao.queryMemberInfoByCode(contype, code, ref memberID);
                                }
                            }
                        }
                        else
                        {
                            netbarMember = JsonUtil.DeserializeJsonToObject<MemberDTO>(resp["netbarMember"].ToString());
                            if (netbarMember != null)
                            {
                                netbarMember.OpenID = string.IsNullOrEmpty(netbarMember.OpenID) ? "" : netbarMember.OpenID;
                                netbarMember.lastUpdateDate = string.IsNullOrEmpty(netbarMember.lastUpdateDate) ? "1970-01-01 00:00:00" : netbarMember.lastUpdateDate;
                                // 更新本地会员信息
                                if (memberDao.updateMemberInfoNew(netbarMember))
                                {
                                    LogHelper.WriteLog("update member info by yun success,memberID=" + netbarMember.Memberid);
                                }
                                else
                                {
                                    LogHelper.WriteLog("update member info by yun error,memberID=" + netbarMember.Memberid);
                                }
                                memberID = (ulong)netbarMember.Memberid;
                            }
                            else
                            {
                                memberDao.queryMemberInfoByCode(contype, code, ref memberID);
                            }
                        }
                    }
                }
                //{
                //    if (resp["result"].ToString() == "0")
                //    {
                //        netbarMember = JsonUtil.DeserializeJsonToObject<MemberDTO>(resp["netbarMember"].ToString());
                //        if (netbarMember != null)
                //        {
                //            // 更新本地会员信息
                //            if (memberDao.updateMemberInfo(netbarMember))
                //            {
                //                LogHelper.WriteLog("update member info by yun success");
                //            }
                //            else
                //            {
                //                LogHelper.WriteLog("update member info by yun error");
                //            }
                //        }

                //        chainMember = JsonUtil.DeserializeJsonToObject<MemberDTO>(resp["chainMember"].ToString());
                //        if (chainMember != null)
                //        {
                //            // 检测是否创建会员
                //            if (memberDao.queryMemberInfoByCode(contype, code, ref memberID) && memberID != 0)
                //            {
                //                if (isChain)
                //                {
                //                    // 更新余额
                //                    netbarMember.BaseBalance = chainMember.BaseBalance;
                //                    netbarMember.lastUpdateDate = chainMember.lastUpdateDate;
                //                    netbarMember.Gid = gid;
                //                    netbarMember.Memberid = (long)memberID;
                //                    if (!memberDao.updateMemberBaseBalance(netbarMember))
                //                    {
                //                        // 更新漫游余额失败则返回
                //                        root = this.GetErrMap(ErrDefine.STR_USER_UPDATE_ERROR);
                //                        responseJson = JsonUtil.SerializeObject(root);
                //                        return ;
                //                    }
                //                }
                //            }
                //            else
                //            {
                //                // 自动创建会员
                //                chainMember.Gid = (int)gid;
                //                chainMember.MemberType = 3;
                //                chainMember.AwardBalance = 0;
                //                if (!isChain)
                //                {
                //                    // 非连锁清空本金
                //                    chainMember.BaseBalance = 0;
                //                }
                //                if (!memberDao.InsertMember(chainMember))
                //                {
                //                    if (isChain)
                //                    {
                //                        // 连锁自动创建会员失败则返回
                //                        root = this.GetErrMap(ErrDefine.STR_USER_CREATE_ERROR);
                //                        responseJson = JsonUtil.SerializeObject(root);
                //                        return;
                //                    }
                //                }
                //                else
                //                {
                //                    memberDao.queryMemberInfoByCode(contype, code, ref memberID);
                //                }
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("请求云端会员信息失败", ex);
                // 连锁店网络异常则返回
                if (isChain)
                {
                    ServerUtil.yunErrorCount++;
                    if (ServerUtil.yunErrorCount > 5)
                    {
                        // 云服务器通讯中断则返回
                        root = this.GetErrMap(ErrDefine.STR_NET_BREAK_OFF);
                        responseJson = JsonUtil.SerializeObject(root);
                        return -1;
                    }
                    else
                    {
                        // 网络波动则返回
                        root = this.GetErrMap(ErrDefine.STR_NET_ERROR);
                        responseJson = JsonUtil.SerializeObject(root);
                        return -1;
                    }
                }
            }

            // 查询会员信息并返回
            IDictionary<string, object> info = (memberID == 0) ? null : memberDao.queryMemberInfo(memberID);

            root = this.GetSuccessMap();
            root["data"] = info;
            responseJson = JsonUtil.SerializeObject(root);
            return (long)memberID;
        }

        #endregion


        //extra channel


        public ResponseDTO UpdateMemberByYun(Object data)
        {
            ResponseDTO dto = new ResponseDTO();

            try
            {
                LogHelper.WriteLog("ExtraLogic: 调入UpdateMemberByYun方法, 参数：" + data.ToString());
                JObject value = JObject.Parse(data.ToString());
                long memberID = long.Parse(value["memberID"].ToString());
                String strPhone = (null == value["phone"] ? "" : value["phone"].ToString());

                IDictionary<string, string> parameters = HttpUtil.initParams();
                parameters.Add("gid", ServerUtil.currentUser.shopid + "");
                parameters.Add("memberId", memberID + "");
                parameters.Add("phone", strPhone);
                String response = HttpUtil.doPost(ServerUtil.CashierMemberUrl + "/queryNetbarMember", parameters);
                LogHelper.WriteLog("CashierServer: 更新云信息获得数据" + response);
                JObject resp = JObject.Parse(response);

                {
                    if (resp["state"].ToString() == "0")
                    {
                        MemberDTO memberDto = JsonUtil.DeserializeJsonToObject<MemberDTO>(resp["member"].ToString());
                        memberDto.Gid = ServerUtil.currentUser.shopid;
                        if (memberDao.updateMemberInfo(memberDto))
                        {
                            dto.State = "0";
                            dto.Info = "成功";
                        }
                        else
                        {
                            dto.State = "-1";
                            dto.Info = "本地版本,更新用户信息失败";
                            LogHelper.WriteLog("CashirServer:更新用户信息失败");
                        }
                        if (resp["result"].ToString() == "0")
                        {
                            if (null != resp["account"] && (!String.IsNullOrEmpty(resp["account"].ToString())))
                            {
                                memberDto.BaseBalance = Double.Parse(resp["account"]["basebalance"].ToString());
                                memberDto.lastUpdateDate = timestampToDateStr(resp["account"]["lastupdatedate"].ToString());
                                if (memberDao.updateMemberBaseBalance(memberDto))
                                {
                                    string responseJson = "";
                                    this.SynUserInfo((UInt64)memberDto.Memberid, ref responseJson);
                                    dto.State = "0";
                                    dto.Info = "成功";
                                    LogHelper.WriteLog("云端更新用户余额成功!");
                                }
                                else
                                {
                                    dto.State = "-1";
                                    dto.Info = "更新本地数据库失败";
                                    LogHelper.WriteLog("云端更新数据库失败,更新用户余额失败");
                                }
                            }
                            else
                            {
                                // 本地版本
                                dto.State = "0";
                                dto.Info = "成功";
                                LogHelper.WriteLog("本地用户只更新用户信息");
                            }

                        }
                        LogHelper.WriteLog("ChishirServer:获取用户数据，用户" + memberDto.MemberName + ",余额为:" + memberDto.BaseBalance);
                        dto.Data = memberDto;

                    }
                    else
                    {
                        if (ServerUtil.currentUser.isShopInSingle())
                        {
                            dto.State = "0";
                            dto.Info = "成功";
                        }
                        else
                        {
                            dto.State = "-1";
                            dto.Info = "用户正在其他门店上机";
                            LogHelper.WriteLog("会员信息没有找到");
                        }
                    }
                }

            }
            catch (Exception ex)
            {

                if (ServerUtil.currentUser.isShopInSingle())
                {
                    dto.State = "0";
                    dto.Info = "成功";
                }
                else
                {
                    LogHelper.WriteLog("云端更新会员 " + data.ToString() + " 信息失败:\n", ex);
                    dto.State = "2";
                    dto.Info = "网络连接失败";
                }
            }

            return dto;
        }

        /// <summary>
        /// 获取云端最新会员信息
        /// </summary>
        /// <param name="account">账号</param>
        /// <returns></returns>
        public ResponseDTO LoadLastMemberInfo(string account)
        {
            ResponseDTO dto = new ResponseDTO();

            try
            {

                IDictionary<string, string> parameters = HttpUtil.initParams();
                parameters.Add("gid", ServerUtil.currentUser.shopid + "");
                parameters.Add("account", account);

                String response = HttpUtil.doPost(ServerUtil.CashierMemberUrl + "/loadMember", parameters);

                JObject resp = JObject.Parse(response);
                if (Convert.ToInt32(resp["result"].ToString()) == 0)
                {

                    MemberDTO chainMemberDto = JsonUtil.DeserializeJsonToObject<MemberDTO>(resp["chainMember"].ToString());
                    MemberDTO netbarMemberDto = JsonUtil.DeserializeJsonToObject<MemberDTO>(resp["netbarMember"].ToString());

                    if (chainMemberDto != null)
                    {
                        chainMemberDto.Gid = ServerUtil.currentUser.shopid;
                        chainMemberDto.BirthDay = DateUtil.GetFormatByTime13(chainMemberDto.BirthDay);
                        if (netbarMemberDto == null)
                        {
                            chainMemberDto.MemberType = 3;
                            chainMemberDto.AwardBalance = 0;
                            if (memberDao.InsertMember(chainMemberDto))
                            {
                                dto.State = "0";
                                dto.Info = "成功";
                            }
                            else
                            {
                                dto.State = "-1";
                                dto.Info = "创建本地会员信息失败";
                            }
                        }
                        else
                        {
                            netbarMemberDto.Gid = ServerUtil.currentUser.shopid;
                            netbarMemberDto.BirthDay = DateUtil.GetFormatByTime13(netbarMemberDto.BirthDay);
                            if (memberDao.updateMemberInfo(netbarMemberDto))
                            {
                                dto.State = "0";
                                dto.Info = "成功";
                            }
                            else
                            {
                                dto.State = "-1";
                                dto.Info = "更新本地会员信息失败";
                            }

                            if (ServerUtil.currentUser.isShopInChain())
                            {
                                netbarMemberDto.BaseBalance = chainMemberDto.BaseBalance;
                                netbarMemberDto.lastUpdateDate = chainMemberDto.lastUpdateDate;
                                if (memberDao.updateMemberBaseBalance(netbarMemberDto))
                                {
                                    string responseJson = "";
                                    this.SynUserInfo((UInt64)netbarMemberDto.Memberid, ref responseJson);
                                    dto.State = "0";
                                    dto.Info = "成功";
                                }
                                else
                                {
                                    dto.State = "-1";
                                    dto.Info = "更新会员连锁本金失败";
                                }
                            }
                        }
                    }
                    else
                    {
                        if (netbarMemberDto == null)
                        {
                            dto.State = "1";
                            dto.Info = "未找到此账号信息";
                        }
                        else
                        {
                            netbarMemberDto.Gid = ServerUtil.currentUser.shopid;
                            netbarMemberDto.BirthDay = DateUtil.GetFormatByTime13(netbarMemberDto.BirthDay);
                            if (memberDao.updateMemberInfo(netbarMemberDto))
                            {
                                dto.State = "0";
                                dto.Info = "成功";
                            }
                            else
                            {
                                dto.State = "-1";
                                dto.Info = "更新本地会员信息失败";
                            }
                        }
                    }
                }
                else
                {
                    LogHelper.WriteLog("load chain member error : check java log");
                    dto.State = "-1";
                    dto.Info = "云端服务器错误";
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("load chain member error : ", ex);

                dto.State = "-1";
                dto.Info = "网络连接失败";
            }
            return dto;
        }

        public static string timestampToDateStr(string datestr)
        {
            long millineseconds = 0;

            try
            {
                millineseconds = long.Parse(datestr);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("timestampToDateStr", ex);
            }
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return dtStart.AddMilliseconds(millineseconds).ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        ///  加入连锁会员
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ResponseDTO AddChainMember(Object data)
        {
            ResponseDTO dto = new ResponseDTO();

            try
            { 
                
                MemberDTO memberDto = JsonUtil.DeserializeJsonToObject<MemberDTO>(data.ToString());
                memberDto.Gid = ServerUtil.currentUser.shopid;
                memberDto.CertificateType = 1;
                memberDto.MemberType = 3;
                if (memberDao.InsertMember(memberDto))
                {
                    dto.State = "0";
                    dto.Info = "成功";
                    LogHelper.WriteLog("ChishirServer:新增用户数据，用户" + memberDto.MemberName + ",余额为:" + memberDto.BaseBalance);
                }
                else
                {
                    dto.State = "-1";
                    dto.Info = "失败";
                    LogHelper.WriteLog("添加连锁用户失败");
                }
                dto.Data = memberDto;
                

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("添加连锁会员出错,错误:\n" + ex.Message);
            }

            return dto;
        }

        private string cashierView;
        private string sidebarView;
        private string clientContentView;
        private string plugins;
        private string backsideView;
        private string lockerView;
        private IDictionary<string, WebPage> fileMap = new Dictionary<string, WebPage>();

        public string getCashierView()
        {
            if (this.cashierView == null || this.cashierView == "")
            {
                this.cashierView = HttpUtil.doGet("http://yun.aida58.com/cashier/view/cashier?shopid=" + this.gid);
            }
            return this.cashierView;
        }

        public string getBacksideView()
        {
            if (this.backsideView == null || this.backsideView == "")
            {
                this.backsideView = HttpUtil.doGet("http://yun.aida58.com/cashier/view/backside?shopid=" + this.gid);
            }
            return this.backsideView;
        }

        public string getSidebarView()
        {
            if (this.sidebarView == null || this.sidebarView == "")
            {
                this.sidebarView = HttpUtil.doGet("http://yun.aida58.com/cashier/view/sidebar?shopid=" + this.gid + "&ver=2");
            }
            return this.sidebarView;
        }

        public string getClientContentView()
        {
            if (this.clientContentView == null || this.clientContentView == "")
            {
                this.clientContentView = HttpUtil.doGet("http://yun.aida58.com/cashier/view/usercenter?shopid=" + this.gid);
            }
            return this.clientContentView;
        }

        public string getLockerView()
        {
            if (this.lockerView == null || this.lockerView == "")
            {
                this.lockerView = HttpUtil.doGet("http://yun.aida58.com/upload/locker/" + this.gid + ".jpg");
            }
            return this.lockerView;
        }

        public void getFile(String url, out WebPage file)
        {
            fileMap.TryGetValue(url, out file);
            if (file != null)
            {
                return;
            }

            string contentType = "";
            string acceptRanges = "";
            UInt32 length = 0;
            string content = HttpUtil.doGetWithContentType("http://yun.aida58.com" + url, ref contentType, ref length, ref acceptRanges);

            file = new WebPage();
            file.content = content;
            file.contentType = contentType;
            file.name = url;
            file.length = length;
            file.acceptRanges = acceptRanges;

            fileMap[url] = file;
        }

        public string getPlugins()
        {
            if (this.plugins == null || this.plugins == "")
            {
                this.plugins = HttpUtil.doGet("http://plugins.aida58.com/plugins.zip");
            }
            return this.plugins;
        }

    }
}
