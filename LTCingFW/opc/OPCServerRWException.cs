using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW.opc
{
    public class OPCServerRWException : Exception
    {
        public OPCServerRWException() { }
        public OPCServerRWException(string message) : base(message) { }
        public OPCServerRWException(string message, Exception innerException) : base(message, innerException) { }
    }
}
