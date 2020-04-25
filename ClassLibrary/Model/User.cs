using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashierLibrary.Model
{
    public class User
    {

        public enum ChainState {  Chain = 1, Single = 2 };
        
		public int id
        {
            get;
            set;
        }

        public String loginname
        {
            get;
            set;
        }

        public String username
        {
            get;
            set;
        }

        public String password
        {
            get;
            set;
        }

        public int shopid
        {
            get;
            set;
        }

        public String shopName
        {
            get;
            set;
        }
        
		/// <summary>
		/// 是否连锁
		/// </summary>
		public int IsChain
		{
            get;
            set;
		}

        public bool isShopInChain()
        {
            if(this.IsChain == (int)ChainState.Chain)
            {
                return true;
            }
            return false;
        }

        public bool isShopInSingle()
        {
            if (this.IsChain == (int)ChainState.Single)
            {
                return true;
            }
            return false;
        }
        
	}
}
