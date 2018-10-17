using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace AhpilyServer
{
    public class ClientPeer
    {
        public Socket ClientSocket { get; set; }

        public List<byte> dataCacheList = new List<byte>();

        public SocketAsyncEventArgs ReceiveArgs;

        public byte[] StartReceive(byte[] packet)
        {

        }

    }
}
