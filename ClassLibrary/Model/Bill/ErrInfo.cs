using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierLibrary.Model.Bill
{

    public class ErrDefine
    {
        public const string STR_SUCCESS = "成功";

        public const string STR_NET_ERROR = "网络错误，请稍后再试";

        public const string STR_USER_NOTACTIVE = "用户未激活";
        public const string STR_USER_NOTEXIST = "会员不存在";

        public const string STR_USER_UPDATE_ERROR = "更新会员信息失败";
        public const string STR_USER_CREATE_ERROR = "自动创建连锁会员失败";
        public const string STR_NET_BREAK_OFF = "云服务器网络中断，请暂时使用临时卡上机";

        public const string STR_USER_HASACTIVE = "用户已经激活";
        public const string STR_PC_NOTFOUND = "机器不存在";
        public const string STR_PC_HASUSER = "机器已有用户上机";
        public const string STR_DB_ERROR = "数据库异常";
        public const string STR_MEM_ERROR = "内存申请失败";
        public const string STR_PC_EXISTING_USER = "机器已有用户使用";
        public const string STR_PC_CHANGE_NOT_ALLOWED = "不允许换机，请联系吧台";
        public const string STR_PC_CHANGE_NOT_ALLOWED_TEMPUSER = "临时卡不允许跨区域换机，请联系吧台";
        public const string STR_PC_CHANGE_NOT_ALLOWED_PERIODUSER = "包时期间不允许跨区域换机，请联系吧台";

        public const string STR_ERROR_PARAMS = "请求参数错误";
        public const string STR_ERROR_HAD_LOGON = "用户已上机";
        public const string STR_ERROR_WRONG_AREA = "上机区域与激活指定区域不一致";
        public const string STR_ERROR_AREA_NOTALLOWED = "上机区域不允许该会员类型上机";
        public const string STR_ERROR_INVALIDE_AUTH = "非法授权请求，请尝试重启收银端";
        public const string STR_ERROR_INVALIDE_FROM = "非法请求来源，请确认收银机ip";
        public const string STR_ERROR_CLIENT_TEMPUSER = "临时会员，请到吧台结账";
        public const string STR_ERROR_SERVER_INITING = "服务端服务初始化进行中";
        public const string STR_ERROR_PASSWORD = "用户名或密码错误";
        public const string STR_ERROR_CLIENTLOCK = "用户被锁状态，请到收银端下机";
        public const string STR_ERROR_LOGINREPEAT = "频繁登陆，请稍等";
        public const string STR_ERROR_LOGOFFREPEAT = "频繁下机，请稍等";
        public const string STR_ERROR_NOMONEY = "余额不足，请充值";
        public const string STR_ERROR_AREA_MATCH = "包时区域与上机区域不匹配";

        public const string STR_ERROR_PERIODTIME = "未到此包时段时间";
        public const string STR_NO_BILLING = "尚未设置此会员类型在此区域的费率";


        public const string STR_ERROR_FEEPAY_NOTALLOW = "本店不允许卡扣";

        public const string STR_TRY_LATER = "服务器异常，请稍后再试";


        public const string DUTY_RESP_DB_QUERY_ERROR = "预交班错误";
        public const string DUTY_RESP_DB_QUERY_INFO_ERROR = "预交班信息不足";
        public const string DUTY_RESP_DB_QUERY_ENDTIME_ERROR = "获取班次结束时间信息不足";
        public const string DUTY_RESP_PARA_ERROR = "交班准备工作错误";
        public const string DUTY_RESP_DB_UPDATE_ERROR = "交班错误";
        public const string DUTY_RESP_DB_UPDATE_REPEAT_ERROR = "该班次交接班已经生成过，不能重复生成";
    }
}
