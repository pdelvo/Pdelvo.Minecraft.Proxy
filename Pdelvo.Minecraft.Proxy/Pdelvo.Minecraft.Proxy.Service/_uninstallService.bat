set SUBDIR=%~dp0
cd %SUBDIR%

%WINDIR%\Microsoft.NET\Framework\v4.0.30319\installutil.exe /u "Pdelvo.Minecraft.Proxy.Service.exe"
pause