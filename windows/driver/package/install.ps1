.\clean.ps1
certutil -addstore -f -enterprise root f3flight-code-signing.cer
$version = (cmd /c ver) | Out-String
if ([System.IntPtr]::Size -eq 4) {
    if ($version.Contains("6.1")) {
        .\devcon32.exe install x86\Win7Debug\package\wudfvhidmini.inf root\wudfvhidmini
    } elseif ($version.Contains("6.3")) {
        .\devcon32.exe install x86\Win8.1Debug\package\wudfvhidmini.inf root\wudfvhidmini
    }
 } else {
    if ($version.Contains("6.1")) {
        .\devcon64.exe install x64\Win7Debug\package\wudfvhidmini.inf root\wudfvhidmini
    } elseif ($version.Contains("6.3")) {
        .\devcon64.exe install x64\Win8.1Debug\package\wudfvhidmini.inf root\wudfvhidmini
    }
 }