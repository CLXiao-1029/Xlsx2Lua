# XlsxToLua
一款将Excel数据转换成lua表的工具。
> .\Xlsx2Lua.exe ..\Config ..\Game\Scripts\Data .lua 6 false true true true true false 0 true

## .Net 版本
>`>= v.7.0.201`

## 引用支持库
| 支持库 | 版本 | 地址 |
| - | - | - |
| `EPPlus` | 6.2.1 | https://epplussoftware.com/ |
| `.Net` | 7.0.201 | https://dotnet.microsoft.com/en-us/download/dotnet/7.0|

## 使用说明

##### 程序执行方式
运行时，根据传入的参数决定是否读配置项。
当传入参数以`--`开头时，将会读取程序对应的配置文件`XlsxToLuaCfg.cfg`。当配置文件存在时，会以配置文件中的数据为准。如果配置文件不存在，会根据传入的参数和默认值生成配置文件并执行程序。
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

##### 配置文件数据
```json
{
    "config path": "Test/Config", //配置表路径。
    "output path": "Test/Lua", //输出文件路径。
    "output ext": "lua",    //输出文件扩展名
    "starting row": 6,  //配置表有效起始行
    "output array": true,   //输出有序数据。lua中的表可分为哈希表(kv)和数组，有序数据就是数组表格式，以index作为table的key。
    "summary config": false,    //是否生成总表，生成总表时，会将加载所有配置表数据合并成一个lua表文件。
    "comment file": true,   // 是否生成注解文件。默认会将所有配置的数据类型，以表名作为类型，合并成一个文件。否则将会加到每张数据表的头部。总表模式下该字段不生效，默认生成单独的注解文件。
    "show time lapse": true,    //显示导表的时间流逝。可以留意每个配置表导出耗时。
    "translation": true,    // 是否开启翻译统计。不开启时有效数据行第四行不生效
    "real time translation": false, //是否开启实时翻译。不推荐，因为每个需要翻译的文本都要去请求翻译API，过程会卡顿。
    "show log level": 2,    // 开启显示日志的等级。0：显示所有日志，1：只显示警告和报错，2：只显示报错。
    "output log file": false    // 是否输出日志文件？在导表结束后会在程序执行目录生成log文件，文件名：log+当前时间，示例：log19961029102900.log
}
```

##### 工作簿命名规则
sheet名字以`|`作为分割符区分，拆分后的最后一个数据文本作为Lua表名存档。示例：`测试导表数据|test`，最终执行导表后会生成一个`test={}`

##### 有效数据行
第一行：当前列的数据描述
第二行：当前列的数据字段，建议使用英文或拼音
第三行：当前列的数据类型，具体的数据类型支持请看下面的[支持的数据类型](#支持的数据类型)
第四行：当前列的翻译状态，如果值为`true`，则会将当前列的数据加入翻译统计。
第五行：预留
第六行：数据起始行

##### 支持的数据类型
常规类型：`long`、`int`、`float`、`double`、`string`、`bool`、`table`
数组类型：`[]`作为常规类型后缀，数组数据以`|`作为分割符填充。示例：`int[]`的数据：`1|3|5`，导出的数据示例`{1,3,5}`
KV类型：`[k:v]`中k作为键名，v作为数据类型，示例:`[id:int]`的数据：`1|3|5`，导出的数据示例`{{id=1},{id=3},{id=5}`
复杂的KV类型：`[k1:v1,k2:v2]`中k作为键名，v作为数据类型，一组kv数据之间以`,`作为分割符填充，示例：`[id:int,name:string]`的数据：`1,世界|3,你好|5,Hello World`，导出的数据示例：`{{id=1,name="世界"},{id=1,name="你好"},{id=1,name="Hello World"}}`
<font color=Crimson>注意：KV类型中不能存在数据类型。因为数组类型的KV类型数据区分都是用`|`分割符来拆分数据的。</font>
特殊类型：`any`，原样输出数据，可以用来输出复杂的lua数据结构。在`any`类型下的数据，如果想导出`string`类型，需要使用引导包起来。示例：`any`的数据`"这是字符串"`，导出的数据示例`xx="这是字符串"`

## 翻译API

经过测试，在大量文本需要翻译时，开启实时翻译功能会导致程序假死。所以不建议开启实时翻译功能，在导表结束后，请运行单独的翻译工具进行翻译。

##### 文件读写
开启翻译统计时，会在配置表目录下生成一个`TranslateMain.xlsx`文件，该文件用来记录当前导表后的所有参与翻译的文本和结果。
开启实时翻译时，会将结果在最后也保存到`TranslateMain.xlsx`文件中。

##### 翻译API
本软件采用的`百度翻译`[官网](https://fanyi.baidu.com/)，[翻译API](https://fanyi-api.baidu.com/api/trans/vip/translate) 
>`http://api.fanyi.baidu.com/api/trans/vip/translate?q=`content`&from=`zh`&to=`en`&appid=`APPID`&salt=`salt`&sign=`sign
