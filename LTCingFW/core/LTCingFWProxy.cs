using log4net;
using Microsoft.CSharp;
using LTCingFW.utils;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;

namespace LTCingFW
{
    public class LTCingFWProxy
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(LTCingFWProxy));
        // private static String LTCingFWDllName = "LTCingFW_temp.dll";
        

        /// <summary>
        /// 创建所有的实例的代理
        /// </summary>
        /// <param name="type_full_name"></param>
        /// <returns></returns>
        public static void createProxyForInstance()
        {

            try
            {
                //检查DLL文件
                String curr_entry_assem_name = Assembly.GetEntryAssembly().Location;
                String curr_assem_name = Assembly.GetExecutingAssembly().Location;
                
                String curr_path = curr_assem_name.Substring(0, curr_assem_name.LastIndexOf('\\'));
                //String dllPath = curr_path + "\\" + LTCingFWDllName;
                //dllPath = dllPath.Replace("\\","/");
                //if (System.IO.File.Exists(dllPath))
                //{
                //    FwUtilFunc.DeleteFileOrDirectory(dllPath);
                //}
                //生成指定语言的代码生成器
                CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
                //编译器参数类
                System.CodeDom.Compiler.CompilerParameters parameters = new System.CodeDom.Compiler.CompilerParameters();
                //是否是可执行文件
                parameters.GenerateExecutable = false;
                //仅在内存中生成
                parameters.GenerateInMemory = true;
                //输出文件集合
                //parameters.OutputAssembly = LTCingFWDllName;
                //加入程序集
                Assembly[] AllAssembly = AppDomain.CurrentDomain.GetAssemblies();
                AssemblyName[] UsedAssembly = Assembly.GetEntryAssembly().GetReferencedAssemblies();
                AssemblyName[] UsedAssembly2 = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
                foreach (Assembly item in AllAssembly)
                {
                    if (!item.FullName.Contains("Microsoft.GeneratedCode"))
                    {
                        parameters.ReferencedAssemblies.Add(item.Location);
                    }
                }
                foreach (AssemblyName itemName in UsedAssembly2)
                {
                    bool hasFlag = false;
                    foreach (string ass in parameters.ReferencedAssemblies)
                    {
                        if (ass.Contains(itemName.Name + ".dll"))
                        {
                            hasFlag = true;
                            break;
                        }
                    }
                    if (!hasFlag)
                    {
                        parameters.ReferencedAssemblies.Add(itemName.Name+".dll");
                    }
                }
                //string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); 
                //parameters.ReferencedAssemblies.Add(curr_assem_name);
                //parameters.ReferencedAssemblies.Add(curr_entry_assem_name);
                //parameters.ReferencedAssemblies.Add("System.dll");
                //parameters.ReferencedAssemblies.Add("System.Data.dll");
                //parameters.ReferencedAssemblies.Add("System.Xml.dll");
                //parameters.ReferencedAssemblies.Add(curr_path+"\\log4net.dll");//log1:缺少AssemblyInfo.cs内配置，无法使用
                //程序代码
                String str = WavingProxyCode();
                //代码生成器执行编译,并生成DLL文件
                CompilerResults res = provider.CompileAssemblyFromSource(parameters, str);
                if (res.Errors.HasErrors)
                {
                    string error = "代理类型编译错误：";
                    foreach (CompilerError err in res.Errors)
                    {
                        error += err.ErrorText;
                        error += "\r\n";
                    }
                    throw new LTCingFWException(error);
                }
                Assembly autoAssembly = res.CompiledAssembly;
                //将生成的dll文件导入到程序集中
                //Assembly autoAssembly = Assembly.LoadFrom(LTCingFWDllName);
                //循环加入到实例集合中
                foreach (FwInstanceBean bean in LTCingFWSet.Beans)
                {
                    Object inst = autoAssembly.CreateInstance(bean.ProxyType);
                    bean.Instance = inst;
                    if (!LTCingFWSet.Instance.ContainsKey(bean.Name))
                    {
                        LTCingFWSet.Instance.Add(bean.Name, bean.Instance);
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 织入代理代码
        /// </summary>
        /// <param name="bean"></param>
        private static string WavingProxyCode()
        {
            //命名空间预设
            StringBuilder sb = new StringBuilder();
            sb.Append(" using System;\n using LTCingFW;\n using System.Threading;\n");
            sb.Append(" using log4net;\n");//log2
            sb.Append(" namespace " + FWConfigs.proxyNameSpace + " { \n ");
            foreach (FwInstanceBean bean in LTCingFWSet.Beans)
            {
                #region 代理类

                String bean_name = bean.Name;
                String type_full_name = bean.Type;
                String[] sps = type_full_name.Split('.');
                String LTCingFW_proxy_name = "_proxy_" + sps[sps.Length - 1];
                sb.Append(String.Format(" public class {0} : {1} {{  \n", LTCingFW_proxy_name, type_full_name));
                //3 sb.Append(String.Format(" private static readonly ILog logger = LogManager.GetLogger(typeof({0}));", LTCingFW_proxy_name));

                #region 代理方法

                Type type = bean.BelongAssembly.GetType(type_full_name);
                
                MethodInfo[] mtds = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (MethodInfo mi in mtds)
                {
                    #region 方法筛选
                    //去除掉队GetType、GetHashCode、Equals、ToString方法的重写
                    if (mi.Name == "GetType" || mi.Name == "GetHashCode" || mi.Name == "Equals" || mi.Name == "ToString")
                    {
                        continue;
                    }
                    //去除属性方法的重写
                    if (mi.Name.IndexOf("get_") >-1 || mi.Name.IndexOf("set_") >-1 )
                    {
                        continue;
                    }
                    //获取切面
                    FwAopBean aspect = GetMethodAspect(type_full_name, mi);
                    //获取DBSessionAttribute
                    DBSessionAttribute dbSessionAttr = GetMethodTransactionAttribute(mi);
                    if (dbSessionAttr == null) {
                        dbSessionAttr = GetClassTransactionAttribute(type);
                    }
                    #endregion

                    if (mi.Name == "NoticeDataChangedCallBack")
                    { }

                    #region 设置方法名、参数、返回值还原
                    String OverrideTag = "";
                    if (mi.IsVirtual)
                    {
                        OverrideTag = "override";
                    }
                    sb.Append(String.Format("\n public {0} {1} {2} ( ", OverrideTag, FwUtilFunc.GetRegularReturnType(mi.ReturnType), mi.Name));
                    #endregion

                    #region 方法内前置内容
                    ParameterInfo[] paramInfos = mi.GetParameters();
                    StringBuilder paramStr = new StringBuilder(" ");
                    StringBuilder objStr = new StringBuilder(" ");
                    for (int i = 0; i < paramInfos.Length; i++)
                    {
                        sb.Append(FwUtilFunc.GetRegularReturnType(paramInfos[i].ParameterType)).Append(" para").Append(i).Append(" ,");
                        if (paramInfos[i].ParameterType.IsByRef)
                        {
                            paramStr.Append(" ref");
                        }
                        paramStr.Append(" para").Append(i).Append(" ,");
                        objStr.Append(" para").Append(i).Append(" ,");
                    }
                    paramStr.Remove(paramStr.Length - 1, 1);
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append("){\n");
                    sb.Append(" object[] paras = new object[]{ ").Append(objStr).Append("};\n");
                    sb.Append(" bool outerSession = false;\n");
                    sb.Append(" DBSession session = null;\n");
                    if (mi.ReturnType.FullName != "System.Void") {
                        sb.Append(" object r = null; \n");
                    }
                    sb.Append(" try \n {\n");
                    sb.Append(" if (!LTCingFWSet.ThreadContextDic.ContainsKey(Thread.CurrentThread.ManagedThreadId)) {\n");
                    sb.Append(" LTCingFWSet.ThreadContextDic.Add(Thread.CurrentThread.ManagedThreadId, new LTCingFW.beans.ThreadContext());\n}\n");
                    sb.Append(" else if(LTCingFWSet.ThreadContextDic[Thread.CurrentThread.ManagedThreadId].DBSession != null)\n");
                    sb.Append(" {\n  outerSession = true;\n}\n");
                    #endregion

                    #region 方法前部分
                    //Transaction判定
                    if (dbSessionAttr!= null) {
                        sb.Append(" if(!outerSession){\n");
                        sb.Append(String.Format("session = DBSession.OpenSession(\"{0}\",{1});\n", dbSessionAttr.DbAlias, dbSessionAttr.OpenTransaction.ToString().ToLower()));
                        sb.Append("LTCingFWSet.ThreadContextDic[Thread.CurrentThread.ManagedThreadId].DBSession = session;\n");
                        sb.Append(" }\n");
                    }
                    //AOP
                    if (aspect != null) {
                        if (FwUtilFunc.StringIsNotEmpty(aspect.BeforeMethod))
                        {
                            //检测切入函数是否为公共和静态
                            int index = aspect.BeforeMethod.LastIndexOf('.');
                            String full_type_name = aspect.BeforeMethod.Substring(0, index);
                            String method_name = aspect.BeforeMethod.Substring(index + 1);
                            Type typeCls = Assembly.GetEntryAssembly().GetType(full_type_name);
                            MethodInfo mtdInfo = typeCls.GetMethod(method_name);
                            if (!mtdInfo.IsPublic || !mtdInfo.IsStatic)
                            {
                                throw new Exception("切入函数需为公共静态函数");
                            }
                            //加入切点函数
                            sb.Append(aspect.BeforeMethod).Append("(paras);\n");
                        }
                    }
                    #endregion

                    #region 原方法，中间部分
                    if (mi.ReturnType.FullName != "System.Void")
                    {
                        sb.Append(" r = ").Append("base.").Append(mi.Name).Append("(").Append(paramStr).Append(");\n");
                    }
                    else
                    {
                        sb.Append("base.").Append(mi.Name).Append("(").Append(paramStr).Append(");\n");
                    }
                    #endregion

                    #region 方法后部分
                    //AOP
                    if (aspect != null)
                    {
                        if (FwUtilFunc.StringIsNotEmpty(aspect.AfterMethod))
                        {
                            //检测切点函数是否为公共和静态
                            int index = aspect.AfterMethod.LastIndexOf('.');
                            String full_type_name = aspect.AfterMethod.Substring(0, index);
                            String method_name = aspect.AfterMethod.Substring(index + 1);
                            Type typeCls = Assembly.GetEntryAssembly().GetType(full_type_name);
                            MethodInfo mtdInfo = typeCls.GetMethod(method_name);
                            if (!mtdInfo.IsPublic || !mtdInfo.IsStatic)
                            {
                                throw new Exception("切入函数需为公共静态函数");
                            }
                            //加入切点函数
                            sb.Append(aspect.AfterMethod).Append("(paras);\n");
                        }
                        if (FwUtilFunc.StringIsEmpty(aspect.BeforeMethod) && FwUtilFunc.StringIsEmpty(aspect.AfterMethod))
                        {
                            throw new Exception("切面需要一个前置切入函数或一个后置切入函数");
                        }
                    }
                    #endregion

                    #region 方法尾部处理
                    sb.Append(" }\n catch (Exception ex) { \n ");
                    //sb.Append(" logger.Warn(\"Proxy_InnerException:\"+ex.Message+ex.StackTrace);\n");
                    sb.Append(" if(session != null && session.Transaction != null) \n{session.RollBack(); session.Close();\n}\n");
                    sb.Append("  throw new LTCingFWException(\"事务回滚,：\"+ex.TargetSite.ToString()+ex.Message+ex.StackTrace); \n");
                    sb.Append(" }\n ");
                    sb.Append(" finally \n { \n if(!outerSession)\n{\n if(session != null && !session.IsClosed()) \n{ \n session.Close(); \n}\n ");
                    sb.Append(" if(LTCingFWSet.ThreadContextDic.ContainsKey(Thread.CurrentThread.ManagedThreadId))\n{ \n");
                    sb.Append(" LTCingFWSet.ThreadContextDic[Thread.CurrentThread.ManagedThreadId].DBSession = null;\n");
                    //sb.Append(" Console.WriteLine(\"清除线程上下文DBSession\"+Thread.CurrentThread.ManagedThreadId);\n");
                    sb.Append("}\n}\n}\n");
                    //返回值
                    if (mi.ReturnType.FullName != "System.Void")
                    {
                        sb.Append(" return (").Append(FwUtilFunc.GetRegularReturnType(mi.ReturnType)).Append(") r; \n");
                    }
                    //结束
                    sb.Append("}\n");
                    #endregion
                }
                #endregion

                sb.Append("}\n");

                #endregion

                //记录日志
                logger.Info(String.Format("create Proxy for bean[{0}]", bean.Name));
                bean.ProxyType = FWConfigs.proxyNameSpace + "." + LTCingFW_proxy_name;
            }
            //命名空间结束
            sb.Append(" } \n");

            return sb.ToString();
        }

        
        private static FwAopBean GetMethodAspect(string type_full_name, MethodInfo method) {
            foreach (FwAopBean aspect in LTCingFWSet.Aspects) {
                if (Regex.IsMatch(type_full_name, aspect.RegxClassScope) && Regex.IsMatch(method.Name, aspect.RegxMethodScope))
                {
                    return aspect;
                }
            }
            return null;
        }

        private static DBSessionAttribute GetMethodTransactionAttribute(MethodInfo method) {
            DBSessionAttribute attr = method.GetCustomAttribute<DBSessionAttribute>();
            return attr;
        }
        private static DBSessionAttribute GetClassTransactionAttribute(Type type) {
            DBSessionAttribute attr = type.GetCustomAttribute<DBSessionAttribute>();
            return attr;
        }

    }
}
