﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace AhpilyServer
{
    public delegate void ReceiveCompeled(ClientPeer client, SocketMsg msg);

    public class ClientPeer
    {

        public ClientPeer()
        {           
             this.ReceiveArgs = new SocketAsyncEventArgs();
             this.ReceiveArgs.UserToken = this;                    
        }

        public Socket ClientSocket { get; set; }
   
        public SocketAsyncEventArgs ReceiveArgs { get; set; }

        private bool isProcess = false; // 是否正在处理

        public ReceiveCompeled rc;

        public List<byte> dataCacheList = new List<byte>();

        /// <summary>
        /// 处理自身数据包
        /// </summary>
        /// <param name="packet"></param>
        public void StartReceive(byte[] packet)
        {
            dataCacheList.AddRange(packet);
            if (!isProcess)
            {
                ProcessReceive();
            }
        }

        /// <summary>
        /// 处理接收的数据
        /// </summary>
        private void ProcessReceive()
        {
            isProcess = true;
           
            byte[] data = EncodeTool.DecodePacket(ref dataCacheList);  // 拆包
            if (data == null)
            {
                isProcess = false;
                return;
            }

            // 把 data 再次转成一个具体的数据类型 , 供我们使用
            SocketMsg msg = EncodeTool.DecodeMsg(data);

            // 回调给上层 
            if (rc!=null)
            {
                rc(this, msg);
            }
            // 尾递归
            ProcessReceive();
        }
    }
}