using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CashierLibrary.Util
{
    public static class WakeOnLanHelper
    {

        ///<summary>
        ///启动指定物理地址的计算机，此计算机必须开启网络启动的设置
        ///</summary>
        ///<param name="macString">物理地址 “FF-FF-FF-FF-FF-FF”格式</param>

        public static int WakeUp(string macString)
        {
            try
            {
                if (null != macString && macString.Split('-').Length == 6)
                {
                    string[] macStringArray = macString.Split('-');
                    byte[] macByteArray = new byte[6];
                    for (int i = 0; i < 6; i++)
                    {
                        macByteArray[i] = Convert.ToByte(macStringArray[i], 16);
                    }

                    UdpClient client = new UdpClient();
                    client.Connect(IPAddress.Broadcast, 9090);

                    byte[] packet = new byte[17 * 6];

                    //写入6字节FF
                    for (int i = 0; i < 6; i++)
                        packet[i] = 0xFF;

                    //写入16遍mac地址
                    for (int i = 1; i <= 16; i++)
                        for (int j = 0; j < 6; j++)
                            packet[i * 6 + j] = macByteArray[j];

                    int result = client.Send(packet, packet.Length);

                    return result;
                }

            }
            catch (Exception ex)
            {
                // Logger.Log.Debug("网络唤起指定计算机异常"+ex.ToString());

            }
            return 0;

        }
    }
}
