
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW.opc
{
    public delegate void AcknowledgeHandler(OPCCommand command);

    public delegate void DataChangeAcknowledgeHandler(OPCCommand command,Array ClientHandles);
}
