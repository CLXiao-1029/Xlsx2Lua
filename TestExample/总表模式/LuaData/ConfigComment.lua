---@class Cfg_Items
---@field id number 编号
---@field name string 名字
---@field desc string 描述
---@field type number 类型
---@field subtype number 子类型
---@field rare number 稀有度
---@field icon string 图标
---@field randomvalue number[] 随机概率

---@class Cfg_TempConfig
---@field id number 编号
---@field name string 名字
---@field desc string 描述
---@field info string 不需要翻译的文本
---@field Int number 整数
---@field Long number 长整数
---@field Float number 浮点数
---@field String string 字符串
---@field Bool boolean 布尔值
---@field Table table Lua表
---@field IntArr1 number[] 整数数组1
---@field IntArr2 number[] 整数数组2
---@field ArrayLong1 number[] 长整数数组1
---@field ArrayLong2 number[] 长整数数组2
---@field FloatArr1 number[] 浮点数数组1
---@field FloatArr2 number[] 浮点数数组2
---@field StringArr1 string[] 字符串数组1
---@field StringArr2 string[] 字符串数组2
---@field BoolArr1 boolean[] 布尔值数组1
---@field BoolArr2 boolean[] 布尔值数组2
---@field hashTable1 table<number,number> 字典类型1 {tid,value}
---@field hashTable2 table<number,number> 字典类型2 {tid,value}
---@field anyValue any 任意数值，数值不会进行转化，填入的是数字导出来是数字，字符串需要加引号

---@class Cfg_ConfigAll
---@field Items Cfg_Items[]
---@field TempConfig Cfg_TempConfig[]


