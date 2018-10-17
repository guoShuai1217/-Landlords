using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] EncodePacket(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(data.Length);
                    bw.Write(data);

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
                throw new Exception("包的长度不足4 , 无法构成一个完整的消息");
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
                        throw new Exception("数据长度不够包头约定的长度");
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









    }
}
