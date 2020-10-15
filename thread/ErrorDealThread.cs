using log4net;
using LTCingFW.beans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LTCingFW.thread
{
    public class ErrorDealThread : BaseThread
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ErrorDealThread));
        public override void run(object param)
        {
            logger.Info("开启默认错误处理线程！");

            try
            {
                while (true)
                {
                    Thread.Sleep(LoopRate);

                    foreach (Exception err in LTCingFWSet.ErrList)
                    {
                        logger.Warn("\n" + err.ToString());
                    }

                    LTCingFWSet.ErrList.Clear();

                }
            }
            catch (Exception e)
            {
                logger.Warn("ErrorDealThreadError:" + e.Message + e.StackTrace);
            }
            finally
            {
                utils.FwUtilFunc.CloseThread(Thread.CurrentThread.Name);
            }
        }


    }
}
