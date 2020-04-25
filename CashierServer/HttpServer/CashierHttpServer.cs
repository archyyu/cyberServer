using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CashierLibrary.HttpServer;
using Newtonsoft.Json.Linq;
using CashierLibrary.Util;
using CashierServer.Util;
using CashierServer.Logic;
using CashierServer.Model;
using System.Reflection;
using System.Diagnostics;
using CashierLibrary.Model;
using CashierServer.Logic.Surf;

namespace CashierServer.HttpServer
{
    public class CashierHttpServer : CashierLibrary.HttpServer.HttpServer
    {

        private const string url = "/cashier/extrachannel";

        public SurfLogic surfLogic { get; set; }
        
        public CashierHttpServer(int port) : base(port)
        {

        }
		
		/// <summary>
		/// Get方法处理函数
		/// </summary>
		/// <param name="p">server进程</param>
        public override void handleGETRequest(HttpProcessor p)
        {
			try
			{
				p.SendResponse(HttpProcessor.HttpStatus.Err, "");
			}
			catch (Exception ex)
			{
				LogHelper.WriteLog("GetMethod is forbidden,err is /n"+ex.Message);
			}
        }

		/// <summary>
		/// 处理post请求
		/// </summary>
		/// <param name="p">处理进程</param>
		/// <param name="inputData">报文数据</param>
        public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData)
        {
			try
			{
				string data = inputData.ReadToEnd();
				Debug.WriteLine("接收到信息***{0}",data);
				RequestDTO request = JsonUtil.DeserializeJsonToObject<RequestDTO>(data);

				// 验证令牌
				if (null != request)
				{
					if (validateToken(request))
					{

						if (p.http_url.Equals(url))
						{

                            if (request.Fn == "UpdateMemberByYun")
                            {
                                ResponseDTO response = this.surfLogic.UpdateMemberByYun(request.Data);
                                p.SendResponse(HttpProcessor.HttpStatus.Ok, JsonUtil.SerializeObject(response));
                            }
                            else if (request.Fn == "AddChainMember")
                            {
                                ResponseDTO response = this.surfLogic.AddChainMember(request.Data);
                                p.SendResponse(HttpProcessor.HttpStatus.Ok,JsonUtil.SerializeObject(response));
                            }

							return;
						}

					}

				}
				// 发送错误信息
				p.SendResponse(HttpProcessor.HttpStatus.Err, "");

			
			}
			catch (Exception ex)
			{

				LogHelper.WriteLog("post请求处理出错,错误:\n"+ex.Message);
				p.SendResponse(HttpProcessor.HttpStatus.Err, "");
			}
          
            
        }

		/// <summary>
		/// 验证令牌
		/// </summary>
		/// <param name="dto"></param>
		/// <returns></returns>
        private bool validateToken(RequestDTO dto)
        {
			Debug.WriteLine("HttpServer:获取token******{0}",dto.Token);
			string result = MD5Util.EncryptWithMd5(dto.Fn + dto.Tm+IniUtil.key());
			Debug.WriteLine("HttpServer:计算token******{0}", result);
			if (dto.Token.Equals(result))
			{
				return true;
			}

			return false;
		}



    }
}
