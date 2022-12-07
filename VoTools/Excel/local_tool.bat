
for %%d in (%~dp0..) do set ParentDirectory=%%~fd
echo %ParentDirectory%

set EXPORT_CODE_PATH= %ParentDirectory%/Assets/Neon/Scripts/VOs
set EXPORT_DATA_PATH= %ParentDirectory%/Assets/Neon/Datas/Configs
set GEN_CLIENT=%ParentDirectory%/Tools/luban_client_server\Luban.ClientServer.exe

echo "====code path===="
echo %EXPORT_CODE_PATH%
echo "====data path====""
echo %EXPORT_DATA_PATH%

echo "====Start====""
%GEN_CLIENT% -j cfg --^
 -d  %ParentDirectory%/Excel/Configs/__root__.xml ^
 --input_data_dir  %ParentDirectory%/Excel/Datas ^
 --output_code_dir %EXPORT_CODE_PATH% ^
 --output_data_dir %EXPORT_DATA_PATH% ^
 --gen_types code_cs_unity_json,data_json ^
 -s all  ^

pause
pause