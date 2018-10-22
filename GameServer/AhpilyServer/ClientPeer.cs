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
    public delegate void ReceiveCompeled(ClientPeer client, SocketMsg msg); // 接收数据完成时 , 回调给上层的委托

    public delegate void SendDisconnect(ClientPeer client, string reason); // 发送数据时 断开连接了, 回调给上层的委托

    public class ClientPeer
    {

        public ClientPeer()
        {
            this.ReceiveArgs = new SocketAsyncEventArgs();
            this.ReceiveArgs.UserToken = this;
            this.SendArgs = new SocketAsyncEventArgs();
            this.SendArgs.Completed += SendArgs_Completed;
        }

      
        public Socket ClientSocket { get; set; }
   
        public SocketAsyncEventArgs ReceiveArgs { get; set; }

        public ReceiveCompeled rc;




        #region 接收数据


        public List<byte> dataCacheList = new List<byte>();

        private bool isReceiveProcess = false; // 是否正在处理

        /// <summary>
        /// 处理自身数据包
        /// </summary>
        /// <param name="packet"></param>
        public void StartReceive(byte[] packet)
        {
            dataCacheList.AddRange(packet);
            if (!isReceiveProcess)
            {
                ProcessReceive();
            }
        }

        /// <summary>
        /// 处理接收的数据
        /// </summary>
        private void ProcessReceive()
        {
            isReceiveProcess = true;
           
            byte[] data = EncodeTool.DecodePacket(ref dataCacheList);  // 拆包
            if (data == null)
            {
                isReceiveProcess = false;
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
        #endregion


        #region 断开连接

       

        /// <summary>
        /// 断开数据
        /// </summary>
        public void Disconnect()
        {
            // 接收数据清空
            dataCacheList.Clear();
            isReceiveProcess = false;

            // 发送数据清空
            sendQue.Clear();
            isSendProcess = false;
          
            // Socket对象清空
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
            ClientSocket = null;
        }
        #endregion


        #region 发送数据


        private Queue<byte[]> sendQue = new Queue<byte[]>();

        private bool isSendProcess = false;

        private SocketAsyncEventArgs SendArgs;

        public SendDisconnect sd;

        public void Send(int opCode,int subCode,object value)
        {
            SocketMsg msg = new SocketMsg(opCode, subCode, value);
            byte[] data = EncodeTool.EncodeMsg(msg); // 把需要发送的 SocketMsg 消息 转换成 字节数组
            byte[] packet = EncodeTool.EncodePacket(data); // 再把字节数组 封装成 包头 + 包尾

            sendQue.Enqueue(packet);
            if (!isSendProcess)
                Send();
        }

        /// <summary>
        /// 处理发送的数据
        /// </summary>
        private void Send()
        {
            isSendProcess = true;

            if (sendQue.Count == 0)
            {
                isSendProcess = false;
                return;
            }

            byte[] packet = sendQue.Dequeue(); // 取出一条数据
            SendArgs.SetBuffer(packet, 0, packet.Length); // 放到发送缓冲区
            bool result = ClientSocket.SendAsync(SendArgs); // 发送出去
            if (!result)
            {
                ProcessSend();
            }
        }

        private void SendArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessSend();
        }

        /// <summary>
        /// 发送完成时调用
        /// </summary>
        private void ProcessSend()
        {
            // 先看看有没有发送出错
            if (SendArgs.SocketError != SocketError.Success)
            {
                // 发送出错了 (客户端断开连接了)
                if (sd!=null)
                {
                    sd(this, SendArgs.SocketError.ToString());
                }
            }
            else
            {
                Send();
            }
        }

      


        #endregion
    }
}
