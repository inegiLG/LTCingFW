using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LTCingFW.opc
{

    [XmlRoot("COMMANDS")]
    public class XmlOpcCmdRoot
    {
        [XmlElement(ElementName = "COMMAND" ,IsNullable = false)]
        public List<XmlOpcCmd> OpcCommands { get; set; }
    }

    
    public class XmlOpcCmd
    {
        [XmlAttribute(AttributeName = "NAME")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "MODE")]
        public string Mode { get; set; }
        [XmlElement(ElementName = "OPCITEM")]
        public List<XmlOpcItem> OpcItems { get; set; }
    }

    
    public class XmlOpcItem
    {
        [XmlAttribute(AttributeName = "ID")]
        public String Id { get; set; }
        [XmlAttribute(AttributeName = "TYPE")]
        public String Type { get; set; }
        [XmlAttribute(AttributeName = "ADDRESS")]
        public String Address { get; set; }
        [XmlAttribute(AttributeName = "DESCRIBE")]
        public String Describe { get; set; }
    }
}
