using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LTCingFW.utils
{
    /// <summary>
    /// 参考用
    /// </summary>
    public class XmlManager
    {
        private string Xml_FileName;//定义要引入的文件名变量
        XmlDocument xml_doc = new XmlDocument();

        public XmlManager()
        { }

        public XmlManager(string fileName)//类的重载，直接加载XML文件
        {
            Xml_FileName = fileName;
        }

        public bool FileIsExist() {
            if (System.IO.File.Exists(Xml_FileName))//判断文件是否存在
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string FileName//文件名存取器
        {
            get
            { return Xml_FileName; }
            set
            { Xml_FileName = value; }
        }
        /// <summary>
        /// 更改xml文件地址,并载入
        /// </summary>
        /// <param name="fileName">xml文件地址</param>
        public void LoadFrom(string fileName)//加载XML文件
        {
            Xml_FileName = fileName;
            xml_doc.Load(fileName);
        }

        public void Load() {
            xml_doc.Load(Xml_FileName);
        }

        public void createBlankXmlFile()//创建一个新的XML文件
        {
            XmlDocument xml_doc = new XmlDocument();
            XmlDeclaration xmldecl = xml_doc.CreateXmlDeclaration("1.0", "utf-8", null);
            xml_doc.AppendChild(xmldecl);
            
            xml_doc.Save(Xml_FileName);
        }

        public object[] readAll_xmlnode()//读取所有节点的名称。
        {
            List<object> result = new List<object>();//定义一个泛型,用于存放节点名
            XmlReader rdr = XmlReader.Create(FileName);
            while (rdr.Read())
            {
                if (rdr.NodeType == XmlNodeType.Element)
                {
                    result.Add(rdr.Name);
                }
            }
            return result.ToArray();
        }

        public object[] readXMlFile()//读取文件的全部内容。lu不用
        {
            List<object> xml_Read = new List<object>();//定义一个泛型,用于存放读到的行
            System.IO.StreamReader reader = new System.IO.StreamReader(FileName, System.Text.ASCIIEncoding.Default);
            try
            {
                while (reader.Peek() >= 0)
                {
                    xml_Read.Add(reader.ReadLine());//逐行读取
                }
            }
            finally
            {
                reader.Close();
            }
            return xml_Read.ToArray();
        }

        public string to_element(string element_name)//转换为元素路径值
        { return "//" + element_name; }

        public string to_attribute(string element_name, string attribute_nane, string attribute_value)//转换为元素属性路径值
        { return "//" + element_name + "[@" + attribute_nane + "='" + attribute_value + "']"; }

        public string xml_path(string[] path_array)//将数组中的内容组合成XML元素路径值
        {
            string xml_path = string.Empty;
            for (int i = 0; i < path_array.Length; i++)
            { xml_path += path_array[i].ToString(); }

            return xml_path;
        }

        public bool hasNode(string xml_path) {
            XmlNode xml_node = getNode(xml_path);
            if (xml_node == null) {
                return false;
            }
            return true;
        }

        public XmlNodeList getNodechilds(string xml_path)//获取指定元素的所有子元素//同时也可作为获取指定元素的指定子元素的值用
        {
            XmlNodeList nodeList = xml_doc.SelectSingleNode(xml_path).ChildNodes;
            return nodeList;
        }

        public XmlNode getNode(string xml_path)//获取指定元素的指定子元素的值用
        {
            XmlNode xml_node = xml_doc.SelectSingleNode(xml_path);
            return xml_node;
        }
        /// <summary>
        /// //获取指定元素的指定子元素的InnerText
        /// </summary>
        /// <param name="fatherNode"></param>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public static string getNodeInnerText(XmlNode fatherNode,string nodeName)
        {
            XmlNode xml_node = fatherNode.SelectSingleNode(nodeName);
            if (xml_node == null)
            {
                return null;
            }
            else {
                return xml_node.InnerText.Trim();
            }
            
        }
        public XmlAttributeCollection getAttributes(string xml_path)//读取指定元素属性的集合
        {
            XmlNode xml_node = xml_doc.SelectSingleNode(xml_path);
            return xml_node.Attributes;
        }

        public object getAttribute(string xml_path, string xml_attributeName)//获取指定元素的指定属性值
        {
            XmlNode xml_node = xml_doc.SelectSingleNode(xml_path);
            return xml_node.Attributes[xml_attributeName].Value;
        }

        public object getInnerText(XmlNode xml_node) {
            return xml_node.InnerText;
        }
        public String getInnerText(String xml_path)//获取指定元素的innertext值
        {
            XmlNode xml_node = xml_doc.SelectSingleNode(xml_path);
            if (null == xml_node) {
                return null;
            }
            return xml_node.InnerText;
        }

        public XmlNode addRootNode(String nodeName) {
            XmlElement newroot = xml_doc.CreateElement(nodeName);
            newroot.InnerText = "";
            XmlNode node = xml_doc.AppendChild(newroot);
            xml_doc.Save(FileName);
            return node;
        }

        public XmlNode addChildNode(XmlNode FatherXmlNode, String ChildNode_name)//向某元素节点添加子元素
        {
            XmlElement FatherElement = (XmlElement)FatherXmlNode;
            XmlElement newTitle = xml_doc.CreateElement(ChildNode_name);
            XmlNode node = FatherElement.AppendChild(newTitle);
            xml_doc.Save(FileName);
            return node;
        }

        public void addInnertext(XmlNode theXmlNode, String innertext)//向某元素节点添加Innertext
        {
            XmlElement ChildElement = (XmlElement)theXmlNode;
            ChildElement.InnerText = innertext;
            xml_doc.Save(FileName);
        }

        public void addAttributes(XmlNode theXmlNode, String Attribute_name, String Attribute_value)//向某元素节点添加或修改属性
        {
            XmlElement ChildElement = (XmlElement)theXmlNode;
            ChildElement.SetAttribute(Attribute_name, Attribute_value);
            xml_doc.Save(FileName);
        }

        public void modifyAttributes(XmlNode theXmlNode, String Attribute_name, String Attribute_value)//向某元素节点修改属性
        {
            XmlElement ChildElement = (XmlElement)theXmlNode;
            ChildElement.Attributes[Attribute_name].Value = Attribute_value;//.SetAttribute(Attribute_name, Attribute_value);
            xml_doc.Save(FileName);
        }

        public void modifyInnertext(XmlNode theXmlNode, String newInnertext)//修改某元素节点的Innertext值
        {
            XmlElement ChildElement = (XmlElement)theXmlNode;
            ChildElement.InnerText = newInnertext;
            xml_doc.Save(FileName);
        }

        public void modifyNodeName(XmlNode oldChildNode, string newName)//修改某元素节点的元素名称
        {
            XmlNode newNode = xml_doc.CreateNode(oldChildNode.NodeType, newName, oldChildNode.NamespaceURI);//创建新的结点元素

            foreach (XmlAttribute attri in oldChildNode.Attributes)//复制所有属性到新结点
            {
                newNode.Attributes.Append(attri);
            }

            foreach (XmlNode child_Node in oldChildNode.ChildNodes)//复制所有子结点到新结点
            {
                newNode.AppendChild(child_Node);
            }

            XmlNode parent = oldChildNode.ParentNode;
            parent.ReplaceChild(newNode, oldChildNode);//用新结点替换子结点
            xml_doc.Save(FileName);
        }

        public void delelteMiddleNodeSaveChilds(XmlNode theXmlNode)//删除节点但保留其子节点
        {
            XmlNode node = theXmlNode;//获得当前结点
            XmlNode parent = node.ParentNode;//获得结点的父结点
            XmlNode nodetemp = node.Clone();//创建此节点的一个副本
            parent.RemoveChild(node);
            parent.InnerXml = nodetemp.InnerXml;
            xml_doc.Save(FileName);
        }

        public void delelteNodeAndChilds(XmlNode theXmlNode)//删除节点的全部内容
        {
            XmlNode node = theXmlNode;
            XmlNode fathernode = node.ParentNode;
            fathernode.RemoveChild(node);
            xml_doc.Save(FileName);
        }

        public void deleteChildsAndAttributes(XmlNode theXmlNode)//删除指定元素的所有内容[属性/子结点等]但保留此元素
        {
            theXmlNode.RemoveAll();
            xml_doc.Save(FileName);
        }

        public void modifyNodeAttributeName(XmlNode node, string attributeOldName, string attributeNewName)//修改某元素节点属性名
        {
            XmlNode newNode = xml_doc.CreateNode(XmlNodeType.Element, node.Name, node.NamespaceURI);
            XmlAttributeCollection collection = node.Attributes;//注意使用集合,这里避免了集合中元素数量的变化
            foreach (XmlAttribute a in collection)
            {
                if (a.Name == attributeOldName)
                {
                    XmlAttribute newAttr = xml_doc.CreateAttribute(attributeNewName);
                    newAttr.Value = a.Value;
                    newNode.Attributes.Append(newAttr);
                }
                else
                {
                    XmlAttribute newAttr = xml_doc.CreateAttribute(a.Name);
                    newAttr.Value = a.Value;
                    newNode.Attributes.Append(newAttr);
                }
            }
            XmlNodeList nodes = node.ChildNodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                newNode.AppendChild(nodes[0]);  //注意，当我们Append一个XMLNode时，总是取第一个Node。
            }
            XmlNode parent = node.ParentNode;
            parent.ReplaceChild(newNode, node);//在父节点层替换子节点
            xml_doc.Save(FileName);
        }

        public void delAttribue(XmlNode node, string attributeName)//删除指定元素节点的指定属性
        {
            XmlNode newNode = xml_doc.CreateNode(XmlNodeType.Element, node.Name, node.NamespaceURI);
            XmlAttributeCollection collection = node.Attributes;//注意使用集合,这里避免了集合中元素数量的变化
            foreach (XmlAttribute a in collection)
            {
                if (a.Name == attributeName)
                { }
                else
                {
                    XmlAttribute newAttr = xml_doc.CreateAttribute(a.Name);
                    newAttr.Value = a.Value;
                    newNode.Attributes.Append(newAttr);
                }
            }
            XmlNodeList nodes = node.ChildNodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                newNode.AppendChild(nodes[0]);  //注意，当我们Append一个XMLNode时，总是取第一个Node。
            }
            XmlNode parent = node.ParentNode;
            parent.ReplaceChild(newNode, node);//在父节点层替换子节点
            xml_doc.Save(FileName);
        }
    }
}
