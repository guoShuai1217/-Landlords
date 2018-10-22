using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AhpilyServer
{
    /// <summary>
    /// 网络消息 
    /// 发送消息的时候 , 都要封装成这个类来发送
    /// </summary>
    public class SocketMsg
    {
        public int OpCode { get; set; }  // 操作码

        public int SubCode { get; set; } // 子操作

        public object Value { get; set; } // 参数


        public SocketMsg()
        {

        }


        public SocketMsg(int opCode, int subCode,object value)
        {
            this.OpCode = opCode;
            this.SubCode = subCode;
            this.Value = value;
        }
    }
}
