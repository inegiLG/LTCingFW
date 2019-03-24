using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    public class OrmDataType
    {


        public enum SqlDbType
        {
            //
            // 摘要:
            //     System.Int64。 64 位带符号整数。
            BigInt = 0,
            //
            // 摘要:
            //     System.Array类型System.Byte。 固定长度流，范围在 1 到 8000 个字节之间的二进制数据。
            Binary = 1,
            //
            // 摘要:
            //     System.Boolean。 无符号的数字值，可为 0，1，或 null。
            Bit = 2,
            //
            // 摘要:
            //     System.String。 范围在 1 到 8000 个字符之间的非 Unicode 字符固定长度流。
            Char = 3,
            //
            // 摘要:
            //     System.DateTime。 日期和时间数据，值范围从 1753 年 1 月 1 日至 12 月 31 日，精确到 3.33 毫秒到 9999。
            DateTime = 4,
            //
            // 摘要:
            //     System.Decimal。 固定的精度和小数位数之间的数值范围为-10 38 -1 和 10 38 -1。
            Decimal = 5,
            //
            // 摘要:
            //     System.Double。 浮点数，范围在-1.79 e + 308 到 1.79 e + 308 之间。
            Float = 6,
            //
            // 摘要:
            //     System.Array类型System.Byte。 范围从 0 到 2 的二进制数据的长度可变的流 31 -1 （或者 2147483647） 字节。
            Image = 7,
            //
            // 摘要:
            //     System.Int32。 32 位带符号整数。
            Int = 8,
            //
            // 摘要:
            //     System.Decimal。 货币值，范围从-2 63 （即-9223372036854775808） 到 2 63 -1 （或 9223372036854775807），精确到货币单位的万分之一。
            Money = 9,
            //
            // 摘要:
            //     System.String。 范围在 1 到 4000 个字符之间的 Unicode 字符的固定长度流。
            NChar = 10,
            //
            // 摘要:
            //     System.String。 最大长度为 2 Unicode 数据的长度可变的流 30 -1 （或者 1073741823） 个字符。
            NText = 11,
            //
            // 摘要:
            //     System.String。 范围在 1 到 4000 个字符之间的 Unicode 字符长度可变的流。 如果字符串大于 4000 个字符，隐式转换将失败。
            //     使用字符串长度超过 4000 个字符时，请显式设置对象。 使用 System.Data.SqlDbType.NVarChar 数据库列时 nvarchar(max)。
            NVarChar = 12,
            //
            // 摘要:
            //     System.Single。 浮点数，范围在-3.40 e + 38 到 3.40 e + 38 之间。
            Real = 13,
            //
            // 摘要:
            //     System.Guid。 全局唯一标识符 （或 GUID） 中。
            UniqueIdentifier = 14,
            //
            // 摘要:
            //     System.DateTime。 数值范围从 1900 年 1 月 1 日到 2079 年 6 月 6 日精度为一分钟的日期和时间数据。
            SmallDateTime = 15,
            //
            // 摘要:
            //     System.Int16。 16 位带符号整数。
            SmallInt = 16,
            //
            // 摘要:
            //     System.Decimal。 一个范围从-214，748.3648 到 +214,748.3647，精确到货币单位的万分之一的货币值。
            SmallMoney = 17,
            //
            // 摘要:
            //     System.String。 最大长度为 2 的非 Unicode 数据的变量长度流 31 -1 （或者 2147483647） 个字符。
            Text = 18,
            //
            // 摘要:
            //     System.Array类型System.Byte。 自动生成二进制数字，保证在数据库中是唯一。 timestamp 通常用作为表行加版本戳的机制。 存储大小为
            //     8 个字节。
            Timestamp = 19,
            //
            // 摘要:
            //     System.Byte。 8 位无符号整数。
            TinyInt = 20,
            //
            // 摘要:
            //     System.Array类型System.Byte。 范围在 1 到 8000 个字节之间的二进制数据长度可变的流。 如果字节数组大于 8000 个字节，隐式转换将失败。
            //     在使用字节数组大于 8000 个字节时，请显式设置对象。
            VarBinary = 21,
            //
            // 摘要:
            //     System.String。 范围在 1 到 8000 个字符之间的非 Unicode 字符长度可变的流。 使用 System.Data.SqlDbType.VarChar
            //     数据库列时 varchar(max)。
            VarChar = 22,
            //
            // 摘要:
            //     System.Object。 可以包含数值的特殊数据类型，字符串、 二进制文件中，或日期数据，以及 SQL Server 值的空和 Null，这将假定如果没有其他声明类型。
            Variant = 23,
            //
            // 摘要:
            //     XML 值。 获取将 XML 作为字符串使用 System.Data.SqlClient.SqlDataReader.GetValue(System.Int32)
            //     方法或 System.Data.SqlTypes.SqlXml.Value 属性，或指定为 System.Xml.XmlReader 通过调用 System.Data.SqlTypes.SqlXml.CreateReader
            //     方法。
            Xml = 25,
            //
            // 摘要:
            //     一个 SQL Server 用户定义类型 (UDT)。
            Udt = 29,
            //
            // 摘要:
            //     用于指定包含在表值参数中的结构化的数据的特殊数据类型。
            Structured = 30,
            //
            // 摘要:
            //     日期数据，从 1 月的值范围为 1，1 AD 到公元 9999 年 12 月 31 日。
            Date = 31,
            //
            // 摘要:
            //     基于 24 小时制时间数据。 时间值范围是 00:00:00 到 23:59:59.9999999 100 纳秒精度。 对应于 SQL Server time
            //     值。
            Time = 32,
            //
            // 摘要:
            //     日期和时间数据。 日期值范围是从 1 月 1，1 AD 到公元 9999 年 12 月 31 日。 时间值范围是 00:00:00 到 23:59:59.9999999
            //     100 纳秒精度。
            DateTime2 = 33,
            //
            // 摘要:
            //     时区的日期和时间数据。 日期值范围是从 1 月 1，1 AD 到公元 9999 年 12 月 31 日。 时间值范围是 00:00:00 到 23:59:59.9999999
            //     100 纳秒精度。 时区值范围是-14:00 至 + 14:00。
            DateTimeOffset = 34
        }
        public enum OracleDbType
        {
            BFile = 101,
            Blob = 102,
            Byte = 103,
            Char = 104,
            Clob = 105,
            Date = 106,
            Decimal = 107,
            Double = 108,
            Long = 109,
            LongRaw = 110,
            Int16 = 111,
            Int32 = 112,
            Int64 = 113,
            IntervalDS = 114,
            IntervalYM = 115,
            NClob = 116,
            NChar = 117,
            NVarchar2 = 119,
            Raw = 120,
            RefCursor = 121,
            Single = 122,
            TimeStamp = 123,
            TimeStampLTZ = 124,
            TimeStampTZ = 125,
            Varchar2 = 126,
            XmlType = 127,
            BinaryDouble = 132,
            BinaryFloat = 133,
            Boolean = 134
        }
        public enum MySqlDbType
        {
            //数字类
            BIT = 201,
            BOOL = 202,
            TINYINT = 203,
            SMALLINT = 204,
            MEDIUMINT = 205,
            INT = 206,
            BIGINT = 207,
            //浮点类型
            FLOAT = 208,
            DOUBLE =209,
            DECIMAL =210,
            //字符串类型
            CHAR = 211,
            VARCHAR = 212,
            TINYTEXT =213,
            TEXT = 214,
            MEDIUMTEXT = 215,
            LONGTEXT = 216,
            TINYBLOB = 217,
            BLOB = 218,
            MEDIUMBLOB =219,
            LONGBLOB = 220,
            //日期类
            DATE = 221,
            DATETIME = 222,
            TIMESTAMP = 223,
            TIME = 224,
            YEAR = 225,
            //其他类型
            BINARY = 226,
            VARBINARY = 227,
            ENUM = 228,
            SET = 229,
            GEOMETRY = 230,
            POINT = 231,
            MULTIPOINT =232,
            LINESTRING = 233,
            MULTILINESTRING = 234,
            POLYGON = 235,
            GEOMETRYCOLLECTION = 236
        }

        public enum CommonType
        {
            //字符串
            STRING = 1001,
            //整数
            INT = 1002,
            //小数
            DECIMAL = 1003,
            //布尔
            BOOL = 1004,
            //二进制
            BINARY = 1005,
            //日期
            DATE = 1006
        }

    }

   
}
