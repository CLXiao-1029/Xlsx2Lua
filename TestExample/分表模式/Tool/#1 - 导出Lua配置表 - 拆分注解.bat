::������
::���� Excel Ŀ¼											..\Config
::���� �����ļ� Ŀ¼										..\LuaData
::���� �����ļ� ��չ��										.lua
::���ö� Excel ��Ч��ʼ����									6
::���� �����ļ� �Ƿ�����									false
::�Ƿ������ܱ�												true
::�Ƿ��ֵ����ļ���ע��									true
::��ʾʱ������												true
::�Ƿ�������												true
::�Ƿ�ʵʱ����(�����Ѿ����Ϊ��������)						false
::��ʾ��־�ĵȼ�											0:All, 1:Warning+Error, 2:Error
::�Ƿ������־�ļ�											true
::��ȡ�ύ��¼����											0:None,1:Git,2:Svn

@echo off

REM ��ɾ��Ŀ¼�������ļ�
del /s /q "..\LuaData\*"

REM ��ʼ����
".\Xlsx2Lua.exe" ..\Config ..\LuaData .lua 6 false false true true true false 0 true 0

pause