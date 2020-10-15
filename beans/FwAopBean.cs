using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    public class FwAopBean
    {
        public String Scope { get; set; }
        public String BeforeMethod { get; set; } = "";
        public String AfterMethod { get; set; } = "";


        //构造函数
        public FwAopBean() { }
        public FwAopBean(String scope, String befMethod, String aftMethod)
        {
            Scope = scope;
            BeforeMethod = befMethod;
            AfterMethod = aftMethod;
            setRegxScope();
        }

        /// <summary>
        /// 正则范围
        /// </summary>
        private String regxClassScope = null;
        private String regxMethodScope = null;

        public String RegxClassScope
        {
            get
            {
                return regxClassScope;
            }
        }
        public String RegxMethodScope
        {
            get
            {
                return regxMethodScope;
            }
        }

        private void setRegxScope()
        {
            try
            {
                int dex = Scope.LastIndexOf('.');
                String cls_reg = Scope.Substring(0, dex);
                String mth_reg = Scope.Substring(dex + 1);

                if (cls_reg == "")
                {
                    cls_reg = "~";
                }
                regxClassScope = cls_reg.ToString().Replace(".", "\\.").Replace("*", ".*").Replace("~", ".*");
                regxMethodScope = mth_reg.ToString().Replace("(", "\\(").Replace(")", "\\)").Replace("..", "*").Replace("*", ".*").Replace("~", ".*");
            }
            catch (Exception ex)
            {
                throw new Exception("Scope请符合范式 [包.类].方法名 其中可用~代替任何字符串->"+ex.Message);
            }
        }


    }
}
