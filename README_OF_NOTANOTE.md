# Notanote



# 配置开发环境

## 引入 Package
`Unity Editor上方菜单` -> `Windows` -> `Package Manager` -> `Add package from git URL...`
分别导入下方三个Package

``` plain text
# Milease Core
https://github.com/MorizeroDev/Milease.git

# Color Tools
https://github.com/ParaParty/ParaPartyUtil.git?path=Colors

# Unity Native
https://github.com/ParaParty/ParaPartyUtil.git?path=UnityNative
```


## avifenc
*其实目前可以不用配这个部分，因为涉及这个部分的功能还没启用*

<h3>Windows平台</h3>

1.下载[msys2](https://www.msys2.org/)并安装；

+ 如果已经安装可以跳过这一步。

2.打开后在命令行里输入`pacman -S ucrt64/mingw-w64-ucrt-x86_64-libavif`并执行；<br>
3.等待执行完毕（注意，中途有让你确认是否执行安装的一步），然后将`msys2所在目录/ucrt64/bin`填入系统环境变量。

<h3>其他平台</h3>

没搞过，自己去学（？）

# 从Weblate获取最新版翻译文件

准备工作：打开[https://weblate.milthm.cn/accounts/profile/#api](https://weblate.milthm.cn/accounts/profile/#api)，找到`您的个人 API 密钥`一项，然后复制此秘钥（打码文字右边的复制按钮），在`path-to-project/Secrets`文件夹下新建一个名为`weblate_token.txt`的文件，然后将该秘钥粘贴进此文件并保存。

+ `weblate_token.txt`只需要创建一次，以后可以一直用。
+ 如果更换设备请直接从原设备上把文件拷过来，或重复上述步骤。
+ 该token与账户绑定，如果你点了上面链接中界面右侧那个`重新生成API密钥`，则需要将新生成的密钥复制并替换该txt文件中的内容。

拉取文件：找到Unity上方菜单栏中的`i18n`选项卡，点击`🔽 拉取远程翻译`，然后等待完成。如果报错可以先尝试自己修复，修复不了再去问主程序。

# 构建

准备工作：
1、确保已经拉取最新版翻译文件
2、在上方菜单栏中找到`Notanote` -> `Check`，检查并修复该子菜单下所有项目
3、在完成上述两项工作后进行commit，并对该commit添加一个tag（push不push看你心情）。

+ 构建前需要确保commit区没有非"Tolerant"类文件。（"Tolerant"类文件具体含义可去`WorkspaceDirtyJudgement.cs`看）
+ tag的内容一般情况下应符合[https://semver.org](https://semver.org)中提出的规范
  + 下面一条提示中提到的MAY关键字遵循
  + 一般格式是`A.B.C`或`A.B.C-custom`等，其中`A`,`B`,`C`分别为三个整数。`A`为大版本号，当且仅当进行了与旧版不兼容的版本或差距极大（例：更换了UI）的改动时推进；`b`为小版本号，当且仅当进行了功能类改动（增加、删除或修改了某个或某几个功能）时推进；`C`为补丁版本号，当且仅当进行了bug修复、性能优化等非功能类改动时推进；拥有`-`代表预览版，其中`custom`为自定义的字符串（一般是一个词组），用于描述该预览版本内容（如果什么都没有可以用`rc`+`数字`来代替）；除此之外还可以（MAY）加上`+buildExt`（例：`1.1.4+24yearsold`, `5.1.4+isstudent`），表示该版本的扩展信息。
+ 如何给指定commit添加tag请自行查阅相关资料。
+ *注意：一定要在添加tag之后才构建，否则可能会出现构建出的文件夹名与tag名不匹配的现象。*
+ **一个很抽象的bug：构建标记（即Scripting Defines）的修改有的时候不会立即生效，需要重启Unity才能生效。因此请记得检查`ProjectSettings\ProjectSettings.asset`是否有Changed记录。**

开始构建：首先，考虑好你要构建的版本，去`构建工具` -> `构建标记` -> `版本`中选择你需要构建的版本，然后在`构建工具` -> `构建`选择你需要的平台构建。如果准备工作都做好的话理论上不会报错。报错的话问主程序。

+ 关于版本选择：发布版是用于推送到Steam、Taptap正式渠道和App Store的包；普通版是没有任何特殊标记和特殊按键，没有控制台的包；内测版是内测用的包；开发版是组内用的包
+ 需要注意的是，构建对Unity编辑器运行的平台有要求。运行在Windows平台的包只能在Windows平台构建，而不能在Linux或MacOS平台构建；运行在MacOS平台的包只能在MacOS平台构建，而不能在Linux或MacOS平台构建。不过运行在Linux的包可以在三个平台中任意一个上构建。
  + *然而实际上Linux平台上和MacOS平台上都可以做到交叉编译运行在Windows的c++程序，但是Unity官方没做。*

# 关于导入谱面

文件结构参照其他谱面。要注意的是请把`imgs`和`imgs_xx`文件夹里的图片，也就是故事版图片的`Max Size`设为16384，并且把`Format`设置为RGBA 32 bit。
每首歌的文件夹名叫做曲目id，曲目id的命名规范：
+ 如果原曲曲名为中文，请使用拼音；
+ 如果原曲曲名为日文，请使用罗马音（不会翻的可以找群里的日语翻译，或者参考[这个网站](https://www.kawa.net/works/ajax/romanize/japanese.html)给出的罗马音并选择合适的罗马音）；
+ 如果原曲曲名为其他语言，请使用曲名的英文翻译，或者直接转化成对应的英文字母（可以按照字形，在该语言字母表中的位置等）；
+ 曲目id仅允许使用大写字母、小写字母、阿拉伯数字、空格、`.`、`[`、`]`、`{`、`}`和`;`，其中第一个字符仅允许使用大写字母或小写字母。尤其要注意不允许使用下划线（`_`），否则目前由`曲目id+难度`构成的谱面id的解析器会识别错误。

# 符号：
+ DEVELOPMENT_VERSION - 开发版本
+ CLOSED_BETA_VERSION - 内测版本

# 特殊按键：

除正式版（即仅内测版或开发版或编辑器）：
+ 选曲界面（不是选章节界面！）按下`Y`：给自己+300糖果（代码位于`SelectUiManager.cs`）
+ 任意界面按下`End`：在控制台输出Nrk的组成（代码位于`NrkDebug.cs`）

仅开发版或编辑器：
+ 游玩界面按下`-`：直接结束当前游戏并把成绩设为All Perfect（代码位于`playViewManager.cs`）
+ 游玩界面按下`+`：直接结束当前游戏并把成绩设为全部Miss（代码位于`playViewManager.cs`）

仅编辑器
+ 选曲界面按住`Shift`再单机解锁曲子可以忽略解锁条件解锁（代码位于`SelectUiManager.cs`）

# 其他

新开的类、接口等以及新写的各类方法还有变量名等都要遵循C#的规范，之前amx写的代码没遵循（雾）

<br>

___

<br>

# 附录1：RFC 2119
见工程根目录下`rfc2119.pdf`。
