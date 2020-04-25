using CashierLibrary.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroMQ;

namespace CashierServer.Logic.Surf
{
    public class ZeroMqService : IDisposable
    {

        public static readonly string cashierSubject = "wjcsh";

        public static readonly string clientSubject = "wjclt";

        private ZContext ctx;

        private ZSocket publish;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    //component.Dispose();
                }
                this.publish.Close();
                disposed = true;

            }
        }

        public ZeroMqService()
        {
            try
            {
                this.ctx = new ZContext();
                this.publish = new ZSocket(this.ctx, ZSocketType.PUB);
                this.publish.Linger = TimeSpan.Zero;
                this.publish.Bind("tcp://" + "*" + ":18002");
                this.publish.ReconnectInterval = new TimeSpan(20);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("zero mq init err",ex);
            }
        }

        public void Publish(byte[] msg)
        {
            try
            {
                if (this.publish == null)
                {
                    return;
                }

                using (ZFrame zframe = new ZFrame(msg, 0, msg.Length))
                {
                    this.publish.Send(zframe);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("zero mq publish error",ex);
            }
        }
        
        
       
    }
}
