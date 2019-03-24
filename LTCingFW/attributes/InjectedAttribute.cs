using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class InjectedAttribute : BaseInstanceAttribute
    {
        public InjectedAttribute(string name)
        {
            Name = name;
        }
        public InjectedAttribute() { }
    }
}
