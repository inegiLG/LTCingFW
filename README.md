#LTCingFW说明#

##一、概述

LTCingFW是一个轻量级C#Winform应用的框架，它主要用于帮助程序简单化编程和整体规范化架构。本着一切简单化的原则，尽可能的让一切可读性、操作性提高，让程序员的工作量变少。

###1.1 目录：目录结构

+ 项目
	+ 引用
		+ log4net.dll
		+ LTCingFW.dll
	+ AOP
    	+ ASPECTS.cs
	+ MVC
		+ CONTROLLER
		+ MODEL
		+ SERVICE
		+ VIEW
		+ VIEWMODEL
	+ RESOURCE
    + UTIL
    App.config
	log4net.config
	LTCingFW.xml

图1-1

###1.2 使用：
####1.2.1 将LTCingFW.xml放到你的项目中，并设置为始终复制。
```html
<?xml version="1.0"?>
<LTCingFW>
  <beans>
    <bean name="OPCControl" type ="OPCServerRW.OPCControl"></bean>
  </beans>
  <aspects>
    <aspect scope="*.Service.*.~Save" beforemethod="LGPMS.Aop.AopPlugIns.testBeforeAop" aftermethod="LGPMS.Aop.AopPlugIns.testAfterAop"/>
  </aspects>

  <dbs>
    <db dbAlias="labdb">
      <providername>System.Data.SqlClient</providername>
      <connectionstring>Data Source=192.168.12.241;Initial Catalog=test;User ID=XX;Password=XXX;Pooling=True;Min Pool Size=1;Max Pool Size=500;Connect Timeout=5</connectionstring>
    </db>
    <db dbAlias="oracle1">
      <providername>Oracle.ManagedDataAccess.Client</providername>
      <connectionstring>DATA SOURCE=192.168.12.241:1522/test;USER ID=XXX;PASSWORD=XXXX;POOLING=True;MAX POOL SIZE=500;DECR POOL SIZE=2;CONNECTION TIMEOUT=5;INCR POOL SIZE=5;MIN POOL SIZE=1</connectionstring>
    </db>
    <db dbAlias="mysql1">
      <providername>MySql.Data.MySqlClient</providername>
      <connectionstring>server=127.0.0.1;database=test;user id=xxx;password=xxx;connectiontimeout=5;pooling=True;maxpoolsize=500;minpoolsize=1</connectionstring>
    </db>
  </dbs>
  <configs>
    <sqlserverdatalocation>E:\SqlserverData\</sqlserverdatalocation>
  </configs>
</LTCingFW>
```

####1.2.2 引入log4net
目前使用的版本是2.0.11，给出案例log4net.config,将其与LTCingFW.xml放于同一目录，设置始终复制。
使用debug模式，可以显示sql语句。
```html
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <configSections>
    <section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, log4net" />
  </configSections>
  <log4net debug="true">
    <root>
      <level value="debug" />
      <appender-ref ref="RollingLogFileAppender" />
      <appender-ref ref="ConsoleAppender" />
    </root>
    <appender name="ConsoleAppender"  type="log4net.Appender.ConsoleAppender" >
      <layout type="log4net.Layout.PatternLayout">
        <conversionPattern value="[%date] %level [%thread][%c{1}:%line] - %m%n" />
      </layout>
    </appender>
    <appender name="RollingLogFileAppender" type="log4net.Appender.RollingFileAppender" >
      <file value="./log/logfile_" />
      <param name="AppendToFile" value="true" />
      <param name="MaxSizeRollBackups" value="100" />
      <param name="MaxFileSize" value="10240" />
      <param name="StaticLogFileName" value="false" />
      <param name="DatePattern" value="yyyy-MM-dd&quot;.log&quot;" />
      <param name="RollingStyle" value="Date" />
      <layout type="log4net.Layout.PatternLayout">
        <conversionPattern value="[%date] %level [%thread][%c{1}:%line] - %m%n" />
      </layout>
    </appender>
  </log4net>
</configuration>
```

##二、IOC

IOC(Inversion of Control)控制反转，用于减少程序间的耦合度。我们将程序的模块组件的依赖关系交给框架来管理，而不是由我们自己创建，自己把一些实例赋值给另一些实例中，这会导致各个组件的依赖十分混乱。
在框架中，由框架来管理我们组件的生命周期，包括框架启动时创建和程序结束时释放销毁，在通过依赖注入的方式，使得各个组件能够得到其他组件的引用。
在这个过程中，我们需要完成以下两个步骤：
1. 注册组件实例
2. 实例注入

###2.1注册组件实例
注册组件实例有以下两种方法：
1.使用特性

我们需要使用[Controller][Service][Dao]来告诉框架我们注册的实例是什么。这3种特性是针对类的特性，框架会将标注了以上特性的类自动生成实例。这些实例作为Bean存储于我们框架的LTCingFwSet中的Beans属性中，我们可以从中按照Name属性提取出来。

```html
    [Controller(Name="cmsControl")]
    public class CMS_CONTROLLER
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(CMS_CONTROLLER));
        [Injected(Name = "CMS_COMMON_SERVICE")]
        protected CMS_COMMON_SERVICE CMS_COMMON_SERVICE_INST;
```
图2-2
图2-2中的[Controller]表示该类会被框架自动创建实例。它是一个Controller组件。该组件的名字为CmsControl。当我们不配置特性的Name属性时，默认会把类名作为Name属性的值，类似的特性还有[Service]和[Dao]。

2.在配置文件中配置
```html
  <beans>
    <bean name="OPCControl" type ="OPCServerRW.OPCControl"></bean>
  </beans>
```
图2-3
对于程序外部引用的类，我们可以在LTCingFW.xml配置文件中，配置组件类的名字和全名。

###2.2 实例注入
所有的组件实例都是已经创建好的，获取实例的引用，有两种方式：
1.使用【inject】特性
在图2-2中，我们看到有一个名为CMS_COMMON_SERVICE的Service被注入给了Controller，此时我们没有创建过该Service，便可以使用引用的Service了。

2.手动提取
实例注入完全是在组件实例之间完成的，当我们想在一个并不是组件的类（比如一个静态类）中使用组件实例，我们需要无法使用注入的方式，但是可以从框架中提取出该组件实例。方法如下：


```html
        OPCControl opcControl = LTCingFW.LTCingFWSet.GetInstanceBean("OPCControl") as OPCControl;

        CMS_COMMON_SERVICE CMS_COMMON_SERVICE_INST = LTCingFWSet.GetInstanceBean("CMS_COMMON_SERVICE") as CMS_COMMON_SERVICE;
```
图2-4
图2-4中在线程中得到名为OPCControl的组件实例的引用和名为CMS_COMMON_SERVICE的组件实例的引用。

##三、ORM
ORM，即对象关系映射（英语：Object Relational Mapping，简称ORM，或O/RM，或O/R mapping），其主要的目的是建立程序的类和数据库中的表的对应关系，即一个ORM类就是一个数据库表。数据库中的每一行数据对应程序中ORM类（在本框架中是继承了OrmBaseModel的类）的一个实例，表的列是实例的属性。在编写我们的ORM类的过程中，即是编写对应数据库中表的信息。
LTCingFW框架中使用的ORM类如下图所示：

```html
namespace CalcModSimulate.MVVM.MODEL
{
    [OrmTable("TC_DYN_CAL")]
    public class CT_TC_DYN_CAL : OrmBaseModel
    {
        [OrmColumn("HEAT_NO", (int)OrmDataType.CommonType.STRING, PrimaryKey = true)]
        public string s_HEAT_NO { get; set; }
        [OrmColumn("TSC_C_MEAS", (int)OrmDataType.CommonType.DECIMAL)]
        public string d_TSC_C_MEAS { get; set; }
        [OrmColumn("TSC_TEMP_MEAS", (int)OrmDataType.CommonType.INT)]
        public string i_TSC_TEMP_MEAS { get; set; }
        [OrmColumn("DYN_O2_CAL", (int)OrmDataType.CommonType.INT)]
        public string i_DYN_O2_CAL { get; set; }
        [OrmColumn("DYN_O2_REAL", (int)OrmDataType.CommonType.INT)]
        public string i_DYN_O2_REAL { get; set; }
        [OrmColumn("DYN_COOL_CAL", (int)OrmDataType.CommonType.INT)]
        public string i_DYN_COOL_CAL { get; set; }
        [OrmColumn("DYN_COOL_REAL", (int)OrmDataType.CommonType.INT)]
    }
}
```
图3-1
1.类必须以【OrmTable】进行标注，其后必须跟着实际数据库中的实际表名。
2.类必须继承OrmBaseModel。
3.类中属性，凡是用于对应数据库表中的列，则必须使用属性。
4.类中属性，凡是用于对应数据库表中的列，则必须标注【OrmColumn】特性，三个参数分别是列名、列类型、是否为主键。列类型分为六种，在OrmDataType.CommonType枚举中选择：字符、小数、整形、布尔、日期。框架会根据数据类型进行读取、写入时的类型转换。
5.类中属性，凡是用于对应数据库表中的列，则必须使用string类型。框架会将结果转为string类型。如果用于计算等，请使用时自行转换。
6.类中的列对应属性，是真实数据表中列集的子集，但是主键列必须包含，因为如果主键列不全，进行的数据库操作可能失败。

##四、MVC架构
###4.1 整体架构

Controller层依赖Service层，Service层依赖Dao层，不允许出现同级依赖的情况，或者低级依赖高级的情况。


###4.2 Dao数据连接层
Dao意思是Data Access Object，它的作用就是和数据库交互，一个Dao组件实例对应一个数据库中的一个表的操作，一个Dao组件实例中的方法便是对一个数据库中的一个表的增删改查操作。它的可操作范围空间仅仅为一个数据库中的一个表，但是决定是哪个表由OrmModel决定，Dao层提供方法。

```html
[DAO]
public class CAR_DAO : OrmBaseDao
{ 
    public void insert(Car model)
    {
        Insert(model);
    }
}
```
图4-2
DAO层使用【DAO】特性标注，框架已经给出了基础的Dao类(名为OrmBaseDao)，并将该类作为组件实例注册到了框架中，该方法提供几乎所有用到的增删改查方法（条数查询系、分页查询系、查询系、新增系、修改系、删除系），继承该类即可使用其所有的方法。
在LTCingFW框架中，DAO组件仅仅是一个方法的集合，不包含任何表的信息，诸如表名、列名、列类型等信息全部来自OrmBaseModel类。即相比于传统MVC框架，LTCingFW的DAO层不包含任何特殊的信息，因此一个公共的DAO组件实例即可满足所有的Service组件实例。
或者我们可以不写DAO层，直接把OrmBaseDao注入到我们的Service组件实例中去。或者不使用DAO层，因为OrmBaseModel也继承了OrmBaseDao，所以可以如下写：

```html
namespace CalcModSimulate.MVVM.SERVICE
{
    [DBSession("mesdb")]
    [Service]
    public class MES_COMMON_SERVICE : OrmBaseService
    {
        public virtual List<T> QueryTFromMES<T>(T model) where T : OrmBaseModel
        {
            //return dao.SelectT<T>( model);
            return model.SelectT<T>();
        }
    }
}
```
图4-3
图3-3中的两句话是一样的，所以可以直接省略DAO层。

###4.3 Service服务层
Service服务层内可以注入多个Dao组件实例，因此服务层的方法可以对同一个数据库的多个表进行操作，服务层的每一个方法给与一个数据库连接（DBSession），告知该服务层方法需要连接哪个数据库，对于非查询操作可以在DBSession中开启事务，以确保多表操作的原子性。

```html
    [DBSession("mesdb", OpenTransaction = true)]
    [Service]
    public class MES_COMMON_SERVICE : OrmBaseService
    {

    }
```
图4-4
Service层使用[Service]特性标注，框架给出了基础的Service类(名为OrmBaseService),该类包含简单的单表增删改查方法。
在使用该类的时候需要注意：
1，原则上我们希望一个Service只对应一个数据库，可操作范围是一个数据库。所以我们在自己的定义的Service类上加入【DBSession】特性，则该类的所有方法都使用相同的的数据库连接地址。
2，OrmBaseService类已经有注入OrmBaseDao，所以如果我们的Service类继承了OrmBaseServcie，可以不写注入DAO。
3，OrmBaseService类中有对于单表操作的简单方法，基本上可DAO层的方法一样，这些方法的DBSession全部都是使用Service类上标注的
【DBSession】特性来建立数据库连接，所以如果Service类上没有标注【DBSession】特性，这些方法会找不到DBSession而报错。多表操作还需要自己写方法。

Service层的每个方法都可以添加自己的【DBSession】特性，但是我们建议这个方法写到另建的Service类中去。如下图：

```html
namespace CalcModSimulate.MVVM.SERVICE
{
    [Service]
    public class MES_COMMON_SERVICE : OrmBaseService
    {
        [DBSession("localdb")]
        public virtual T QueryByPk<T>(T model) where T : OrmBaseModel
        {
            List<T> list = model.SelectByPrimaryKey<T>(model);
            if (list.Count > 0)
            {
                return list[0];
            }
            return null;
        }
    }
}
```
图4-5
在方法的DBSession优先级要大于在类上的DBSession。请注意所有的Service层的方法全部使用virtual 方法，表示该方法是虚方法，因为框架会对这些方法进行重写，如果不加virtual，该方法便无法得到DBSession数据库连接。

###4.4 Controller控制层
Controller层中可以注入多个Service组件实例，该层的方法可以操作多个Service，即多个数据库，也就是最大的可操作范围。是主要的业务逻辑编写地点。在该层可以掌握程序的全局信息来处理业务。

```html
namespace CalcModSimulate.MVVM.CONTROLLER
{
    [Controller(Name="cmsControl")]
    public class CMS_CONTROLLER
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(CMS_CONTROLLER));

        [Injected(Name = "CMS_COMMON_SERVICE")]
        protected CMS_COMMON_SERVICE CMS_COMMON_SERVICE_INST;

        public virtual void GetCTData(MainFormViewModel vm)
        {
            string luhao = vm.BOF_NO;
            vm.LAB_HM_MN = "VM34FG566";
        }
    }
}
```
图4-6
Controller层使用【Controller】特性标注。Controller的所有方法必须使用virtual，图3-5中GetCTData方法的参数是MainForm的双向绑定实例。该实例中的属性和MainForm中的文本框等组件的内容双向绑定，即改变一方，另一方会立马改变。图中的方法读取BOF_NO属性，然后又给BOF_NO赋值，改变了页面上的显示。之后会在ViewModel一章讲双向绑定的方法。

##五、DBSession
DBSession是数据库连接的意思，我们需要配置好连接信息。
###5.1 配置


```html
  <dbs>
    <db dbAlias="labdb">
      <providername>System.Data.SqlClient</providername>
      <connectionstring>Data Source=192.168.12.241;Initial Catalog=test;User ID=XX;Password=XXX;Pooling=True;Min Pool Size=1;Max Pool Size=500;Connect Timeout=5</connectionstring>
    </db>
    <db dbAlias="oracle1">
      <providername>Oracle.ManagedDataAccess.Client</providername>
      <connectionstring>DATA SOURCE=192.168.12.241:1522/test;USER ID=XXX;PASSWORD=XXXX;POOLING=True;MAX POOL SIZE=500;DECR POOL SIZE=2;CONNECTION TIMEOUT=5;INCR POOL SIZE=5;MIN POOL SIZE=1</connectionstring>
    </db>
    <db dbAlias="mysql1">
      <providername>MySql.Data.MySqlClient</providername>
      <connectionstring>server=127.0.0.1;database=test;user id=xxx;password=xxx;connectiontimeout=5;pooling=True;maxpoolsize=500;minpoolsize=1</connectionstring>
    </db>
  </dbs>
```
1.DBSession的配置写于LTCingFW.xml中的<dbs></dbs>中。
2.dbAlias为别名，自己定义好后，作为识别标识使用。
3. Providename表示其连接为mysql、oracle、sqlserver，且为固定值，分别为【MySql.Data.MySqlClient】【Oracle.ManagedDataAccess.Client】【System.Data.SqlClient】，mysql使用5.5到5.7版本，其他版本未测试，oracle使用9i以后版本，sqlserver使用2008以后版本。
4. Connectingstring是连接字符串，由数据库供应商自行定义的写法，可以从网上找到。

###5.2 使用
在LTCingFW框架中，Service层的方法对应了一个数据库范围的操作，所以DBSession主要用于Service层的方法上。可以写在Service的类上或者方法上。需要注意Service层的方法都必须是virtual方法，如图5-2。
如果Controller层的方法仅仅使用一个数据库，也可以将DBSession写在Controller层的方法上。此时Controller层调用内部Service层的方法时，使用的是外层Controller方法的DBSession，也就是说外层DBSession优先级大于内层DBSession优先级，这样可以减少DBSession的创建。但是一般无需在Controller层上添加DBSession。
另外，DBSession可以使用如下方式从上下文中提取出来，如图5-2：
```html
        [DBSession("localdb")]
        public virtual List<T></T> QueryT<T>(T model) where T : OrmBaseModel
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;

            return dao.Select<T>(session,model);
        }
```
图5-2
可以使用
```html
LTCingFWSet.GetThreadContext().DBSession
```
获取，这里即便不写session参数，框架也会自动将Service层方法的session传入,即：
```html
return dao.Select<T>(model);
```

###5.3 事务
可以为DBSession开启事务，如图5-3所示：

```html
        [DBSession("localdb", OpenTransaction = true)]
        public virtual void UpSertByPk2(OrmBaseModel update_model)
        {
            DataTable rsl = dao.SelectByPrimaryKey( update_model);
            if (rsl.Rows.Count == 1)
            {
                dao.Update( update_model, true);
            }
            else
            {
                dao.Insert( update_model);
            }
        }
```
图5-3
开启的语法是，为DBsession特性添加OpenTransaction = true，开启事务后Service层的方法内，对多表的多次操作会使用同一事务，若中间有操作失败会一起回退。一般只针对于包含有新增、修改、删除的操作。

###5.4 动态注册和使用
DBSession是存放于LTCingFWSet中的，可以动态添加，语法如图5-4所示：

```html
    String ConnectionString = "server=" + cinfo.dbIP +";Port=" + cinfo.dbPort + ";user id="+cinfo.dbUserName+";password="+cinfo.dbPassword+";connectiontimeout=5;pooling=True;maxpoolsize=500;minpoolsize=1";
    FwUtilFunc.AddDBSession("mysql", "localdb", ConnectionString);
```
图5-4
使用FwUtilFunc.AddDBSession方法，三个参数分别为，数据库类型(mysql,sqlserver,oracle),别名，连接字符串。
当我们确保已经注册之后，可以使用DBSession.OpenSession(别名，是否开启事务)的方式，创建DBSession。

##六、AOP
AOP是面向切面的意思，当我们想针对某一类方法添加统一的操作时，可以将这一类的方法看做一个切面，在方法的执行前后添加一些操作。

###6.1 配置
使用AOP需要配置在LTCingFW.xml中，如图6-1所示：

```html
<LTCingFW>
  ...
  <aspects>
    <aspect scope="*.MVVM.CONTROLLER.*.~CTData" beforemethod="CalcModSimulate.AOP.ASPECTS.BeforeAction" aftermethod="CalcModSimulate.AOP.ASPECTS.AfterAction"/>
  </aspects>
  ...
```
图6-1
方法配置需要填写几个参数：
scope：表示针对于哪些方法，【*】表示任意路径名，【.】表示路径间隔，【~】表示任意不完整字符串，如~CTData表示以CTData结尾的所有字符串。
beforemethod：表示在原方法之前执行的方法的路径和方法名，参考图6-2。
aftermethod：表示在原方法之后执行的方法的路径和方法名，参考图6-2，即便原方法出现错误，此方法也会执行。

###6.2 切面方法

```html
namespace CalcModSimulate.AOP
{
    public static class ASPECTS
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ASPECTS));
        //前方法
        public static void BeforeAction(object[] paras) {
            //显示方法名
            String methodName = LTCingFWSet.ThreadContextDic[Thread.CurrentThread.ManagedThreadId].MethodName;
            logger.Info("进入方法：" + methodName);
            for (int i = 0; i < paras.Length; i++)
            {
                logger.Info("参数[" + i + "]：" + paras[i].ToString());
            }
        }
        //后方法
        public static void AfterAction(object[] paras) {
            //打印错误
            Exception ex = LTCingFWSet.ThreadContextDic[Thread.CurrentThread.ManagedThreadId].Error;
            if (ex != null) {
                logger.Warn(ex);
            }
        }
    }
}
```
图6-2
框架对切面方法有要求：
1.切面方法必须为公共的静态函数。
2.切面方法的参数必须为object[],框架会将原函数的参数作为数组传入。
3.错误提取，可以通过以下方式从切面方法中提取原方法发生的错误：
LTCingFWSet.ThreadContextDic[Thread.CurrentThread.ManagedThreadId].Error
4.如果有多个切面匹配到方法，只用第一个切面。

##七、VIEWMODEL
ViewModel的作用是与Winform页面上的诸如文本框等的控件显示内容做双向绑定，即改变一方，另一方自动作出改变。使得页面的内容和ViewModel中的属性一一对应。如果我们在Controller中想要改变页面内容，直接修改ViewModel参数即可，解耦了业务逻辑程序和页面显示之间的紧密关系，规避原有的将逻辑写于Winform页面中导致庞大复杂臃肿的局面。
编写ViewModel需要以下几个步骤：

###7.1 编写ViewModel

```html
namespace CalcModSimulate.MVVM.VIEWMODEL
{
    public class MainFormViewModel : BaseViewModel
    {
        private string _BOF_NO;
        [Validate(Items = ValidateEnum.NOT_NULL, Regx = "^L880-[0-9]{7}$")]
        public string BOF_NO { get { return _BOF_NO; } set { SetProperty(ref _BOF_NO, value); } }
        private string _GRADE_NO;
        public string GRADE_NO { get { return _GRADE_NO; } set { SetProperty(ref _GRADE_NO, value); } }
        private string _HM_WGT;
        public string HM_WGT { get { return _HM_WGT; } set { SetProperty(ref _HM_WGT, value); } }
        private string _CHG_HM_TEMP;
        public string CHG_HM_TEMP { get { return _CHG_HM_TEMP; } set { SetProperty(ref _CHG_HM_TEMP, value); } }
    }
}
```
图7-1
1.ViewModel类请继承BaseViewModel类
2.创建私有变量和对应属性，get方法和set方法按照图示中书写，其中SetProperty为基类BaseViewModel中的方法。

###7.2 在Winform中绑定控件属性

```html
        public MainForm()
        {
            InitializeComponent();
            this.Model = new MainFormViewModel();
            //绑定
            textBox26.DataBindings.Add("Text", this.Model, "BOF_NO");
            textBox23.DataBindings.Add("Text", this.Model, "GRADE_NO");
            textBox61.DataBindings.Add("Text", this.Model, "HM_WGT");
            textBox73.DataBindings.Add("Text", this.Model, "CHG_HM_TEMP");
            textBox62.DataBindings.Add("Text", this.Model, "SCRAP_WGT");
            textBox1.DataBindings.Add("Text", this.Model, "FORCAST_C");
```
图7-2
1.创建ViewModel实例。
2.将控件的属性绑定到ViewModel实例的属性上，语法为：
控件.DataBindings.Add(控件属性名，ViewModel实例，ViewModel实例的属性名)
如图中案例：将TextBox的Text的属性与MainFormViewModel的BOF_NO属性绑定，不仅仅是Text属性可以绑定，基本控件中的大多数属性都可以绑定。

###7.3 属性有效性验证

```html
        private string _BOF_NO;
        [Validate(Items = ValidateEnum.NOT_NULL,Regx = "^L880-[0-9]{7}$")]
        public string BOF_NO { 
            get {
                return _BOF_NO;
            }
            set 
            { 
                SetProperty(ref _BOF_NO, value);
            } 
        }
```
框架可以为ViewModel的绑定属性做有效性验证。实现验证需要在ViewModel的属性上（注意非字段）添加【Validate】特性，该特性有两个验证属性，分别是Item验证和Regx验证，Item验证包括非空、最大字符串长度、最小字符串长度、是否为日期、是否为数字；Regx验证是正则表达式验证，可以同时使用两种方式验证。
1.Item验证：选择LTCingFW.ValidateEnum中的一种，分别是：
NOT_NULL（非空），不可为空。
MAX_LENGTH（最大长度），不可超过最大长度，使用该验证必须给【Validate】特性的MaxLength属性赋值。
MIN_LENGTH（最小长度），不可少于最小长度，使用该验证必须给【Validate】特性的MinLength属性赋值。
IS_DATE （日期），必须为日期的字符串，即Convert.ToDateTime(value)无错即可。
IS_NUMBER（数字），必须为数字的字符串。
2.Regx验证：给【Validate】特性的Regx属性写入正则表达式后，自动启用。框架自动验证验证属性的值是否与【Validate】特性的Regx正则表达式相匹配（IsMatch）。

验证结果存放于ViewModel实例的ValidResultDic字典里，以属性名为key，以ValidResult为值。ValidResult里有验证结果、错误信息、属性名、属性值。可以使用ValidResultDic.clear()清空验证结果。

##八、其他功能

###8.1 Winform命令异步执行

框架自带Winform增强功能，使用需要使Winform继承框架的BaseForm。通常我们点击按钮后如果运行过长，会导致界面卡死，如果使用异步执行功能，则不会出现该情况。

```html
        //按钮点击
        private void sysControlbtn_Click(object sender, EventArgs e)
        {
            string[] prms = new string[4] { configVM .ClientIP, configVM.ServerIP, configVM.ServerPort ,null};
            AsyncExecOnceControllerMethod("RequestClientConfigThread", ControllerName, "RequestClientConfig", prms, sysControlbtn_Click_CallBack);
        }

        private void sysControlbtn_Click_CallBack(object result)
        {
            RetMsg ret = (RetMsg)result;
            if (ret.code != "0")
            {
                MessageBox.Show("获取失败：\n" + ret.message);
            }
        }
```
图8-1
使用BaseForm中的AsyncExecOnceControllerMethod方法，第一个参数是线程名，第二个参数是【Controller】组件实例的名字，第三个参数是Controller里的执行方法名，第四个参数是执行方法的参数集和（object [ ]），第五个参数是回调函数，回调函数的参数必须为object，返回值为void。其中回调函数的参数就是原Controller方法的返回值。

###8.2 简单线程

框架自带了简单线程的基类BaseThread和简单线程的启停方法。减少编写线程时的代码量。使用简单线程需要自定义线程类，并继承BaseThread。继承之后自定义实现run方法。

```html
namespace CalcModSimulate.THREAD
{
    public class L2_COLLECT_THREAD : BaseThread
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(L1_COLLECT_THREAD));

        public override void run(object param)
        {

            while (IsOpen)
            {
                try
                {

                }
                catch (Exception e)
                {
                    logger.Warn(e.ToString());
                }
                Thread.Sleep(LoopRate * 3);
            }
        }
    }
}
```
图8-2
图8-2给出了简单线程类的基础写法，该线程类继承了BaseThread，并实现了BaseThread类的抽象方法run，IsOpen是BaseForm中的属性，关闭线程会将IsOpen置为False，线程自动结束。LoopRate是BaseForm的属性，表示循环间隔，默认为1000ms，可以自行修改。
简单线程的启停分别使用以下方法：
启动：FwUtilFunc.OpenThread(threadContext,threadName,param);
参数1：继承了BaseThread的线程类实例，参数2：线程名，参数3：run方法的参数。
停止：FwUtilFunc.CloseThread(threadName);
参数1：线程名。

###8.3 HTTP请求

框架自带了简单的HTTP请求方法。包括GET方法和POST方法。
GET方法：
LTCingFW.utils.HttpUtil.HttpGet(string Url)
参数1：访问地址
返回结果字符串。

POST方法：
LTCingFW.utils.HttpUtil.HttpPost(string url, string postDataStr,int waitTime_ms)
参数1：访问地址，参数2：访问时附带内容，参数3：等待时间
返回结果字符串。

###8.4 自定义配置

参见图1-1中的
```html
<LTCingFW>
  <configs>
    <sqlserverdatalocation>E:\SqlserverData\</sqlserverdatalocation>
  </configs>
</LTCingFW>
```
可以在程序中使用如下命令读取出来：
```html
  LTCingFWSet.GetUserDefinedConfig("//LTCingFW/configs/sqlserverdatalocation");
```
###8.5 级联查询

在自定义的ORM类中，如果加入级联表属性，可以级联查询级联表，写法如下
```html
    [OrmTable("CMS_REALTIME_DATA")]
    public class CMS_REALTIME_DATA : OrmBaseModel
    {
		[OrmColumn("HEAT_NO", (int)OrmDataType.CommonType.STRING,PrimaryKey = true)]
		public string HEAT_NO { get; set; }
		[OrmColumn("RECORD_TIME", (int)OrmDataType.CommonType.DATE, PrimaryKey = true)]
		public string RECORD_TIME { get; set; }
		[OrmForeign(LocalColumnName= "HEAT_NO",ForeignColumnName = "HEAT_NO",LZModel =LZModelEnum.LAZY)]
        public ForeignOrmModel<CMS_RECORD_DATA> FOR_RECORD { get; set; }
    }
```
最后一条属性为级联属性，使用级联属性必须注意以下几点：
1.返回值为ForeignOrmModel，泛型类型为级联的ORM类，属性名自定义。
2.必须使用[OrmForeign]特性标注。特性的参数LocalColumnName是本ORM类的属性，ForeignColumnName是级联类的属性，LZModel是饥饿查询还是懒惰查询。如果查询条件过多（即有多个列相等条件，A JOIN B ON A.a=B.a AND A.b=B.c）,LocalColumnName和ForeignColumnName的值按照对应顺序用逗号隔开（LocalColumnName="a,b",ForeignColumnName = "a,c"）。
3.提取级联结果
```html
CMS_REALTIME_DATA real = CMS_COMMON_SERVICE_INST.QueryByPk(nstRealData);
List<CMS_RECORD_DATA> records = real.FOR_RECORD.Result;
```
ForeignOrmModel的属性Result中提取，切类型为List<级联ORM类>。


作者：liugan
邮箱：liuganlg@163.com
