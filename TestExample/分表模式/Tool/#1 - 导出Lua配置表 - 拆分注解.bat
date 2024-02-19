::程序名
::设置 Excel 目录											..\Config
::设置 导出文件 目录										..\LuaData
::设置 导出文件 扩展名										.lua
::设置读 Excel 有效起始行数									6
::设置 导出文件 是否有序									false
::是否生成总表												true
::是否拆分导出文件的注解									true
::显示时间流逝												true
::是否开启翻译												true
::是否实时翻译(功能已经拆分为单独程序)						false
::显示日志的等级											0:All, 1:Warning+Error, 2:Error
::是否输出日志文件											true
::获取提交记录类型											0:None,1:Git,2:Svn

@echo off

REM 先删除目录下所有文件
del /s /q "..\LuaData\*"

REM 开始导表
".\Xlsx2Lua.exe" ..\Config ..\LuaData .lua 6 false false true true true false 0 true 0

pause