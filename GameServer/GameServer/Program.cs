﻿using AhpilyServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerPeer serverSocket = new ServerPeer();
            serverSocket.Start(6666, 10);

            Console.ReadKey();
        }
    }
}
