set targetfile=%~1
set folderToUse=%~2
echo %targetfile%
xcopy /Y /S %targetfile% "F:\QDX-Test-Environment\resources\[Other Scripts]\mpchat\%folderToUse%"