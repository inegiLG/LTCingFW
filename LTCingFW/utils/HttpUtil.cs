using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW.utils
{
    public class HttpUtil
    {

        public string HttpGet(string Url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();
            return retString;
        }


        public static string HttpPost(string url, string postDataStr)
        {
            string strReturn;
            //在转换字节时指定编码格式
            byte[] byteData = Encoding.UTF8.GetBytes(postDataStr);

            //配置Http协议头
            HttpWebRequest resquest = (HttpWebRequest)WebRequest.Create(url);
            resquest.Method = "POST";
            resquest.ContentType = "application/x-www-form-urlencoded";
            resquest.ContentLength = byteData.Length;

            //发送数据
            using (Stream resquestStream = resquest.GetRequestStream())
            {
                resquestStream.Write(byteData, 0, byteData.Length);
            }

            //接受并解析信息
            using (WebResponse response = resquest.GetResponse())
            {
                //解决乱码：utf-8 + streamreader.readToEnd
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8"));
                strReturn = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();
            }

            return strReturn;
        }



    }
}
