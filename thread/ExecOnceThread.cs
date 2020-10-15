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
    public class ExecOnceThread : BaseThread
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ExecOnceThread));
        public override void run(object param)
        {
            ThreadParam tps = param as ThreadParam;

            try
            {
                object result = utils.FwUtilFunc.ExecControllerMethod(tps.ControllerName, tps.MethodName, tps.MethodParams);
                if (null != tps.CallBack)
                {
                    tps.CallBack(result);
                }
            }
            catch (Exception e)
            {
                logger.Warn("ExecOnceThreadError:" + e.Message + e.StackTrace);
            }
            finally
            {
                utils.FwUtilFunc.CloseThread(Thread.CurrentThread.Name);
            }
        }


    }
}
