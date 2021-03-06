﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using CashierLibrary.Util;
using CashierLibrary.Model;

namespace CashierLibrary.HttpServer
{
    public class HttpProcessor
    {
        public TcpClient socket;
        public HttpServer srv;

        private Stream inputStream;
        public StreamWriter outputStream;

        public String http_method;
        public String http_url;
        public String http_protocol_versionstring;
        public Hashtable httpHeaders = new Hashtable();

		public enum HttpStatus
		{
			Ok,
			Err,
		}


        private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB

        public HttpProcessor(TcpClient s, HttpServer srv)
        {
            this.socket = s;
            this.srv = srv;
        }


        private string streamReadLine(Stream inputStream)
        {
            int next_char;
            string data = "";
            while (true)
            {
                next_char = inputStream.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                if (next_char == -1) { Thread.Sleep(1); continue; };
                data += Convert.ToChar(next_char);
            }
            return data;
        }
        public void process(object obj)
        {

            try
            {

                // we can't use a StreamReader for input, because it buffers up extra data on us inside it's
                // "processed" view of the world, and we want the data raw after the headers
                inputStream = new BufferedStream(socket.GetStream());

                // we probably shouldn't be using a streamwriter for all output from handlers either
                outputStream = new StreamWriter(new BufferedStream(socket.GetStream()));
                try
                {
                    parseRequest();
                    readHeaders();
                    if (http_method.Equals("GET"))
                    {
                        handleGETRequest();
                    }
                    else if (http_method.Equals("POST"))
                    {
                        handlePOSTRequest();
                    }
                }
                catch (Exception e)
                {
                    SendResponse(HttpStatus.Err, e.Message);

                }
                outputStream.Flush();
                // bs.Flush(); // flush any remaining output
                
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("process error", ex);
            }
            finally
            {
                inputStream = null; outputStream = null; // bs = null;            
                socket.Close();
            }
        }

        public void parseRequest()
        {
            String request = streamReadLine(inputStream);
            string[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }
            http_method = tokens[0].ToUpper();
            http_url = tokens[1];
            http_protocol_versionstring = tokens[2];

            Console.WriteLine("starting: " + request);
        }

        public void readHeaders()
        {
            Console.WriteLine("readHeaders()");
            String line;
            while ((line = streamReadLine(inputStream)) != null)
            {
                if (line.Equals(""))
                {
                    Console.WriteLine("got headers");
                    return;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("invalid http header line: " + line);
                }
                String name = line.Substring(0, separator).ToUpper();
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++; // strip any spaces
                }

                string value = line.Substring(pos, line.Length - pos);
                Console.WriteLine("header: {0}:{1}", name, value);
                httpHeaders[name] = value;
            }
        }

        public void handleGETRequest()
        {
            srv.handleGETRequest(this);
        }

        private const int BUF_SIZE = 4096;
        public void handlePOSTRequest()
        {
            // this post data processing just reads everything into a memory stream.
            // this is fine for smallish things, but for large stuff we should really
            // hand an input stream to the request processor. However, the input stream 
            // we hand him needs to let him see the "end of the stream" at this content 
            // length, because otherwise he won't know when he's seen it all! 

            Console.WriteLine("get post data start");
            int content_len = 0;
            MemoryStream ms = new MemoryStream();

		

			if (this.httpHeaders.ContainsKey(("Content-Length").ToUpper()))
            {
                content_len = Convert.ToInt32(this.httpHeaders[("Content-Length").ToUpper()]);
                if (content_len > MAX_POST_SIZE)
                {
                    throw new Exception(
                        String.Format("POST Content-Length({0}) too big for this simple server",
                          content_len));
                }
                byte[] buf = new byte[BUF_SIZE];
                int to_read = content_len;
                while (to_read > 0)
                {
                    Console.WriteLine("starting Read, to_read={0}", to_read);

                    int numread = this.inputStream.Read(buf, 0, Math.Min(BUF_SIZE, to_read));
                    Console.WriteLine("read finished, numread={0}", numread);
                    if (numread == 0)
                    {
                        if (to_read == 0)
                        {
                            break;
                        }
                        else
                        {
                            throw new Exception("client disconnected during post");
                        }
                    }
                    to_read -= numread;
                    ms.Write(buf, 0, numread);
                }
                ms.Seek(0, SeekOrigin.Begin);
            }
            Console.WriteLine("get post data end");
            srv.handlePOSTRequest(this, new StreamReader(ms));

        }

        public void Send404()
        {
            this.SendResponse(HttpStatus.Err,"");
        }

		/// <summary>
		/// 发送响应
		/// </summary>
		/// <param name="status">发送状态</param>
		/// <param name="data">数据</param>
		public void SendResponse(HttpStatus status, String data)
		{
            switch (status)
            {
                case HttpStatus.Ok:
                    outputStream.WriteLine("HTTP/1.0 200 OK");
                    break;
                case HttpStatus.Err:
                    outputStream.WriteLine("HTTP/1.0 404 File not found");
                    break;
                default:
                    break;
            }
            outputStream.WriteLine("Connection: close");
            outputStream.WriteLine("");
            if (!string.IsNullOrEmpty(data))
            {
                outputStream.WriteLine(data);
            }
		}

        public void SendReponseWithContentType(WebPage page)
        {
            outputStream.WriteLine("HTTP/1.0 200 OK");
            outputStream.WriteLine("Connection: close");

            if (page.acceptRanges.Length > 0)
            {
                outputStream.WriteLine("Accept-Ranges:" + "bytes");
            }
            
            outputStream.WriteLine("Content-Type:" + page.contentType);
            outputStream.WriteLine("Content-Length:" + page.length);
            outputStream.WriteLine("");
            
            outputStream.WriteLine(page.content);
            
        }

    }
}
