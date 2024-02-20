# XlsxToLua
一款将Excel数据转换成lua表的工具，支持分表和总表导出，支持多语言。

实时翻译功能已拆成独立功能，详情请转[XlsxTranslation](https://github.com/CLXiao-1029/XlsxTranslation)

# Release 最新版本
查看最新的[Release](https://github.com/CLXiao-1029/Xlsx2Lua/releases)版本

[Release](https://github.com/CLXiao-1029/Xlsx2Lua/releases)

## .Net 版本
>`>= v.7.0.201`

## 引用支持库
| 支持库 | 版本 | 地址 |
| - | - | - |
| `EPPlus` | 6.2.1 | https://epplussoftware.com/ |
| `.Net` | 7.0.201 | https://dotnet.microsoft.com/en-us/download/dotnet/7.0|

## 使用说明

##### 例子/Example/Demo
测试例子中详细说明演示了程序的用法，欢迎体验[TestExample](#https://github.com/CLXiao-1029/Xlsx2Lua/tree/master/TestExample)

##### 程序执行方式
运行时，根据传入的参数决定是否读配置项。

当传入参数以`--`开头时，将会读取程序对应的配置文件`Xlsx2Lua.cfg`。当配置文件存在时，会以配置文件中的数据为准。如果配置文件不存在，会根据传入的参数和默认值生成配置文件并执行程序。

传参说明请参照[参数说明](#参数说明)

配置文件数据说明参照[配置数据](#配置文件数据)

开启翻译统计时，会在配置表目录下生成一个`TranslateMain.xlsx`文件，该文件用来记录当前导表后的所有参与翻译的文本和结果。

##### 参数说明
当前已经支持12个参数的输入。

| 编号 | 类型 |默认值|是否必填| 释义 |
| --- | ---- | ---- |-------|----|
| 1 | `string` |  | 是 | 设置 Excel文件 目录 |
| 2 | `string` |  | 是 | 设置 导出文件 目录 |
| 3 | `string` | `lua` | 否 | 设置 导出文件 扩展名 |
| 4 | `int` | `6` | 否 | 设置读 Excel文件 有效起始行数，因为EPPlus从1开始计数 |
| 5 | `bool` | `true` | 否 | 设置 导出文件 是否连续index |
| 6 | `bool` | `false` | 否 | 是否生成总表 |
| 7 | `bool` | `false` | 否 | 是否拆分导出文件的注解 |
| 8 | `bool` | `true` | 否 | 是否开启翻译统计 |
| 9 | `bool` | `true` | 否 | 是否开启实时翻译 |
| 10 | `bool` | `true` | 否 | 是否显示导表耗时 |
| 11 | `bool` | `true` | 否 | 是否显示控制台日志 |
| 12 | `bool` | `false` | 否 | 是否输出日志文件 |
| 13 | `int` | `0` | 否 | 获取当前配置表的最新记录，默认不获取。当前仅支持Git记录获取 |

##### 配置文件数据
```json
{
    "ConfigPath": "..\\Config",//配置表路径。
    "OutputPath": "..\\LuaData",//输出文件路径。
    "OutputExt": ".lua",//输出文件扩展名
    "StartingRow": 6,//配置表有效起始行
    "OutputArray": false,//输出有序数据。lua中的表可分为哈希表(kv)和数组，有序数据就是数组表格式，以index作为table的key。
    "SummaryConfig": true,//是否生成总表，生成总表时，会将加载所有配置表数据合并成一个lua表文件。
    "CommentFile": true,// 是否生成注解文件。默认会将所有配置的数据类型，以表名作为类型，合并成一个文件。否则将会加到每张数据表的头部。总表模式下该字段不生效，默认生成单独的注解文件。
    "ShowTimelapse": true,//显示导表的时间流逝。可以留意每个配置表导出耗时。
    "Translation": true,// 是否开启翻译统计。不开启时有效数据行第四行不生效
    "RealtimeTrans": false,//是否开启实时翻译。该功能已经拆分成单独的工具
    "ShowLogLevel": 0,// 开启显示日志的等级。0：显示所有日志，1：只显示警告和报错，2：只显示报错。
    "OutputLogFile": true,// 是否输出日志文件？在导表结束后会在程序执行目录生成log文件，文件名：程序名+当前时间，示例：Xlsx2Lua-20231219151955.log
    "Translations": [
        "zh",
        "en"
    ], // 翻译文件的语种
    "TranslationNames": [
        "中文",
        "英语"
    ],// 翻译文件的语种中文释义
    "CommitRecordType": 0 // 获取提交记录类型，默认不获取。0：不获取，1：获取当前配置表路径下Git的最新提交记录，2：获取最新的SVN提交记录（暂未提供）
}
```

##### 工作簿命名规则
sheet名字以`|`作为分割符区分，拆分后的最后一个数据文本作为Lua表名存档。示例：`测试导表数据|test`，最终执行导表后会生成一个`test={}`

##### 有效数据行
第一行：当前列的数据描述
第二行：当前列的数据字段，建议使用英文或拼音
第三行：当前列的数据类型，**数据类型不区分大小写**。具体的数据类型支持请看下面的[支持的数据类型](#支持的数据类型)
第四行：当前列的翻译状态，如果值为`true`，则会将当前列的数据加入翻译统计。
第五行：预留
第六行：数据起始行

##### 支持的数据类型
常规类型：`long`、`int`、`float`、`double`、`string`、`bool`、`table`

复杂数据类型中有数组和字典（哈希表），每组数据以`|`作为分割符填充，同组数据以英文逗号`,`作为分隔符填充。

数组类型有两种，一种是简写`[]`，一种是全拼`array[]`。
简写模式下`[]`作为常规类型后缀。示例：`int[]`的数据：`1|3|5`，导出的数据示例`{1,3,5}`
全拼模式下`array[]`头尾拼接。示例：`array[int]`的数据：`1|3|5`，导出的数据示例`{1,3,5}`

字典类型（哈希表类型）同样有两种，简写`[]`，全拼`arrayTable[]`，不过该数据类型中`[]`作为作用域性质的存在。

举例1：`[id:int]`的数据`1|3|5`，导出的数据示例`{{id=1},{id=3},{id=5}`

举例2：`[id:int,icon:string]`的数据`1,icon_1|3,icon_3|5,icon_5`，导出的数据示例`{{id=1,icon="icon_1"},{id=3,icon="icon_5"},{id=5,icon="icon_5"}`

举例3：`arrayTable[id:int]`的数据`1|3|5`，导出的数据示例`{{id=1},{id=3},{id=5}`

举例4：`arrayTable[id:int,icon:string]`的数据`1,icon_1|3,icon_3|5,icon_5`，导出的数据示例`{{id=1,icon="icon_1"},{id=3,icon="icon_5"},{id=5,icon="icon_5"}`

特殊类型：`any`，原样输出数据，可以用来输出复杂的lua数据结构。在`any`类型下的数据，如果想导出`string`类型，需要使用引导包起来。示例：`any`的数据`"这是字符串"`，导出的数据示例`xx="这是字符串"`

**注意：数据类型不区分大小写**

## 翻译API

经过测试，在大量文本需要翻译时，开启实时翻译功能会导致程序假死。所以不建议开启实时翻译功能，在导表结束后，请运行单独的翻译工具进行翻译。

##### 文件读写
开启翻译统计时，会在配置表目录下生成一个`TranslateMain.xlsx`文件，该文件用来记录当前导表后的所有参与翻译的文本和结果。

~~开启实时翻译时，会将结果在最后也保存到`TranslateMain.xlsx`文件中~~。

实时翻译功能已经移交给[XlsxTranslation](https://github.com/CLXiao-1029/XlsxTranslation)

##### 翻译API
本软件采用的`百度翻译`[官网](https://fanyi.baidu.com/)，[翻译API](https://fanyi-api.baidu.com/api/trans/vip/translate) 
>`http://api.fanyi.baidu.com/api/trans/vip/translate?q=`content`&from=`zh`&to=`en`&appid=`APPID`&salt=`salt`&sign=`sign