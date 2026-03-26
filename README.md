# ElShrine
My C# projects for Windows, Including libs and apps.


## ElShrineLib
基于IOC设计、内置轻量化的依赖注入实现，包含日志、配置、指令、参数解析、上下文隔离的插件管理等基建服务，和数据持久化、数据结构等辅助类，同时附带了一套可扩展的解释器，以及许多涵盖反射的类型处理、字符串拼接判断、集合数据的扩展方法；

该库的所有实现均无第三方依赖，可独立使用，且可自行扩展，具有较高通用性；

### 基建服务&引导程序入口

#### 引导程序：[ElShrine.Bootstrapper]

该库的基建服务依赖于Bootstrapper提供的静态入口，为了适配扩展，Bootstrapper接受标准IServiceProvider参数和本库要求的IModuleRegister参数；

注：在本库中，默认IServiceProvider可以获取未注册的服务实例，使用Transient模式进行实例化；

可以在初始化引导中使用默认的注册模式，这将使用本库中的轻量化IServiceProvider实现的依赖注入容器，也可以传入其他IServiceProvider。

初始化完成后，将执行在[Bootstrapper.RegisterFinalization]中的委托，默认情况下，引导程序首先扫描全部注册服务并进行实例化和缓存，再按照Priority降序执行委托；

注：服务的预实例化取决于是否具有InitializationInfoAttribute且开启预实例化，除非手动声明为不必预先实例化，否则均会在这一阶段实例化；

##### 基建服务[ElShrine.Modules]

1、结构化日志 [ILogManager]：

该日志使用类似于控制流追踪的模式来创建并维护一颗日志树，特点是可扩展行类型、结构化定义的富文本、使用Logger完成日志操作，using语句配合OpenScope来隐式地完成跨线程与堆栈层级的稳定树维护；

在错误处理上，采用冒泡模式，允许在级间批量处理错误，在错误记录时会保留原始Exception的引用信息；

树形日志文件输出：使用反序列化，从流式输出的日志中重建树，再“渲染”文本为可读的日志文件；

2、本地化 [ILocalizationManager]：

很基础、很简单的本地化功能，基于基本的键值替换完成；

3、插件管理 [IPluginManager]：

基于AssemblyLoadContext处理实现，允许在运行时加载插件，并自动完成依赖注入；提供IPlugin作为插件实现接口和PluginInfoAttribute附带元数据信息；

插件管理器提供全面的状态感知，且保证上下文管理的准确性，避免内存泄漏问题；

同时，提供PluginResourceTracker\<TResourceBase\>来代理处理插件内部的单例资源管理，避免手动维护插件内资源倒转内存泄漏；

4、配置管理 [IOptionManager]：

使用设置基类OptionBase统一处理设置，使用特性OptionItemAttribute标记设置项，提供访问设置的统一入口，对插件管理进行了高度适配，以扫描方式处理设置类的管理，提供相应的状态感知；

在数据持久化上，将设置统一使用json格式处理内层数据序列化，再统一以xml格式储存，双层嵌套的主要目的是为了解决Type引用导致的上下文无法卸载问题，将上下文与配置相对独立处理；

注：可以使用OptionAttribute来配置设置目录信息

5、参数解析 [IParamParserManager] & 指令管理[ICommandManager]：

总体上说与设置管理类似，特化处理了不同上下文的扫描收集情况，这部分内容由CommandInovker调用，是本库提供的调试控制台的基础组件；

可以使用CommandCarrierAttribute来配置指令类信息，使用CommandAttribute来配置指令方法信息

### 数据结构

1. 二级索引项的查询、批量查询索引器
2. 抽象树形结构及其扩展方法
3. 脏追踪数据基类
4. 流式的集合访问包装
5. 文件（路径）信息的封装，提供IO访问
6. 缓存层
7. 注册表

### 解释器

1. Tokenizer
2. Parser：ExpressionParser & StatmentParser
3. 自定义的抽象语法树，AST
4. 自定义的AST执行上下文，ASTContext

### 通用序列化（DataHanlder\<TData\>）

### 异步相关

1. 缓存失效验证
2. 支持异步数据拉取的分页器，支持页跳转、范围预加载、缓存等优化数据访问的功能，低请求负担，自带请求合并
3. 用于同步锁定的强引用容器
4. 异步扩展方法，执行等待、取消、超时等操作

### 设置&指令

1. 全局指令：[ElShrine.Commands.GlobalCommands]
1. 本地化指令：[ElShrine.Commands.LocalizationCommands]
1. 配置指令：[ElShrine.Commands.OptionCommands]
1. 插件指令：[ElShrine.Commands.PluginCommands]
1. 日志指令：[ElShrine.Commands.LogCommands]

额外：进程相关指令：[ElShrine.Commands.ProcessCommands]

1. 全局设置：[ElShrine.OptionsCommonOptions]

### 特性

提供ValidableAttributeBaes用于在读取程序集中的属性时进行复杂的属性验证逻辑；

库当前包括对方法、属性、字段、类型的基本验证实现；

## ElShrineLib.Graphics

主要添加ColorData作为标准的颜色数据结构，提供色彩空间转换、序列化和反序列化、Hex转换等功能；

ColorData内部使用一个uint32储存实际颜色数据；

## ElShrineLib.Wpf.Generators.Attributes & ElShrineLib.Wpf.Generators

用于的WPF代码生成器，基于ElShrineLib.Wpf.Generators.Attributes，提供代码生成器和对应标记特性，用于生成WPF组件的代码；

## ElShrineLib.Wpf

WPF库，基于ElShrineLib、ElShrineLib.Graphics、ElShrineLib.Wpf.Generators.Attributes，提供一整套原生的WPF组件，以及对应的基建服务；

该系列组件为主题组件，使用代码生成器来自动实现接口实现伪多继承，从而从原生控件上继承，保留接口不变的情况下以将整套原生组件替换为支持高度可配置动画、主题的控件组；

动画系统基于ElShrineLib的解释器，将解释器作为Converter处理动画表达式来构建可执行表达式，从而允许在表达式中配置动态的动画参数计算；

## ElShrine.VisualTool

基于ElShrineLib.Wpf简单完成的WpfApp，UI插件化的应用程序，其只包括一个IPluginUIManager服务用于插件管理的可视化相关内容；

这个服务同时定义了可被加载为页面的插件的对应接口；