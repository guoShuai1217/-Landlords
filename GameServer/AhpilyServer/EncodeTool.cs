using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace AhpilyServer
{
    /// <summary>
    /// 编码工具类
    /// </summary>
    public static class EncodeTool
    {

        #region 粘包拆包问题

        
        /// <summary>
        /// 构造包 (包头 + 包尾)
        /// 包头 是这个包的长度
        /// 包尾 就是这个包
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public static byte[] EncodePacket(byte[] packet)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(packet.Length);
                    bw.Write(packet);

                    byte[] dataArray = new byte[(int)ms.Length];
                    Buffer.BlockCopy(ms.GetBuffer(), 0, dataArray, 0, (int)ms.Length);

                    return dataArray;
                }
            }
        }


        /// <summary>
        /// 解析包 (根据包头的长度去加载这个包)
        /// </summary>
        /// <param name="dataCacheList"></param>
        /// <returns></returns>
        public static byte[] DecodePacket(ref List<byte> dataCacheList)
        {
            if (dataCacheList.Count < 4) // 包头是 int 类型 , 占4个字节
            {
                //throw new Exception("包的长度不足4 , 无法构成一个完整的消息");
                return null;
            }

            using (MemoryStream ms = new MemoryStream(dataCacheList.ToArray()))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    // 123456
                    int length = br.ReadInt32(); // 这个包的长度 12
                    int remainLength = (int)(ms.Length - ms.Position); // 包的剩余长度 3456
                    if (length > remainLength)
                    {
                        //throw new Exception("数据长度不够包头约定的长度");
                        return null;
                    }

                    byte[] data = br.ReadBytes(length); // 返回包头长度的包出去

                    // 更新 缓存区 dataCacheList
                    dataCacheList.Clear();
                    dataCacheList.AddRange(br.ReadBytes(remainLength));

                    return data;
                }
            }

        }

        #endregion


        #region SocketMsg 和 字节数组 的相互转化


        /// <summary>
        /// 把 SocketMsg 类 转换成 字节数组 发送出去
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static byte[] EncodeMsg(SocketMsg msg)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(msg.OpCode);
                    bw.Write(msg.SubCode);

                    if (msg.Value != null)
                    {
                        byte[] valueByte = EncodeObject(msg.Value);
                        bw.Write(valueByte);                    
                    }

                    byte[] dataArray = new byte[ms.Length];
                    Buffer.BlockCopy(ms.GetBuffer(), 0, dataArray, 0, (int)ms.Length);
                    return dataArray;
                }
            }
        }

        /// <summary>
        /// 把 字节数组 再转换成 SocketMsg 类对象
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static SocketMsg DecodeMsg(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    SocketMsg msg = new SocketMsg();
                    msg.OpCode = br.ReadInt32();
                    msg.SubCode = br.ReadInt32();

                    int length = (int)(ms.Length - ms.Position); // 剩余字节长度
                    if (length > 0)  // 还有剩余的字节没有读取 , 证明 Value 不为空 
                    {
                        byte[] dataArray = br.ReadBytes(length);
                        object value = DecodeObject(dataArray);
                        msg.Value = value;
                    }

                    return msg;
                }
            }
        }


        #endregion


        #region  object 和 字节数组 的相互转化 (序列化和反序列化)

        /// <summary>
        /// 序列化 (把 object 转换成 字节数组)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] EncodeObject(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, obj);

                byte[] value = new byte[ms.Length];
                Buffer.BlockCopy(ms.GetBuffer(), 0, value, 0, (int)ms.Length);

                return value;
            }
        } 

        /// <summary>
        /// 反序列化(把 字节数组 转换成 object)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static object DecodeObject(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                BinaryFormatter bf = new BinaryFormatter();
                object value = bf.Deserialize(ms);
                return value;
            }
        }


        #endregion




    }
}
