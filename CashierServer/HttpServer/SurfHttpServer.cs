using CashierLibrary.HttpServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CashierLibrary.Util;
using System.Diagnostics;
using CashierLibrary.Model;
using CashierServer.Logic.Surf;
using CashierLibrary.Model.Bill;
using Newtonsoft.Json.Linq;
using CashierServer.Util;

namespace CashierServer.HttpServer
{
    public class SurfHttpServer : CashierLibrary.HttpServer.HttpServer
    {

        public SurfLogic surfLogic { get; set; }
        
        public SurfHttpServer(int port) : base(port)
        {

        }

        public override void handleGETRequest(HttpProcessor p)
        {
            try
            {

                if (p.http_url.StartsWith("/cashier/view"))
                {
                    string content = this.surfLogic.getCashierView();
                    p.SendResponse(HttpProcessor.HttpStatus.Ok, content);
                    return;
                }
                else if (p.http_url.StartsWith("/cashier/backside"))
                {
                    string content = this.surfLogic.getBacksideView();
                    p.SendResponse(HttpProcessor.HttpStatus.Ok, content);
                    return;
                }
                else if (p.http_url.StartsWith("/client/sidebar"))
                {
                    string content = this.surfLogic.getSidebarView();
                    p.SendResponse(HttpProcessor.HttpStatus.Ok, content);
                    return;
                }
                else if (p.http_url.StartsWith("/client/main"))
                {
                    string content = this.surfLogic.getClientContentView();
                    p.SendResponse(HttpProcessor.HttpStatus.Ok, content);
                    return;
                }
                else if (p.http_url.StartsWith("/plugins"))
                {
                    string content = this.surfLogic.getPlugins();
                    p.SendResponse(HttpProcessor.HttpStatus.Ok, content);
                    return;
                }
                else if (p.http_url.StartsWith("/locker"))
                {
                    string content = this.surfLogic.getLockerView();
                    p.SendResponse(HttpProcessor.HttpStatus.Ok, content);
                    return;
                }
                else
                {
                    WebPage file;
                    this.surfLogic.getFile(p.http_url, out file);
                    p.SendReponseWithContentType(file);
                    return;
                }

                p.SendResponse(HttpProcessor.HttpStatus.Err, "");
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("GetMethod is forbidden,err is /n" + ex.Message);
            }
        }

        public string Base64Encode(string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            return Convert.ToBase64String(bytes);
        }

        public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData)
        {
            try
            {
                string data = inputData.ReadToEnd();
                LogHelper.WriteLog("request data:" + data);
                RequestDTO request = JsonUtil.DeserializeJsonToObject<RequestDTO>(data);

                // 验证令牌
                if (null == request)
                {
                    p.SendResponse(HttpProcessor.HttpStatus.Err, "");
                    return ;
                }


                if (validateToken(request) == false)
                {
                    p.SendResponse(HttpProcessor.HttpStatus.Err, "");
                    return;
                }


                JObject map  = (JObject)request.Data;

                string responseStr = "";

                if (request.Fn == "activeUser")
                {
                    ActiveData activeData = new ActiveData();

                    JArray activeList = JArray.Parse(map["list"].ToString());

                    foreach (JObject item in activeList)
                    {

                        activeData.Account = item["account"].ToString();
                        activeData.MemberId = UInt64.Parse(item["memberID"].ToString());
                        activeData.PcName = item["machineName"].ToString();
                        activeData.CostType = (CostType)UInt32.Parse(item["costType"].ToString());
                        activeData.AreaTypeId = UInt32.Parse(item["areaType"].ToString());
                        activeData.RoomOwner = false;
                        activeData.RuleId = UInt32.Parse(item["ruleId"].ToString());
                        activeData.RuleValue = float.Parse(item["ruleValue"].ToString());
                        activeData.PeriodStartTime = float.Parse(item["periodBegin"].ToString());
                        activeData.PeriodEndTime = float.Parse(item["periodEnd"].ToString());

                        this.surfLogic.ActiveUser(activeData, ref responseStr, true);
                        break;
                    }


                }
                else if (request.Fn == "logonUser")
                {
                    LoginData loginData = new LoginData();

                    loginData.Account = map["account"].ToString();
                    loginData.MemberId = UInt64.Parse(map["memberID"].ToString());
                    loginData.PcName = map["pcName"].ToString();
                    loginData.Timestamp = UInt32.Parse(map["tm"].ToString());
                    loginData.Password = map["pwd"].ToString();
                    loginData.LoginType = UInt32.Parse(map["loginType"].ToString());

                    this.surfLogic.PcLoginUser(loginData, ref responseStr);
                }
                else if (request.Fn == "logoffUser")
                {
                    UInt64 memberID = UInt64.Parse(map["memberID"].ToString());
                    LogHelper.WriteLog(request.From + " logoff user:" + memberID);
                    this.surfLogic.LogOffUser(memberID, ref responseStr, request.isFromCashier(), false, true);
                }
                else if (request.Fn == "queryOnlineUserList")
                {
                    this.surfLogic.QueryOnlineUserList(ref responseStr);
                }
                else if (request.Fn == "stopAllUserCost")
                {
                    this.surfLogic.StopAllUserCost(ref responseStr);
                }
                else if (request.Fn == "pcHeart")
                {
                    UInt64 memberID = UInt64.Parse(map["memberID"].ToString());

                    SurfPc surfPc = new SurfPc();
                    surfPc.PcInfo = new PcInfo();
                    surfPc.PcName = map["pcName"].ToString();
                    surfPc.PcInfo.Ip = map["pcIp"].ToString();
                    surfPc.PcInfo.Mac = map["pcMac"].ToString();
                    surfPc.PcInfo.State = 1;
                    surfPc.PcInfo.MachineName = surfPc.PcName;

                    this.surfLogic.PcHeart(memberID, surfPc, ref responseStr);
                }
                else if (request.Fn == "queryUser")
                {
                    UInt64 memberID = UInt64.Parse(map["memberID"].ToString());
                    this.surfLogic.QueryUser(memberID, ref responseStr);
                }
                else if (request.Fn == "changePc")
                {
                    UInt64 memberID = UInt64.Parse(map["memberID"].ToString());
                    string pcName = map["newPc"].ToString();
                    UInt32 areaId = UInt32.Parse(map["newAreaType"].ToString());
                    this.surfLogic.ChangePc(memberID, pcName, areaId, ref responseStr);
                }
                else if (request.Fn == "changeCostType")
                {
                    ActiveData activeData = new ActiveData();

                    JArray activeList = JArray.Parse(map["list"].ToString());

                    foreach (JObject item in activeList)
                    {

                        activeData.Account = item["account"].ToString();
                        activeData.MemberId = UInt64.Parse(item["memberID"].ToString());
                        activeData.CostType = (CostType)UInt32.Parse(item["costType"].ToString());
                        activeData.AreaTypeId = UInt32.Parse(item["areaType"].ToString());
                        activeData.RoomOwner = false;
                        activeData.RuleId = UInt32.Parse(item["ruleId"].ToString());
                        activeData.RuleValue = float.Parse(item["ruleValue"].ToString());
                        activeData.PeriodStartTime = float.Parse(item["periodBegin"].ToString());
                        activeData.PeriodEndTime = float.Parse(item["periodEnd"].ToString());

                        this.surfLogic.WeekToPeriodOrDuration(activeData, ref responseStr);

                        break;
                    }
                }
                else if (request.Fn == "serverUserSync" || request.Fn == "synAccount")
                {
                    UInt64 memberId = UInt64.Parse(map["memberID"].ToString());
                    this.surfLogic.SynUserInfo(memberId, ref responseStr);
                }
                else if (request.Fn == "changeToWeek")
                {
                    UInt64 memberId = UInt64.Parse(map["memberID"].ToString());
                    this.surfLogic.changeToWeek(memberId, ref responseStr);
                }
                else if (request.Fn == "aidaOrderCharge")
                {
                    UInt64 memberId = UInt64.Parse(map["memberID"].ToString());
                    UInt64 orderId = UInt64.Parse(map["orderNo"].ToString());
                    float orderCost = float.Parse(map["orderCost"].ToString());
                    float baseCost = float.Parse(map["baseCost"].ToString());


                    this.surfLogic.payOrderByBaseBalance(memberId, orderId, orderCost, baseCost, ref responseStr);
                }
                else if (request.Fn == "addUser")
                {
                    this.surfLogic.AddUser(map, ref responseStr);
                }
                else if (request.Fn == "updateUser")
                {
                    this.surfLogic.UpdateUser(map, ref responseStr);
                }
                else if (request.Fn == "chargeUser")
                {
                    this.surfLogic.ChargeUser(map, ref responseStr);
                }
                else if (request.Fn == "getMemberID")
                {
                    this.surfLogic.GetMemberID(map["query"].ToString(), ref responseStr);
                }
                else if (request.Fn == "fuzzyQueryMemberInfo")
                {
                    this.surfLogic.SearchUser(map["query"].ToString(), ref responseStr);
                }
                else if (request.Fn == "queryMemberInfo")
                {
                    string contype = map["contype"].ToString();
                    string code = map["code"].ToString();

                    this.surfLogic.QueryMemberInfo(contype, code, ref responseStr);
                }
                else if (request.Fn == "queryUserBaseInfo")
                {
                    this.surfLogic.QueryUserBaseInfo(map["account"].ToString(), ref responseStr);
                    //this.surfLogic.QueryUserBaseInfoEx(map["account"].ToString(), ref responseStr);
                }
                else if (request.Fn == "queryDutyCheckInfo")
                {
                    this.surfLogic.GetDutyData(map, ref responseStr);
                }
                else if (request.Fn == "dutyLogin")
                {
                    this.surfLogic.SubmitDutyData(map, ref responseStr);
                }
                else if (request.Fn == "updatePWD")
                {
                    this.surfLogic.UpdatePWD(Convert.ToUInt64(map["memberID"]), map["newPWD"].ToString(), ref responseStr);
                }
                else
                {

                }

                LogHelper.WriteLog("response data:" + responseStr);
                p.SendResponse(HttpProcessor.HttpStatus.Ok,responseStr);

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("post method err:",ex);
                //
            }


        }

        private bool validateToken(RequestDTO dto)
        {

            if (dto.Fn == "pcHeart" || dto.Fn == "getMemberID")
            {
                return true;
            }

            Debug.WriteLine("HttpServer:获取token******{0}", dto.Token);
            string result = MD5Util.EncryptWithMd5(dto.Fn + dto.Tm + IniUtil.key());
            Debug.WriteLine("HttpServer:计算token******{0}", result);
            if (dto.Token.Equals(result))
            {
                return true;
            }

            return false;
        }
    }
}
