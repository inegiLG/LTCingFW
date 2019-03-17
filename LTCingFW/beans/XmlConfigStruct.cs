using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LTCingFW
{
    [XmlRoot("LTCingFW")]
    public class ConfigStructRoot
    {
        [XmlElement(ElementName = "beans", IsNullable = true)]
        public Beans_Branch BeanBranch { get; set; }
        [XmlElement(ElementName = "aspects", IsNullable = true)]
        public Aspects_Branch AspectBranch { get; set; }
        [XmlElement(ElementName = "dbs", IsNullable = true)]
        public DBs_Branch DbBranch { get; set; }
        [XmlElement(ElementName = "cache", IsNullable = true)]
        public Cache_Branch CacheBranch { get; set; }

    }

    public class Beans_Branch
    {
        [XmlElement(ElementName = "bean", IsNullable = true)]
        public List<Bean_Leaf> BeanLeafs { get; set; }
    }

    public class Bean_Leaf
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
    }

    public class Aspects_Branch
    {
        [XmlElement(ElementName = "aspect", IsNullable = true)]
        public List<Aspect_Leaf> AspectLeafs { get; set; }
    }

    public class Aspect_Leaf
    {
        [XmlAttribute(AttributeName = "scope")]
        public string Scope { get; set; }
        [XmlAttribute(AttributeName = "beforemethod")]
        public string BeforeMethod { get; set; }
        [XmlAttribute(AttributeName = "aftermethod")]
        public string AfterMethod { get; set; }
    }

    public class DBs_Branch
    {
        [XmlElement(ElementName = "db", IsNullable = true)]
        public List<DB_Leaf> DbLeafs { get; set; }
    }

    public class DB_Leaf
    {
        [XmlAttribute(AttributeName = "dbAlias")]
        public string DbAlias { get; set; }
        [XmlElement(ElementName = "dbtype", IsNullable = false)]
        public string DbType { get; set; }//oracle or sqlserver or mysql
        [XmlElement(ElementName = "datasource", IsNullable = false)]
        public string DataSource { get; set; }
        [XmlElement(ElementName = "userid", IsNullable = false)]
        public string UserID { get; set; }
        [XmlElement(ElementName = "password", IsNullable = false)]
        public string Password { get; set; }
        [XmlElement(ElementName = "timeout", IsNullable = true)]
        public string ConnectionTimeout { get; set; }
        [XmlElement(ElementName = "pooling", IsNullable = true)]
        public string Pooling { get; set; }
        [XmlElement(ElementName = "maxsize", IsNullable = true)]
        public string MaxPoolSize { get; set; }
        [XmlElement(ElementName = "minsize", IsNullable = true)]
        public string MinPoolSize { get; set; }
        [XmlElement(ElementName = "incrsize", IsNullable = true)]
        public string IncrPoolSize { get; set; }
        [XmlElement(ElementName = "decrsize", IsNullable = true)]
        public string DecrPoolSize { get; set; }
        [XmlElement(ElementName = "initialcatalog", IsNullable = true)]
        public string InitialCatalog { get; set; }
        [XmlElement(ElementName = "loadbalancing", IsNullable = true)]
        public string LoadBalancing { get; set; }
        [XmlElement(ElementName = "proxypassword", IsNullable = true)]
        public string ProxyPassword { get; set; }
        [XmlElement(ElementName = "proxyuserId", IsNullable = true)]
        public string ProxyUserId { get; set; }
        [XmlElement(ElementName = "database", IsNullable = true)]
        public string Database { get; set; }
        [XmlElement(ElementName = "connectionstring", IsNullable = true)]
        public string ConnectionString { get; set; }

        public const String OracleDBType = "oracle";
        public const String SqlServerDBType = "sqlserver";
        public const String MysqlDBType = "mysql";
    }

    public class Cache_Branch
    {
        [XmlElement(ElementName = "switch", IsNullable = false)]
        public string Switch { get; set; }// on or off
        [XmlElement(ElementName = "type", IsNullable = true)]
        public string CacheType { get; set; } //memory or local or net
        [XmlElement(ElementName = "localposition", IsNullable = true)]
        public string LocalPosition { get; set; }
        [XmlElement(ElementName = "netposition", IsNullable = true)]
        public string NetPosition { get; set; }
    }


}
