using CashierLibrary.Model.Bill;
using CashierLibrary.Util;
using CashierServer.Model;
using CashierServer.Util;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CashierServer.DAO
{
    /// <summary>
	/// 会员处理数据连接类
	/// </summary>
	public class MemberDao : BaseDao
    {

        /// <summary>
        /// 插入会员
        /// </summary>
        /// <param name="dto">会员DTO</param>
        public bool InsertMember(MemberDTO dto)
        {
            //return false;
            // 获取序列号
            int idseq = 0;

            if (dto.Memberid == 0)
            {
                idseq = this.GetseqMember();
                if (idseq == 0)
                {
                    LogHelper.WriteLog("插入会员 【" + dto.MemberName + "】 出错,获取系列号失败");
                    return false;
                }
            }
            else
            {
                string memberId = (dto.Memberid + "");
                idseq = Int32.Parse(memberId.Remove(0,memberId.Length - 7));
            }
            

            CDbMysql mysql = getConnection();
            try
            {
                List<MySqlParameter> parameters = new List<MySqlParameter>();

                MySqlParameter memberNameParameter = new MySqlParameter("@memberName_", MySqlDbType.VarChar, 30);
                memberNameParameter.Value = dto.MemberName;
                memberNameParameter.Direction = ParameterDirection.Input;
                parameters.Add(memberNameParameter);
                
                MySqlParameter birthdayParameter = new MySqlParameter("@birthday_", MySqlDbType.VarChar, 20);
                birthdayParameter.Value = dto.BirthDay;
                birthdayParameter.Direction = ParameterDirection.Input;
                parameters.Add(birthdayParameter);

                MySqlParameter sexParameter = new MySqlParameter("@sex_", MySqlDbType.Byte);
                sexParameter.Value = dto.Sex;
                sexParameter.Direction = ParameterDirection.Input;
                parameters.Add(sexParameter);


                MySqlParameter passwordParameter = new MySqlParameter("@password_", MySqlDbType.VarChar, 32);
                passwordParameter.Value = dto.Password;
                passwordParameter.Direction = ParameterDirection.Input;
                parameters.Add(passwordParameter);

                MySqlParameter phoneParameter = new MySqlParameter("@phone_", MySqlDbType.VarChar, 11);
                phoneParameter.Value = dto.Phone;
                phoneParameter.Direction = ParameterDirection.Input;
                parameters.Add(phoneParameter);

                MySqlParameter qqParameter = new MySqlParameter("@qq_", MySqlDbType.VarChar, 30);
                qqParameter.Value = dto.Qq;
                qqParameter.Direction = ParameterDirection.Input;
                parameters.Add(qqParameter);

                MySqlParameter openIDParameter = new MySqlParameter("@openID_", MySqlDbType.VarChar, 40);
                openIDParameter.Value = dto.OpenID;
                openIDParameter.Direction = ParameterDirection.Input;
                parameters.Add(openIDParameter);

                MySqlParameter certificateTypeParameter = new MySqlParameter("@certificateType_", MySqlDbType.Byte);
                certificateTypeParameter.Value = dto.CertificateType;
                certificateTypeParameter.Direction = ParameterDirection.Input;
                parameters.Add(certificateTypeParameter);

                MySqlParameter identifyPathParameter = new MySqlParameter("@identifyPath_", MySqlDbType.VarChar, 1000);
                identifyPathParameter.Value = dto.IdentifyPath;
                identifyPathParameter.Direction = ParameterDirection.Input;
                parameters.Add(identifyPathParameter);

                MySqlParameter certificateNumParameter = new MySqlParameter("@certificateNum_", MySqlDbType.VarChar, 20);
                certificateNumParameter.Value = dto.CertificateNum;
                certificateNumParameter.Direction = ParameterDirection.Input;
                parameters.Add(certificateNumParameter);

                MySqlParameter memberTypeParameter = new MySqlParameter("@memberType_", MySqlDbType.UInt16);
                memberTypeParameter.Value = dto.MemberType;
                memberTypeParameter.Direction = ParameterDirection.Input;
                parameters.Add(memberTypeParameter);

                MySqlParameter lastUpdateParameter = new MySqlParameter("@lastUpdate_", MySqlDbType.Int32);
                if (null == dto.LastUpdate)
                {
                    dto.LastUpdate = 0;
                }
                lastUpdateParameter.Value = dto.LastUpdate;
                lastUpdateParameter.Direction = ParameterDirection.Input;
                parameters.Add(lastUpdateParameter);

                MySqlParameter gidParameter = new MySqlParameter("@gid_", MySqlDbType.Int32);
                gidParameter.Value = dto.Gid;
                gidParameter.Direction = ParameterDirection.Input;
                parameters.Add(gidParameter);

                MySqlParameter proviceIDParameter = new MySqlParameter("@proviceID_", MySqlDbType.Byte);
                if (null == dto.ProviceID)
                {
                    dto.ProviceID = 0;
                }
                proviceIDParameter.Value = dto.ProviceID;
                proviceIDParameter.Direction = ParameterDirection.Input;
                parameters.Add(proviceIDParameter);

                MySqlParameter cityIDParameter = new MySqlParameter("@cityID_", MySqlDbType.Int32);
                if (null == dto.CityID)
                {
                    dto.CityID = 0;
                }
                cityIDParameter.Value = dto.CityID;
                cityIDParameter.Direction = ParameterDirection.Input;
                parameters.Add(cityIDParameter);

                MySqlParameter districtIDParameter = new MySqlParameter("@districtID_", MySqlDbType.Int32);
                if (null == dto.DistrictID)
                {
                    dto.DistrictID = 0;
                }
                districtIDParameter.Value = dto.DistrictID;
                districtIDParameter.Direction = ParameterDirection.Input;
                parameters.Add(districtIDParameter);

                MySqlParameter addressParameter = new MySqlParameter("@address_", MySqlDbType.VarChar, 100);
                addressParameter.Value = dto.Address;
                addressParameter.Direction = ParameterDirection.Input;
                parameters.Add(addressParameter);

                MySqlParameter accountParameter = new MySqlParameter("@account_", MySqlDbType.VarChar, 30);
                accountParameter.Value = dto.Account;
                accountParameter.Direction = ParameterDirection.Input;
                parameters.Add(accountParameter);

                MySqlParameter seqParameter = new MySqlParameter("@idseq", MySqlDbType.Int32);
                seqParameter.Value = idseq;
                seqParameter.Direction = ParameterDirection.Input;
                parameters.Add(seqParameter);


                MySqlParameter baseParameter = new MySqlParameter("@baseBalance_", MySqlDbType.Decimal);
                baseParameter.Value = dto.BaseBalance;
                baseParameter.Direction = ParameterDirection.Input;
                parameters.Add(baseParameter);

                MySqlParameter awardParameter = new MySqlParameter("@awardBalance_", MySqlDbType.Decimal);
                awardParameter.Value = dto.AwardBalance;
                awardParameter.Direction = ParameterDirection.Input;
                parameters.Add(awardParameter);


                MySqlParameter result = new MySqlParameter("@result", MySqlDbType.Int32);
                result.Value = -1;
                result.Direction = ParameterDirection.Output;
                parameters.Add(result);

                DataTable dt = mysql.ExcProcedure("addMember4Net", parameters.ToArray());
                if ((result.Value) != null && (Convert.ToInt32(result.Value) == 1))
                {
                    return true;
                }
                else
                {
                    LogHelper.WriteLog("插入会员 【" + dto.MemberName + "】 出错,错误码:\n" + Convert.ToInt32(null == result.Value ? "-1" : result.Value));
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("插入会员数据出错:\n", ex);
            }
            finally
            {
                this.releaseConnection(mysql);
            }
            return false;
        }

        public IDictionary<string, object> AddMember(MemberDTO dto)
        {

            IDictionary<string, object> root = new Dictionary<string, object>();

            // 获取序列号
            int idseq = this.GetseqMember();
            if (idseq == 0)
            {
                root["state"] = -1;
                root["info"] = "序列号生成失败";
                return root;
            }

            CDbMysql mysql = getConnection();
           
            try
            {

                List<MySqlParameter> parameters = new List<MySqlParameter>();

                MySqlParameter memberNameParameter = new MySqlParameter("@memberName_", MySqlDbType.VarChar, 30);
                memberNameParameter.Value = dto.MemberName;
                memberNameParameter.Direction = ParameterDirection.Input;
                parameters.Add(memberNameParameter);

                MySqlParameter birthdayParameter = new MySqlParameter("@birthday_", MySqlDbType.VarChar, 20);
                birthdayParameter.Value = dto.BirthDay;
                birthdayParameter.Direction = ParameterDirection.Input;
                parameters.Add(birthdayParameter);

                MySqlParameter sexParameter = new MySqlParameter("@sex_", MySqlDbType.Byte);
                sexParameter.Value = dto.Sex;
                sexParameter.Direction = ParameterDirection.Input;
                parameters.Add(sexParameter);


                MySqlParameter passwordParameter = new MySqlParameter("@password_", MySqlDbType.VarChar, 32);
                passwordParameter.Value = dto.Password;
                passwordParameter.Direction = ParameterDirection.Input;
                parameters.Add(passwordParameter);

                MySqlParameter phoneParameter = new MySqlParameter("@phone_", MySqlDbType.VarChar, 11);
                phoneParameter.Value = dto.Phone;
                phoneParameter.Direction = ParameterDirection.Input;
                parameters.Add(phoneParameter);

                MySqlParameter qqParameter = new MySqlParameter("@qq_", MySqlDbType.VarChar, 30);
                qqParameter.Value = dto.Qq;
                qqParameter.Direction = ParameterDirection.Input;
                parameters.Add(qqParameter);

                MySqlParameter openIDParameter = new MySqlParameter("@openID_", MySqlDbType.VarChar, 40);
                openIDParameter.Value = dto.OpenID;
                openIDParameter.Direction = ParameterDirection.Input;
                parameters.Add(openIDParameter);

                MySqlParameter certificateTypeParameter = new MySqlParameter("@certificateType_", MySqlDbType.Byte);
                certificateTypeParameter.Value = dto.CertificateType;
                certificateTypeParameter.Direction = ParameterDirection.Input;
                parameters.Add(certificateTypeParameter);

                MySqlParameter identifyPathParameter = new MySqlParameter("@identifyPath_", MySqlDbType.VarChar, 1000);
                identifyPathParameter.Value = dto.IdentifyPath;
                identifyPathParameter.Direction = ParameterDirection.Input;
                parameters.Add(identifyPathParameter);

                MySqlParameter certificateNumParameter = new MySqlParameter("@certificateNum_", MySqlDbType.VarChar, 20);
                certificateNumParameter.Value = dto.CertificateNum;
                certificateNumParameter.Direction = ParameterDirection.Input;
                parameters.Add(certificateNumParameter);

                MySqlParameter memberTypeParameter = new MySqlParameter("@memberType_", MySqlDbType.UInt16);
                memberTypeParameter.Value = dto.MemberType;
                memberTypeParameter.Direction = ParameterDirection.Input;
                parameters.Add(memberTypeParameter);

                MySqlParameter lastUpdateParameter = new MySqlParameter("@lastUpdate_", MySqlDbType.Int32);
                if (null == dto.LastUpdate)
                {
                    dto.LastUpdate = 0;
                }
                lastUpdateParameter.Value = dto.LastUpdate;
                lastUpdateParameter.Direction = ParameterDirection.Input;
                parameters.Add(lastUpdateParameter);

                MySqlParameter gidParameter = new MySqlParameter("@gid_", MySqlDbType.Int32);
                gidParameter.Value = dto.Gid;
                gidParameter.Direction = ParameterDirection.Input;
                parameters.Add(gidParameter);

                MySqlParameter proviceIDParameter = new MySqlParameter("@proviceID_", MySqlDbType.Byte);
                if (null == dto.ProviceID)
                {
                    dto.ProviceID = 0;
                }
                proviceIDParameter.Value = dto.ProviceID;
                proviceIDParameter.Direction = ParameterDirection.Input;
                parameters.Add(proviceIDParameter);

                MySqlParameter cityIDParameter = new MySqlParameter("@cityID_", MySqlDbType.Int32);
                if (null == dto.CityID)
                {
                    dto.CityID = 0;
                }
                cityIDParameter.Value = dto.CityID;
                cityIDParameter.Direction = ParameterDirection.Input;
                parameters.Add(cityIDParameter);

                MySqlParameter districtIDParameter = new MySqlParameter("@districtID_", MySqlDbType.Int32);
                if (null == dto.DistrictID)
                {
                    dto.DistrictID = 0;
                }
                districtIDParameter.Value = dto.DistrictID;
                districtIDParameter.Direction = ParameterDirection.Input;
                parameters.Add(districtIDParameter);

                MySqlParameter addressParameter = new MySqlParameter("@address_", MySqlDbType.VarChar, 100);
                addressParameter.Value = dto.Address;
                addressParameter.Direction = ParameterDirection.Input;
                parameters.Add(addressParameter);

                MySqlParameter accountParameter = new MySqlParameter("@account_", MySqlDbType.VarChar, 30);
                accountParameter.Value = dto.Account;
                accountParameter.Direction = ParameterDirection.Input;
                parameters.Add(accountParameter);

                MySqlParameter seqParameter = new MySqlParameter("@idseq", MySqlDbType.UInt64);
                seqParameter.Value = idseq;
                seqParameter.Direction = ParameterDirection.Input;
                parameters.Add(seqParameter);

                MySqlParameter result = new MySqlParameter("@result", MySqlDbType.Int32);
                result.Value = -1;
                result.Direction = ParameterDirection.Output;
                parameters.Add(result);

                DataTable dt = mysql.ExcProcedure("addMember", parameters.ToArray());
                if (result.Value != null)
                {
                    IDictionary<string, object> data = new Dictionary<string, object>();
                    if (Convert.ToInt32(result.Value) == 1)
                    {
                        data["memberID"] = Convert.ToDouble(dt.Rows[0]["memberID_"] == null ? "0" : dt.Rows[0]["memberID_"].ToString());

                        LogHelper.WriteLog("开卡成功，会员账号：" + dto.Account.ToString() + ",会员卡号：" + data["memberID"].ToString());

                        root["state"] = 0;
                        root["info"] = "成功";
                        root["data"] = data;
                        return root;
                    }
                    data["memberID"] = "0";

                    root["state"] = Convert.ToInt32(result.Value);
                    root["data"] = data;

                    if (Convert.ToInt32(result.Value) == 10624)
                    {
                        root["info"] = "账号重复";
                    }
                    else if (Convert.ToInt32(result.Value) == 10623)
                    {
                        root["info"] = "证件号重复";
                    }
                    else if (Convert.ToInt32(result.Value) == 10622)
                    {
                        root["info"] = "手机号重复";
                    }
                    else
                    {
                        root["state"] = -1;
                        root["info"] = "开卡失败";
                    }
                    return root;
                }
                else
                {
                    LogHelper.WriteLog("插入会员 【" + dto.MemberName + "】 出错,错误码:\n" + Convert.ToInt32(null == result.Value ? "-1" : result.Value));
                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("插入会员数据出错:\n", ex);
            }
            finally
            {
                this.releaseConnection(mysql);
            }

            root["state"] = -1;
            root["info"] = "添加会员失败";
            return root;
        }

        public IDictionary<string, object> UpdateMember(MemberDTO dto)
        {
            IDictionary<string, object> root = new Dictionary<string, object>();
            CDbMysql mysql = getConnection();
            try
            {
                List<MySqlParameter> parameters = new List<MySqlParameter>();

                MySqlParameter memberNameParameter = new MySqlParameter("@memberName_", MySqlDbType.VarChar, 30);
                memberNameParameter.Value = dto.MemberName;
                memberNameParameter.Direction = ParameterDirection.Input;
                parameters.Add(memberNameParameter);

                MySqlParameter birthdayParameter = new MySqlParameter("@birthday_", MySqlDbType.VarChar, 20);
                birthdayParameter.Value = dto.BirthDay;
                birthdayParameter.Direction = ParameterDirection.Input;
                parameters.Add(birthdayParameter);

                MySqlParameter sexParameter = new MySqlParameter("@sex_", MySqlDbType.Byte);
                sexParameter.Value = dto.Sex;
                sexParameter.Direction = ParameterDirection.Input;
                parameters.Add(sexParameter);

                MySqlParameter phoneParameter = new MySqlParameter("@phone_", MySqlDbType.VarChar, 11);
                phoneParameter.Value = dto.Phone;
                phoneParameter.Direction = ParameterDirection.Input;
                parameters.Add(phoneParameter);

                MySqlParameter qqParameter = new MySqlParameter("@qq_", MySqlDbType.VarChar, 30);
                qqParameter.Value = dto.Qq;
                qqParameter.Direction = ParameterDirection.Input;
                parameters.Add(qqParameter);

                MySqlParameter openIDParameter = new MySqlParameter("@openID_", MySqlDbType.VarChar, 40);
                openIDParameter.Value = dto.OpenID;
                openIDParameter.Direction = ParameterDirection.Input;
                parameters.Add(openIDParameter);

                MySqlParameter certificateTypeParameter = new MySqlParameter("@certificateType_", MySqlDbType.Byte);
                certificateTypeParameter.Value = dto.CertificateType;
                certificateTypeParameter.Direction = ParameterDirection.Input;
                parameters.Add(certificateTypeParameter);

                MySqlParameter identifyPathParameter = new MySqlParameter("@identifyPath_", MySqlDbType.VarChar, 1000);
                identifyPathParameter.Value = dto.IdentifyPath;
                identifyPathParameter.Direction = ParameterDirection.Input;
                parameters.Add(identifyPathParameter);

                MySqlParameter certificateNumParameter = new MySqlParameter("@certificateNum_", MySqlDbType.VarChar, 20);
                certificateNumParameter.Value = dto.CertificateNum;
                certificateNumParameter.Direction = ParameterDirection.Input;
                parameters.Add(certificateNumParameter);

                MySqlParameter memberTypeParameter = new MySqlParameter("@memberType_", MySqlDbType.UInt16);
                memberTypeParameter.Value = dto.MemberType;
                memberTypeParameter.Direction = ParameterDirection.Input;
                parameters.Add(memberTypeParameter);

                MySqlParameter lastUpdateParameter = new MySqlParameter("@lastUpdate_", MySqlDbType.Int32);
                if (null == dto.LastUpdate)
                {
                    dto.LastUpdate = 0;
                }
                lastUpdateParameter.Value = dto.LastUpdate;
                lastUpdateParameter.Direction = ParameterDirection.Input;
                parameters.Add(lastUpdateParameter);

                MySqlParameter proviceIDParameter = new MySqlParameter("@proviceID_", MySqlDbType.Byte);
                if (null == dto.ProviceID)
                {
                    dto.ProviceID = 0;
                }
                proviceIDParameter.Value = dto.ProviceID;
                proviceIDParameter.Direction = ParameterDirection.Input;
                parameters.Add(proviceIDParameter);

                MySqlParameter cityIDParameter = new MySqlParameter("@cityID_", MySqlDbType.Int32);
                if (null == dto.CityID)
                {
                    dto.CityID = 0;
                }
                cityIDParameter.Value = dto.CityID;
                cityIDParameter.Direction = ParameterDirection.Input;
                parameters.Add(cityIDParameter);

                MySqlParameter districtIDParameter = new MySqlParameter("@districtID_", MySqlDbType.Int32);
                if (null == dto.DistrictID)
                {
                    dto.DistrictID = 0;
                }
                districtIDParameter.Value = dto.DistrictID;
                districtIDParameter.Direction = ParameterDirection.Input;
                parameters.Add(districtIDParameter);

                MySqlParameter addressParameter = new MySqlParameter("@address_", MySqlDbType.VarChar, 100);
                addressParameter.Value = dto.Address;
                addressParameter.Direction = ParameterDirection.Input;
                parameters.Add(addressParameter);

                MySqlParameter memberIdParameter = new MySqlParameter("@memberID_", MySqlDbType.UInt64);
                memberIdParameter.Value = dto.Memberid;
                memberIdParameter.Direction = ParameterDirection.Input;
                parameters.Add(memberIdParameter);

                MySqlParameter pwdParameter = new MySqlParameter("@password_", MySqlDbType.VarChar, 32);
                if (null == dto.Password)
                {
                    dto.Password = "";
                }
                pwdParameter.Value = dto.Password;
                pwdParameter.Direction = ParameterDirection.Input;
                parameters.Add(pwdParameter);

                MySqlParameter gidParameter = new MySqlParameter("@gid_", MySqlDbType.Int32);
                gidParameter.Value = dto.Gid;
                gidParameter.Direction = ParameterDirection.Input;
                parameters.Add(gidParameter);

                MySqlParameter result = new MySqlParameter("@result", MySqlDbType.Int32);
                result.Value = -1;
                result.Direction = ParameterDirection.Output;
                parameters.Add(result);

                DataTable dt = mysql.ExcProcedure("updateMember", parameters.ToArray());
                if (result.Value != null)
                {
                    if (Convert.ToInt32(result.Value) == 1)
                    {
                        root["state"] = 0;
                        root["info"] = "成功";
                        return root;
                    }
                    else if (Convert.ToInt32(result.Value) == 1403)
                    {
                        root["state"] = 1403;
                        root["info"] = "会员不存在";
                        return root;
                    }
                }
                else
                {
                    LogHelper.WriteLog("修改会员 【" + dto.MemberName + "】 出错,错误码:\n" + Convert.ToInt32(null == result.Value ? "-1" : result.Value));
                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("修改会员数据出错:\n", ex);
            }
            finally
            {
                this.releaseConnection(mysql);
            }

            root["state"] = -1;
            root["info"] = "修改会员失败";
            return root;
        }

        public bool chargeUser(UInt64 memberID, int rechargeWay, int awardFeeType, int rechargeType, int state, int cashierID, int orderSource, int rechargeSource, int rechargeCompaignID, double rechargeFee, double awardFee, double cashBalance)
        {

            return true;
        }

		/// <summary>
		/// 更新用户本金
		/// </summary>
		/// <param name="dto">用户信息</param>
		/// <returns></returns>
		public bool updateMemberBaseBalance(MemberDTO dto)
		{
            if (dto == null)
            {
                return false; 
            }
			String sql = String.Format("update netbar_member_account set baseBalance={0} where memberID={1} and gid={2} and lastUpdateTime < '{3}'",dto.BaseBalance,dto.Memberid,dto.Gid,dto.lastUpdateDate);
			int nRet = this.execute(sql);
            
            LogHelper.WriteLog("updateMemberBaseBalance sql:" + sql + ",ret:" + nRet);
			return true;	 
		}

        public bool ChangeMemberPwd(UInt64 memberId, string pwd)
        {
            string memberSql = "update netbar_member set password='" + pwd + "' where memberID=" + memberId;
            return this.execute(memberSql) > 0;
        }

		/// <summary>
		/// 更新会员信息
		/// </summary>
		/// <param name="dto"></param>
		/// <returns></returns>
		public bool updateMemberInfo(MemberDTO dto)
		{

            if (dto == null)
            {
                return false;
            }

			String memSql = "update netbar_member set ifBindingWX=0";
			if (!string.IsNullOrEmpty(dto.OpenID))
			{
				memSql += ",openid='" + dto.OpenID  + "'";
			}
			if (dto.MemberType > 0)
			{
				memSql += ",memberType= " + dto.MemberType;
			}
            memSql += " where memberID=" + dto.Memberid + " and gid=" + dto.Gid + " and lastUpdateDate < '" + timestampToDateStr(dto.lastUpdateDate) + "'";
            
			int nRet = this.execute(memSql);
            LogHelper.WriteLog("updateMemberInfo sql:" + memSql + ",nRet:" + nRet);
			return true;
		}

        /// <summary>
        /// 更新会员信息（新）
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public bool updateMemberInfoNew(MemberDTO dto)
        {

            if (dto == null)
            {
                return false;
            }

            String memSql = "update netbar_member set openid='" + dto.OpenID + "'";

            if (dto.MemberType > 0)
            {
                memSql += ",memberType= " + dto.MemberType;
            }
            else
            {
                memSql += ",memberType= IF(memberType > 1, memberType, 3) ";
            }
            memSql += ",lastUpdateDate = now() where memberID=" + dto.Memberid + " and gid=" + dto.Gid + " and lastUpdateDate < '" + dto.lastUpdateDate + "'";

            int nRet = this.execute(memSql);
            LogHelper.WriteLog("updateMemberInfo sql:" + memSql + ",nRet:" + nRet);
            return true;
        }

        private string timestampToDateStr(string datestr)
        {
            long millineseconds = 0;

            try
            {
                millineseconds = long.Parse(datestr);
            }
            catch (Exception ex)
            {
                //LogHelper.WriteLog("timestampToDateStr", ex);
            }
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return dtStart.AddMilliseconds(millineseconds).ToString("yyyy-MM-dd HH:mm:ss");
        }

        private bool isDate(string str)
        {
            try
            {
                DateTime.Parse(str);
                return true;
            }
            catch (Exception )
            {
                return false;
            }
        }
        


		/// <summary>
		/// 获取序列号
		/// </summary>
		/// <returns></returns>
		private int GetseqMember()
		{
			List<MySqlParameter> parameters = new List<MySqlParameter>();
			MySqlParameter memberNameParameter = new MySqlParameter("@tableName_", MySqlDbType.VarChar, 30);
			memberNameParameter.Value = "seq_member";
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
                LogHelper.WriteLog("get seq member err:",ex);
            }
            finally
            {
                this.releaseConnection(mysql);
            }
			return 0;
		}

        public IDictionary<string, object> queryMemberInfo(string account)
        {
            string sql = "select m.memberID,m.account,m.memberName,m.password,m.sex,m.birthday,m.phone,m.qq,m.openID,m.memberType,a.baseBalance,a.awardBalance,a.deposit as cashBalance from netbar_member m left join netbar_member_account a on m.memberID = a.memberID where m.account='" + account + "'";
            return this.selectOne(sql);
        }

        public void updateMaxMemberID(Int64 memberId)
        {
            string sql = "update seq_member set id=" + memberId + " where id < " + memberId ;
            this.execute(sql);
        }

        public IDictionary<string, object> queryMemberInfo(UInt64 memberID)
        {
            string sql = "select m.memberID,m.account,m.memberName,m.password,m.sex,m.birthday,m.phone,m.qq,m.openID,m.memberType,a.baseBalance,a.awardBalance,a.deposit as cashBalance from netbar_member m left join netbar_member_account a on m.memberID = a.memberID where m.memberID=" + memberID;
            return this.selectOne(sql);
        }

        public List<IDictionary<string, object>> fuzzyQueryMember(string query)
        {
            string sql = "select memberID,memberName,account,memberType,phone,birthday,certificateType,certificateNum from netbar_member where memberName like " + "'%"+query+"%' or account like " + "'%" + query + "%' or phone like " + "'%" + query + "%' limit 100" ;
            LogHelper.WriteLog("fuzzyQueryMember SQL: " + sql);
            return this.selectList(sql);
        }
        
        public bool queryMemberIDByAccount(string account,ref UInt64 memberID)
        {
            IDictionary<string,object> data = this.selectOne("select memberID from netbar_member where account = '" + account + "'");

            if (data == null)
            {
                return false;
            }
            object obj;
            if (data.TryGetValue("memberID", out obj))
            {
                memberID = UInt64.Parse(obj.ToString());
                return true;
            }
            return false;
        }

        #region 检测是否已创建会员信息


        public bool queryMemberInfoByCode(string contype, string code, ref UInt64 memberID)
        {
            string readSql = "select memberID from netbar_member ";
            if (contype == "1")
            {
               readSql += "where account = '" + code + "'";
            }
            else if (contype == "2")
            {
                readSql += "where openID = '" + code + "'";
            }
            else
            {
                return false;
            }

            IDictionary<string, object> data = this.selectOne(readSql);
            if (data == null)
            {
                return false;
            }

            object obj;
            if (data.TryGetValue("memberID", out obj))
            {
                memberID = UInt64.Parse(obj.ToString());
                return true;
            }
            return false;
        }

        #endregion


        public bool PayOrderByBaseBalance(UInt32 gid, UInt64 memberId, UInt64 orderId, float orderCost, float baseCost)
        {
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            CDbMysql mysql = this.getConnection();
            try
            {
                MySqlParameter goodsOrderID_ = new MySqlParameter("@goodsOrderID_", MySqlDbType.UInt64);
                goodsOrderID_.Value = orderId;
                goodsOrderID_.Direction = ParameterDirection.Input;
                parameters.Add(goodsOrderID_);

                MySqlParameter memberID_ = new MySqlParameter("@memberID_", MySqlDbType.UInt64);
                memberID_.Value = memberId;
                memberID_.Direction = ParameterDirection.Input;
                parameters.Add(memberID_);

                MySqlParameter orderFee_ = new MySqlParameter("@orderFee_", MySqlDbType.Float);
                orderFee_.Value = orderCost;
                orderFee_.Direction = ParameterDirection.Input;
                parameters.Add(orderFee_);

                MySqlParameter baseBalance_ = new MySqlParameter("@baseBalance_", MySqlDbType.Float);
                baseBalance_.Value = baseCost;
                baseBalance_.Direction = ParameterDirection.Input;
                parameters.Add(baseBalance_);

                MySqlParameter awardBalance_ = new MySqlParameter("@awardBalance_", MySqlDbType.Float);
                awardBalance_.Value = 0;
                awardBalance_.Direction = ParameterDirection.Input;
                parameters.Add(awardBalance_);

                MySqlParameter deposit_ = new MySqlParameter("@deposit_", MySqlDbType.Float);
                deposit_.Value = 0;
                deposit_.Direction = ParameterDirection.Input;
                parameters.Add(deposit_);

                MySqlParameter gid_ = new MySqlParameter("@gid_", MySqlDbType.Int32);
                gid_.Value = gid;
                gid_.Direction = ParameterDirection.Input;
                parameters.Add(gid_);

                MySqlParameter result = new MySqlParameter("@result", MySqlDbType.Int32);
                result.Value = 1;
                result.Direction = ParameterDirection.Output;
                parameters.Add(result);

                DataTable dt = mysql.ExcProcedure("aida_createGoodsOrder", parameters.ToArray());
                if ((result.Value) != null && (Convert.ToInt32(result.Value) == 1))
                {
                    return true;
                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("PayOrderByBaseBalance err:", ex);
            }
            finally
            {
                this.releaseConnection(mysql);
            }
            return false;
        }
	}
}
