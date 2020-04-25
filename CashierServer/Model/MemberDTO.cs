using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierServer.Model
{
	public class MemberDTO
	{
		private long memberid;

		private String account;

		private String memberName;

		private String birthDay;

        private int sex;

		private String password;

		private String phone;

		private String qq;

		private String openID;

		private Byte certificateType;

		private String identifyPath;

		private String certificateNum;

		private byte memberType;

		private int? lastUpdate;

		private int gid;

		private Byte? proviceID;

		private int? cityID;

		private int? districtID;

		private String address;

		private long idseq;

		private double baseBalance;

		private double awardBalance;

		private int shopId;

		private int onlineState;

		private String onlineArea;

		private String onlineMachine;

        public String lastUpdateDate { get; set; }
        
		public long Memberid
		{
			get
			{
				return memberid;
			}

			set
			{
				memberid = value;
			}
		}

		public string Account
		{
			get
			{
				return account;
			}

			set
			{
				account = value;
			}
		}

		public string MemberName
		{
			get
			{
				return memberName;
			}

			set
			{
				memberName = value;
			}
		}

		public string BirthDay
		{
			get
			{
				return birthDay;
			}

			set
			{
				birthDay = value;
			}
		}

        public int Sex
		{
			get
			{
				return sex;
			}

			set
			{
				sex = value;
			}
		}

		public string Password
		{
			get
			{
				return password;
			}

			set
			{
				password = value;
			}
		}

		public string Phone
		{
			get
			{
				return phone;
			}

			set
			{
				phone = value;
			}
		}

		public string Qq
		{
			get
			{
				return qq;
			}

			set
			{
				qq = value;
			}
		}

		public string OpenID
		{
			get
			{
				return openID;
			}

			set
			{
				openID = value;
			}
		}

		public byte CertificateType
		{
			get
			{
				return certificateType;
			}

			set
			{
				certificateType = value;
			}
		}

		public string IdentifyPath
		{
			get
			{
				return identifyPath;
			}

			set
			{
				identifyPath = value;
			}
		}

		public string CertificateNum
		{
			get
			{
				return certificateNum;
			}

			set
			{
				certificateNum = value;
			}
		}

		public byte MemberType
		{
			get
			{
				return memberType;
			}

			set
			{
				memberType = value;
			}
		}

		public int? LastUpdate
		{
			get
			{
				return lastUpdate;
			}

			set
			{
				lastUpdate = value;
			}
		}

		public int Gid
		{
			get
			{
				return gid;
			}

			set
			{
				gid = value;
			}
		}

		public byte? ProviceID
		{
			get
			{
				return proviceID;
			}

			set
			{
				proviceID = value;
			}
		}

		public int? CityID
		{
			get
			{
				return cityID;
			}

			set
			{
				cityID = value;
			}
		}

		public int? DistrictID
		{
			get
			{
				return districtID;
			}

			set
			{
				districtID = value;
			}
		}

		public string Address
		{
			get
			{
				return address;
			}

			set
			{
				address = value;
			}
		}

		public long Idseq
		{
			get
			{
				return idseq;
			}

			set
			{
				idseq = value;
			}
		}

		public double BaseBalance
		{
			get
			{
				return baseBalance;
			}

			set
			{
				baseBalance = value;
			}
		}

		public double AwardBalance
		{
			get
			{
				return awardBalance;
			}

			set
			{
				awardBalance = value;
			}
		}

		public int ShopId
		{
			get
			{
				return shopId;
			}

			set
			{
				shopId = value;
			}
		}

		public int OnlineState
		{
			get
			{
				return onlineState;
			}

			set
			{
				onlineState = value;
			}
		}

		public string OnlineArea
		{
			get
			{
				return onlineArea;
			}

			set
			{
				onlineArea = value;
			}
		}

		public string OnlineMachine
		{
			get
			{
				return onlineMachine;
			}

			set
			{
				onlineMachine = value;
			}
		}

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
;
            sb.Append("memberName:");
            sb.Append(memberName);
            sb.Append("birthday:");
            sb.Append(birthDay);
            sb.Append("sex:");
            sb.Append(sex);
            sb.Append("password:");
            sb.Append(password);
            sb.Append("phone:");
            sb.Append(phone);
            sb.Append("qq:");
            sb.Append(qq);
            sb.Append("openID:");
            sb.Append(openID);
            sb.Append("certificateType:");
            sb.Append(certificateType);
            sb.Append("identifyPath:");
            sb.Append(identifyPath);
            sb.Append("certificateNum:");
            sb.Append(certificateNum);
            sb.Append("lastUpdate:");
            sb.Append(lastUpdate);
            sb.Append("memberType:");
            sb.Append(MemberType);
            sb.Append("gid:");
            sb.Append(gid);
            sb.Append("proviceID:");
            sb.Append(proviceID);
            sb.Append("cityID:");
            sb.Append(cityID);
            sb.Append("districtID:");
            sb.Append(districtID);
            sb.Append("address:");
            sb.Append(address);
            sb.Append("memberid:");
            sb.Append(memberid);
            sb.Append(",account:");
            sb.Append(account);

            return sb.ToString();
        }
    }
}
