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
            request.Timeout = 10000;
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
            return HttpPost(url, postDataStr, -1);
        }

        public static string HttpPost(string url, string postDataStr,int waitTime_ms)
        {
            string strReturn;
            //在转换字节时指定编码格式
            byte[] byteData = Encoding.UTF8.GetBytes(postDataStr);

            //配置Http协议头
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteData.Length;
            request.Timeout = 10000;
            if (waitTime_ms > 0)
            {
                request.ReadWriteTimeout = waitTime_ms;//默认5分钟超时
            }
            //发送数据
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(byteData, 0, byteData.Length);
            }

            //接受并解析信息
            using (WebResponse response = request.GetResponse())
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
