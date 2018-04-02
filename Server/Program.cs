using System;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            SocketListener.startListening();
            //Client.StartClient();

        }
    }
}
