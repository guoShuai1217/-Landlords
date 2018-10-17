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
        private Socket clientSocket;

        public void SetSocket(Socket socket)
        {
            this.clientSocket = socket;
        }

    }
}
