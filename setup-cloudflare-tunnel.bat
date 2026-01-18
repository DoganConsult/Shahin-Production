@echo off
echo Installing Cloudflare Tunnel Service...
echo.
echo This requires Administrator privileges.
echo Please run this script as Administrator.
echo.
cloudflared.exe service install eyJhIjoiNjZiNTFhYzk2OTkxMWQ0MzY0ZjQ4M2Q4ODdhNjZjMGYiLCJ0IjoiODNiMzA4NjAtNTdjNS00OThlLTkxYjItMDM5M2E0MjJhZjk4IiwicyI6IlltVmxNelZoWm1FdE1USm1NQzAwWldJd0xUaGpNRFl0Tmprd01UZGhPRGc1T1RFMSJ9
echo.
echo Starting Cloudflare tunnel to expose localhost:5000...
cloudflared.exe tunnel run --url http://localhost:5000
pause
