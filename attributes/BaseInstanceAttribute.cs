using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    [AttributeUsage(AttributeTargets.All)]
    public class BaseInstanceAttribute : Attribute
    {
        protected String _name = String.Empty;
        public String Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }
    }
}
