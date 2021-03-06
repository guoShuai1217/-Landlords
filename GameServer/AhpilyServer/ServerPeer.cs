﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AhpilyServer
{
    public class ServerPeer
    {
        private Socket serverSocket;

        private Semaphore sema;  // 限制访问服务器的数量

        private ClientPeerPool clientPool; // 客户端连接池对象

        private IApplication app;  // 应用层

        /// <summary>
        /// 设置应用层
        /// </summary>
        /// <param name="app"></param>
        public void SetApplication(IApplication app)
        {
            this.app = app;
        }
      
        /// <summary>
        /// 开启服务器
        /// </summary>
        /// <param name="port">端口号</param>
        /// <param name="maxCount">最大连接数量</param>
        public void Start(ushort port,ushort maxCount)
        {
            
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sema = new Semaphore(maxCount, maxCount);

                clientPool = new ClientPeerPool(maxCount);
                ClientPeer clientPeer = null;
                for (int i = 0; i < maxCount; i++)
                {
                    clientPeer = new ClientPeer();                   
                    clientPeer.ReceiveArgs.Completed += Receive_Compelet;                  
                    clientPeer.rc = ReceiveCompeleted;
                    clientPeer.sd = Disconnect;
                    clientPool.EnqueueClient(clientPeer);
                }
                
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                serverSocket.Listen(maxCount);

                Console.WriteLine("服务器启动成功"); // 服务器启动成功之后, 就开始监听客户端的接入(StartAccept())
                StartAccept(null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
           
        }

        #region 接收客户端的接入

        /// <summary>
        /// 接收客户端的连接
        /// </summary>
        private void StartAccept(SocketAsyncEventArgs e)
        {
            if (e == null)
            {
                e = new SocketAsyncEventArgs();
                e.Completed += Accept_Completed;
            }

           
            bool result = serverSocket.AcceptAsync(e); // 异步事件是否执行完毕 
                                                       // result == true 正在执行 , 执行完毕后触发事件 e.Completed (也就是触发 Accept_Completed 方法)
            if (!result)                               // result == false 已经执行完毕 , 直接处理 连接请求(ProcessAccept())
            {
                ProcessAccept(e);
            }

        }

      
        // 接受异步事件完成的时候触发
        private void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        /// <summary>
        /// 处理连接请求
        /// </summary>
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            sema.WaitOne(); // 限制访问服务器的客户端数量

            //Socket clientSocket = e.AcceptSocket;
            ClientPeer clientPeer = clientPool.DequeueClient();
            clientPeer.ClientSocket = e.AcceptSocket; // 得到客户端的对象

            StartReceive(clientPeer);

            e.AcceptSocket = null;
            StartAccept(e);
        }

        #endregion


        #region 接收数据

      
        /// <summary>
        /// 开始接收客户端发来的数据
        /// </summary>
        /// <param name="client"></param>
        private void StartReceive(ClientPeer client)
        {
            try
            {
                bool result =  client.ClientSocket.ReceiveAsync(client.ReceiveArgs);
                if (!result)
                {
                    ProcessReceive(client.ReceiveArgs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }

        // 接收客户端发来的数据 , 接收完成的时候 触发的事件
        private void Receive_Compelet(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
        }

        /// <summary>
        /// 处理客户端发送的数据
        /// </summary>
        /// <param name="receiveArgs"></param>
        private void ProcessReceive(SocketAsyncEventArgs receiveArgs)
        {
            ClientPeer client = receiveArgs.UserToken as ClientPeer;

            if (client.ReceiveArgs.SocketError == SocketError.Success && client.ReceiveArgs.BytesTransferred > 0)
            {
                byte[] dataArray = new byte[client.ReceiveArgs.BytesTransferred];
                Buffer.BlockCopy(client.ReceiveArgs.Buffer, 0, dataArray, 0, client.ReceiveArgs.BytesTransferred);

                client.StartReceive(dataArray); // 拆包

                StartReceive(client); // 尾递归
            }
            else if (client.ReceiveArgs.BytesTransferred == 0)
            {
                if (client.ReceiveArgs.SocketError == SocketError.Success) 
                {
                    // 客户端主动断开连接
                    Disconnect(client, "客户端主动断开连接");
                }
                else 
                {
                    // 网络异常 , 被动断开连接
                    Disconnect(client, client.ReceiveArgs.SocketError.ToString());
                }
            }
        }

        #endregion

        /// <summary>
        /// 数据解析完成的处理
        /// </summary>
        /// <param name="client"></param>
        /// <param name="obj"></param>
        private void ReceiveCompeleted(ClientPeer client, SocketMsg msg)
        {
            // 给应用层 , 让其使用
            app.OnReceive(client, msg);
        }

        #region 断开连接

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="client">当前指定的客户端</param>
        /// <param name="reason">断开原因</param>
        public void Disconnect(ClientPeer client, string reason)
        {
            try
            {
                if (client == null)
                {
                    throw new Exception("当前客户端为空 , 无法断开连接");
                }

                // 通知应用层,客户端断开连接了                
                app.OnDisconnect(client);

                client.Disconnect();
                clientPool.EnqueueClient(client); // 回收对象,以便下次使用
                sema.Release();  // 退出信号量 , 返回前一个计数
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        #endregion


    }
}
