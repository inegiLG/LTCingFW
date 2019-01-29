
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DaoAttribute : BaseInstanceAttribute
    {
        private bool _isImpl = true;
        public bool IsImpl
        {
            get { return _isImpl; }
            set
            {
                _isImpl = value;
            }
        }
    }
}
