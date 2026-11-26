@echo off
chcp 65001 >nul
title Xem Co So Du Lieu ChattingDB
cls
echo ===================================================================
echo             XEM CO SO DU LIEU CHATTING - ChattingDB
echo ===================================================================
echo.
echo [1] Danh sach Tai khoan nguoi dung - Bang Users:
echo -------------------------------------------------------------------
powershell -NoProfile -Command "Invoke-Sqlcmd -Query 'SELECT Id, Username, DisplayName, Email, IsEmailVerified, Status, CreatedAt FROM Users' -ServerInstance '.' -Database 'ChattingDB' | Format-Table -AutoSize"

echo.
echo [2] Danh sach Ban be - Bang Friendships:
echo -------------------------------------------------------------------
powershell -NoProfile -Command "Invoke-Sqlcmd -Query 'SELECT f.Id, u1.Username AS User1, u2.Username AS User2, f.CreatedAt FROM Friendships f JOIN Users u1 ON f.User1Id = u1.Id JOIN Users u2 ON f.User2Id = u2.Id' -ServerInstance '.' -Database 'ChattingDB' | Format-Table -AutoSize"

echo.
echo [3] Tin nhan gan nhat - Bang Messages:
echo -------------------------------------------------------------------
powershell -NoProfile -Command "Invoke-Sqlcmd -Query 'SELECT TOP 10 m.Id, u.Username AS NguoiGui, m.Content, m.Type, m.CreatedAt FROM Messages m JOIN Users u ON m.SenderId = u.Id ORDER BY m.CreatedAt DESC' -ServerInstance '.' -Database 'ChattingDB' | Format-Table -AutoSize"

echo.
echo ===================================================================
echo Meo: De xem truc quan bang giao dien do hoa:
echo 1. Mo SQL Server Management Studio 20 tren may
echo 2. Hoac mo Visual Studio - View - SQL Server Object Explorer
echo ===================================================================
echo.
pause
