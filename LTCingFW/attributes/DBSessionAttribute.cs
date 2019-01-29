using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DBSessionAttribute:Attribute
    {
        private string dbAlias;
        public String DbAlias {
            get
            {
                return dbAlias;
            }
            set
            {
                dbAlias = value;
            }
        }

        private bool openTransaction = false;
        public bool OpenTransaction
        {
            get
            {
                return openTransaction;
            }
            set
            {
                openTransaction = value;
            }
        }
        public DBSessionAttribute(string DbAlias) {
            dbAlias = DbAlias;
        }
    }
}
