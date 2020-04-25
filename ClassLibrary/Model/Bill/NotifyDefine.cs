using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierLibrary.Model.Bill
{
    public class NotifyDefine
    {

        public const string NOTIFY_UPT_USER = "UpdateUser";
        public const string NOTIFY_UPT_CONFIG = "UpdateConfig";



        public const string NOTIFY_UPT_SN_MODIFYUSER = "ModifyUserInfo";
        public const string NOTIFY_UPT_SN_SYNBALANCE = "SynBalance";
        public const string NOTIFY_UPT_SN_SYNGOODSCOST = "SynGoodsCost";
        public const string NOTIFY_UPT_SN_CANCELGOODORDER = "CancelGoodOrder";
        public const string NOTIFY_UPT_SN_USERACTIVE = "UserActive";
        public const string NOTIFY_UPT_SN_USERLOGON = "UserPcLogon";
        public const string NOTIFY_UPT_SN_USERLOGOFF = "UserPcLogoff";
        public const string NOTIFY_UPT_SN_PCINVALID = "PcInvalid";
        public const string NOTIFY_UPT_SN_CHANGEPC = "UpdatePc";
        public const string NOTIFY_UPT_SN_CORRECTRECHARGE = "CorrectRecharge";
        public const string NOTIFY_UPT_SN_UPDATEGOODORDER = "UpdateGoodOrder";

    }
}
