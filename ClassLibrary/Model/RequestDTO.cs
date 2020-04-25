using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierLibrary.Model
{
	/// <summary>
	/// 请求数据
	/// </summary>
	public class RequestDTO
	{

		private String fn;

		private String tm;

		private String ver;

		private String token;

        public string From { get; set; }

        public bool isFromCashier()
        {
            return From == "cashier";
        }

		private Object data;

		/// <summary>
		/// 方法参数
		/// </summary>
		public string Fn
		{
			get
			{
				return fn;
			}

			set
			{
				fn = value;
			}
		}

		/// <summary>
		/// 时间戳
		/// </summary>
		public string Tm
		{
			get
			{
				return tm;
			}

			set
			{
				tm = value;
			}
		}

		/// <summary>
		/// 版本
		/// </summary>
		public string Ver
		{
			get
			{
				return ver;
			}

			set
			{
				ver = value;
			}
		}

		/// <summary>
		/// 验证令牌
		/// </summary>
		public string Token
		{
			get
			{
				return token;
			}

			set
			{
				token = value;
			}
		}

		/// <summary>
		/// Json数据
		/// </summary>
		public Object Data
		{
			get
			{
				return data;
			}

			set
			{
				data = value;
			}
		}
	}
}
