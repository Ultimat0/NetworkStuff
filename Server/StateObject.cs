using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class StateObject
    {
        public Socket workSocket = null;

        public const int bufferSize = 1024;

        public byte[] buffer = new byte[bufferSize];

        public StringBuilder message = new StringBuilder();

    }
}
