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
    public class ServerPeer
    {
        private Socket serverSocket;

        private Semaphore sema;  // 限制访问服务器的数量

        private ClientPeerPool clientPool; // 客户端连接池对象

      
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
                    clientPeer.ReceiveArgs.UserToken = clientPeer;
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

            sema.WaitOne(); // 限制访问服务器的客户端数量
           
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
            Socket clientSocket = e.AcceptSocket;
            ClientPeer clientPeer = clientPool.DequeueClient();
            clientPeer.ClientSocket = clientSocket;

            StartReceive(clientPeer);

            e.AcceptSocket = null;
            StartAccept(e);
        }

        #endregion


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
            catch (Exception e )
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
            }
            else if (client.ReceiveArgs.BytesTransferred == 0)
            {
                if (client.ReceiveArgs.SocketError == SocketError.Success) // 客户端主动断开连接
                {
                    
                }
                else // 网络异常 , 被动断开连接
                {

                }
            }
        }


    }
}
