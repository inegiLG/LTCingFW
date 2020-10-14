#LTCingFW说明#

##一、概述

LTCingFW是一个轻量级C#Winform应用的框架，它主要用于帮助程序简单化编程和整体规范化架构。本着一切简单化的原则，尽可能的让一切可读性、操作性提高，让程序员的工作量变少。

###1.1 目录：目录结构
![图1-1][link_pic_1_1]
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

![图2-1][link_pic_2_1]
图2-1
图2-1是框架中使用的特性的继承关系，在实际中我们需要使用[Controller][Service][Dao]来告诉框架我们注册的实例是什么。这3种特性是针对类的特性，框架会将标注了以上特性的类自动生成实例。这些实例作为Bean存储于我们框架的LTCingFwSet中的Beans属性中，我们可以从中按照Name属性提取出来。

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

![图4-1][link_pic_4_1]
图4-1
图4-1中的空心箭头表示依赖关系，实心箭头表示数据交互。
依赖关系请按照图中所示，不允许出现Controller依赖Controller的情况，即高级依赖低级的关系。

###4.2 Dao数据连接层
Dao意思是Data Access Object，它的作用就是和数据库交互，一个Dao组件实例对应一个数据库中的一个表，一个Dao组件实例中的方法便是对一个数据库中的一个表的增删改查操作。它的可操作范围空间仅仅为一个数据库中的一个表。

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

[link_pic_1_1]:data:image/png;base64,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

[link_pic_2_1]:data:image/png;base64,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

[link_pic_4_1]:data:image/png;base64,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

