using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AhpilyServer
{
    /// <summary>
    /// 客户端连接池 (用于重用客户端连接对象)
    /// </summary>
    public class ClientPeerPool
    {
        private Queue<ClientPeer> clientQue;

        public ClientPeerPool(int capacity)
        {
            clientQue = new Queue<ClientPeer>(capacity);
        }

        /// <summary>
        /// 进队列
        /// </summary>
        /// <param name="clientPeer"></param>
        public void EnqueueClient(ClientPeer clientPeer)
        {
            clientQue.Enqueue(clientPeer);
        }

        /// <summary>
        /// 出队列
        /// </summary>
        public ClientPeer DequeueClient()
        {
           return clientQue.Dequeue();
        }
    }
}
