using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    public class FWAppContainer
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(FWAppContainer));

        private static readonly Dictionary<string, object> _appContainer = new Dictionary<string, object>();

        public static Dictionary<string, object> Container
        {
            get { return _appContainer; }
        }

        public static void setProperty(string x, object y)
        {
            lock (_appContainer)
            {
                if (_appContainer.ContainsKey(x))
                {
                    _appContainer[x] = y;
                }
                else
                {
                    _appContainer.Add(x, y);
                }
            }
        }

        public static object getProperty(string x)
        {
            if (_appContainer.ContainsKey(x)) {
                return _appContainer[x];
            }
            return null;
        }

        public static void removeProperty(string x)
        {
            lock (_appContainer)
            {
                if (_appContainer.ContainsKey(x))
                {
                    _appContainer.Remove(x);
                }
            }
        }

        public static bool Contains(string x)
        {
            return _appContainer.ContainsKey(x);
        }
    }
}
