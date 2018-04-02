using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    class SocketListener
    {
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public static void startListening ()
        {
            byte[] bytes = new byte[1024];

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            Console.WriteLine(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 8087);

            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    allDone.Reset();

                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    allDone.WaitOne();
                }
            }
            catch (Exception e) 
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        public static void AcceptCallback (IAsyncResult result) 
        {
            allDone.Set();

            Socket listener = (Socket)result.AsyncState;
            Socket handler = listener.EndAccept(result);

            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult result)
        {
            String content = String.Empty;

            StateObject state = (StateObject)result.AsyncState;
            Socket handler = state.workSocket;

            int bytesRead = handler.EndReceive(result);

            if (bytesRead > 0)
            {
                state.message.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                content = state.message.ToString();

                if (/*content.IndexOf("<EOF>") > -1*/content == "SEND")
                {
                    //  Console.WriteLine("Read {0} bytes from socket. \n Data : {1}", content.Length, content);

                    Console.WriteLine("Send file");

                    Send(handler, content);
                }
                else if (content == "RECV")
                {
                    Console.WriteLine("Receive file");

                    Send(handler, content);


                }
                else
                {
                    handler.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
            }
        }

        private static void Send (Socket handler, String data) 
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult result)
        {
            try
            {
                Socket handler = (Socket)result.AsyncState;

                int bytesSent = handler.EndSend(result);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
