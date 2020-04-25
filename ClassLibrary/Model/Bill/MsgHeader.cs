using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CashierLibrary.Model.Bill
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct MsgHeader
    {
        
        public UInt32 Length { get; set; }
        public UInt32 Flag { get; set; }

        public UInt16 MainCommand { get; set; }         // 主命令
        public UInt16 SubCommand { get; set; }          // 辅命令
        public UInt32 BnCommand { get; set; }           // 业务自定义命令

        public UInt32 ControlFlag { get; set; }         // 控制标记
        public UInt32 SendIp { get; set; }              // 发送消息的机器ip					
        public UInt32 PayloadSize { get; set; }         // json 负载的大小		
        public UInt16 HeaderChksum { get; set; }        // 头部数据校验
        public UInt16 PayloadChksum { get; set; }       // 负载数据校验

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public char[] ExtendFlag;          // 扩展信息字段			- depend by subcommand or BnCommand（plugin）
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public char[] ExtendFlag2;         // 扩展信息字段			- depend by subcommand or BnCommand（plugin）

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public char[] ChannelList;         // 接收频道列表         - ip或者专属频道

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public char[] PluginListFileter;   // 插件列表过滤			- "|" 分隔

        public byte[] toBytes()
        {
            int size = 544;
            IntPtr buffer = Marshal.AllocHGlobal(size);  //开辟内存空间
            try
            {
                Marshal.StructureToPtr(this, buffer, false);   //填充内存空间
                byte[] bytes = new byte[size];
                Marshal.Copy(buffer, bytes, 0, size);   //填充数组
                return bytes;
            }
            catch (Exception )
            {
                return null;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);   //释放内存
            }

        }

    }


}
