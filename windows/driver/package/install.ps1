.\clean.ps1
certutil -addstore -f -enterprise root f3flight-code-signing.cer
if ([System.IntPtr]::Size -eq 4) {
 .\devcon32.exe install x86\package\wudfvhidmini.inf root\wudfvhidmini
 } else {
 .\devcon64.exe install x64\package\wudfvhidmini.inf root\wudfvhidmini 
 }