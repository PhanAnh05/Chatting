@echo off
title Chatting Client (Windows Forms LAN Messenger)
echo ========================================================
echo   KHOI DONG CHATTING CLIENT
echo ========================================================
cd /d "%~dp0"
dotnet run --project Chatting.Client/Chatting.Client.csproj
