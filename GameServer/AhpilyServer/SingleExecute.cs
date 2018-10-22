using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AhpilyServer
{

    public delegate void ExecuteDelegate();

    /// <summary>
    /// 单线程池
    /// </summary>
    public class SingleExecute
    {

        /// <summary>
        /// 单线程逻辑处理
        /// </summary>
        /// <param name="executeDele"></param>
        public void Execute(ExecuteDelegate executeDele)
        {
            lock (this)
            {
                executeDele();
            }          
        }




        #region 用 Mutex 类处理


        //Mutex mu = new Mutex();

        ///// <summary>
        ///// 单线程逻辑处理
        ///// </summary>
        ///// <param name="executeDele"></param>
        //public void Executing(ExecuteDelegate executeDele)
        //{
        //    lock (this)
        //    {
        //        mu.WaitOne();
        //        executeDele();
        //        mu.ReleaseMutex();
        //    }
        //}
        #endregion


    }
}
