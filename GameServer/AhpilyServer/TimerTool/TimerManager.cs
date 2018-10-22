using AhpilyServer.Concurrent;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace AhpilyServer.TimerTool
{
    /// <summary>
    /// 定时任务(计时器) 管理类
    /// </summary>
    public class TimerManager
    {
        private static TimerManager instance = null;

        public static TimerManager Instance
        {
            get
            {
                lock (instance)
                {
                    if (instance == null)
                    {
                        instance = new TimerManager();
                    }
                    return instance;
                }
            }
        }

        /// <summary>
        /// 实现定时器的主要功能类
        /// </summary>
        private Timer timer;

        /// <summary>
        /// 线程安全的 Id
        /// </summary>
        private ConcurrentInt Id = new ConcurrentInt(-1);

        public TimerManager()
        {
            timer = new Timer(10); // 10ms触发一次
            timer.Elapsed += Timer_Elapsed;
        }

        /// <summary>
        /// 任务Id 和 任务模型 的映射
        /// </summary>
        private ConcurrentDictionary<int, TimerModel> idDic = new ConcurrentDictionary<int, TimerModel>();

        private List<int> idList = new List<int>();

        /// <summary>
        /// 达到时间间隔的时候,触发的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (idList)
            {
                TimerModel model = null;

                foreach (int item in idList)
                {
                    idDic.TryRemove(item, out model);
                }

                idList.Clear();
            }
          
            foreach (TimerModel item in idDic.Values)
            {
                if (item.Time <= DateTime.Now.Ticks)
                    item.Run();
            }
        }

        /// <summary>
        /// 添加定时任务 (指定触发的时间 例如:2018年10月22日17:35:35)
        /// </summary>
        /// <param name="dateTime">当前时间</param>
        /// <param name="timeDele"></param>
        public void AddTimeEvent(DateTime dateTime , TimeDelegate timeDele)
        {
            long delayTime = dateTime.Ticks - DateTime.Now.Ticks;
            if (delayTime <= 0)
                return;
            AddTimeEvent(delayTime, timeDele);
        }

        /// <summary>
        /// 添加定时任务 (指定延时的时间)
        /// </summary>
        /// <param name="delayTime">延时时间(ms)</param>
        /// <param name="timeDele"></param>
        public void AddTimeEvent(long delayTime , TimeDelegate timeDele)
        {
            //                             先赋值,再++ ,  当前时间     +   延时时间
            TimerModel model = new TimerModel(Id.Add_Get(), DateTime.Now.Ticks + delayTime, timeDele); //
            idDic.TryAdd(model.Id, model);
        }
    }
}
