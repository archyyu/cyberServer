using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CashierLibrary.Util;

namespace CashierLibrary.HttpServer
{
    public abstract class HttpServer
    {
        protected int port;
        TcpListener listener;
        bool is_active = true;

        public HttpServer(int port)
        {
            this.port = port;
        }

        public void listen()
        {
            listener = new TcpListener(port);
            listener.Start();
            while (is_active)
            {

                try
                {

                    TcpClient s = listener.AcceptTcpClient();
                    HttpProcessor processor = new HttpProcessor(s, this);
                    //Thread thread = new Thread(new ThreadStart(processor.process));
                    //thread.Start();

                    ThreadPool.QueueUserWorkItem(new WaitCallback(processor.process),null);

                    Thread.Sleep(1);
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("listen error",ex);
                }


            }
        }

        public IDictionary<string,string> parsePostData(string data)
        {
            IDictionary<string, string> results = new Dictionary<string, string>();



            return results;
        }

        public abstract void handleGETRequest(HttpProcessor p);
        public abstract void handlePOSTRequest(HttpProcessor p, StreamReader inputData);

    }
}
