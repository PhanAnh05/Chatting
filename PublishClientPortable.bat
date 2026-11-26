@echo off
title Publish Chatting Client Portable
echo ========================================================
echo   DONG GOI CHATTING CLIENT CHO MAY TINH KHAC TRONG LAN
echo ========================================================
cd /d "%~dp0"
echo Dang xuat ban ban Client doc lap...
dotnet publish Chatting.Client/Chatting.Client.csproj -c Release -o publish/ChattingClient
echo.
echo ========================================================
echo   HOAN TAT!
echo   Thu muc phat hanh: %~dp0publish\ChattingClient
echo   Ban chi can copy thu muc nay sang may tinh khac va
echo   chay Chatting.Client.exe!
echo ========================================================
pause
