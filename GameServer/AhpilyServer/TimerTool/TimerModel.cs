using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AhpilyServer.TimerTool
{
    public delegate void TimeDelegate();

    /// <summary>
    /// 定时器任务的数据模型
    /// </summary>
    public class TimerModel
    {

        public int Id;

        /// <summary>
        /// 任务执行的时间
        /// </summary>
        public long Time;

        private TimeDelegate timeDele;

        public TimerModel(int id, long time, TimeDelegate timeDele)
        {
            this.Id = id;
            this.Time = time;
            this.timeDele = timeDele;
        }


        /// <summary>
        /// 触发任务的方法
        /// </summary>
        public void Run()
        {
            timeDele();
        }


    }
}
