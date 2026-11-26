@echo off
title Chatting Server (ASP.NET Core Web API + SignalR)
echo ========================================================
echo   KHOI DONG CHATTING SERVER (PORT 5000)
echo ========================================================
cd /d "%~dp0"
dotnet run --project Chatting.Server/Chatting.Server.csproj
pause
