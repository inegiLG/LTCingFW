using log4net;
using LTCingFW;
using LTCingFW.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LTCingFW
{
    public class ValidationFunc
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ValidationFunc));

        /// <summary>
        /// 针对实例的所有string类型属性进行验证
        /// </summary>
        /// <param name="model">需要验证的实例</param>
        /// <returns></returns>
        public static List<ValidResult> ValidAll(object model) {
            List<ValidResult> retList = new List<ValidResult>();
            try
            {
                PropertyInfo[] infos = model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (PropertyInfo info in infos)
                {
                    string memberName = info.Name;
                    if (info.PropertyType != typeof(string)) {
                        continue;
                    }
                    string value = (string)info.GetValue(model);
                    retList.Add(Valid(model, memberName, value));
                }
            }
            catch (Exception e)
            {
                logger.Warn(e.Message);
                return null;
            }
            
            return retList;
        }

        /// <summary>
        /// 针对实例的某个属性验证
        /// </summary>
        /// <param name="model">实例</param>
        /// <param name="memberName">属性名</param>
        /// <param name="value">属性值</param>
        /// <returns></returns>
        public static ValidResult Valid(object model,string memberName,object val) {
            string value = val.ToString();
            ValidResult result = new ValidResult();
            result.Result = true;
            result.ProPertyName = memberName;
            result.Value = value;
            try
            {
                //提取验证信息
                PropertyInfo info = model.GetType().GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);
                ValidateAttribute attr = info.GetCustomAttribute(typeof(ValidateAttribute)) as ValidateAttribute;
                if (attr == null) {
                    return result;
                }
                //默认常用验证模式
                if (attr.Items != 0)
                {
                    //非空
                    if ((attr.Items & ValidateEnum.NOT_NULL) == ValidateEnum.NOT_NULL)
                    {
                        if (FwUtilFunc.StringIsEmpty(value))
                        {
                            result.Result = false;
                            result.ErrorMessage += "必填 ";
                        }
                    }
                    //最大长度
                    if ((attr.Items & ValidateEnum.MAX_LENGTH) == ValidateEnum.MAX_LENGTH)
                    {
                        if (FwUtilFunc.StringIsNotEmpty(value)) {
                            if (attr.MaxLength != 0)
                            {
                                if (value.Length > attr.MaxLength)
                                {
                                    result.Result = false;
                                    result.ErrorMessage += "超过最大长度" + attr.MaxLength + " ";
                                }
                            }
                            else
                            {
                                throw new Exception("未设置最大值");
                            }
                        }
                      
                    }
                    //最小长度
                    if ((attr.Items & ValidateEnum.MIN_LENGTH) == ValidateEnum.MIN_LENGTH)
                    {
                        if (FwUtilFunc.StringIsNotEmpty(value)) {
                            if (attr.MinLength != 0)
                            {
                                if (value.Length < attr.MinLength)
                                {
                                    result.Result = false;
                                    result.ErrorMessage += "少于最小长度" + attr.MinLength + " ";
                                }
                            }
                            else
                            {
                                throw new Exception("未设置最小值");
                            }
                        }
                           
                    }
                    //日期
                    if ((attr.Items & ValidateEnum.IS_DATE) == ValidateEnum.IS_DATE)
                    {
                        if (FwUtilFunc.StringIsNotEmpty(value))
                        {
                            try
                            {
                                Convert.ToDateTime(value);
                            }
                            catch (Exception )
                            {
                                result.Result = false;
                                result.ErrorMessage += "为非法日期格式 ";
                            }
                        }
                        
                    }
                    //数字
                    if ((attr.Items & ValidateEnum.IS_NUMBER) == ValidateEnum.IS_NUMBER)
                    {
                        if (FwUtilFunc.StringIsNotEmpty(value)) {
                            try
                            {
                                Decimal.Parse(value);
                            }
                            catch (Exception )
                            {
                                result.Result = false;
                                result.ErrorMessage += "为非法数字格式 ";
                            }
                        }
                            
                    }
                }
                //自定义正则验证模式
                if (result.Result && FwUtilFunc.StringIsNotEmpty(attr.Regx))
                {
                    try
                    {
                        if (FwUtilFunc.StringIsNotEmpty(value) && !Regex.IsMatch(value, attr.Regx))
                        {
                            result.Result = false;
                            result.ErrorMessage += "未通过自定义正则验证:" + attr.Regx;
                            logger.Warn(value + "不符合自定义正则表达式" + attr.Regx);
                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                    
                }
                //自定义函数验证模式，验证函数的格式，必须有个一string参数，以及一个bool返回值,以及static关键字
                if (result.Result && FwUtilFunc.StringIsNotEmpty(attr.Function_Path))
                {
                    try
                    {
                        if (FwUtilFunc.StringIsNotEmpty(value))
                        {
                            //提取方法，验证方法信息
                            int sp_index = attr.Function_Path.LastIndexOf('.');
                            string classType = attr.Function_Path.Substring(0, sp_index);
                            string methodName = attr.Function_Path.Substring(sp_index + 1);
                            Type type = Assembly.GetEntryAssembly().GetType(classType);
                            MethodInfo mInfo = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.Static);
                            if (mInfo == null)
                            {
                                throw new Exception("自定义验证函数未找到,请确定函数路径且必须为static静态!");
                            }
                            ParameterInfo[] pInfos = mInfo.GetParameters();
                            if (pInfos.Length != 1 || pInfos[0].ParameterType != typeof(string))
                            {
                                throw new Exception("自定义验证函数必须为一个字符串类型参数!");
                            }
                            if (mInfo.ReturnType != typeof(bool))
                            {
                                throw new Exception("自定义验证函数返回值必须为bool布尔类型!");
                            }
                            //执行方法，取得结果
                            bool ret = true;
                            try
                            {
                                ret = (bool)mInfo.Invoke(null, new object[] { value });
                            }
                            catch (Exception)
                            {
                                ret = false;
                            }
                            //设置返回提示
                            if (!ret)
                            {
                                result.Result = false;
                                result.ErrorMessage += "未通过自定义验证方法 ";
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.ErrorMessage += "验证内部错误";
                logger.Warn(ex.Message);
            }
            //返回提示
            return result;
        }
    }
}
