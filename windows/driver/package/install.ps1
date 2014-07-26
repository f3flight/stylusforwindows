.\clean.ps1
$version = (cmd /c ver) | Out-String
if ([System.IntPtr]::Size -eq 4) {
    if ($version.Contains("6.1")) {
        .\devcon32.exe install x86\Win7Debug\package\spenvhid.inf root\spenvhid
    } elseif ($version.Contains("6.3")) {
        .\devcon32.exe install x86\Win8.1Debug\package\spenvhid.inf root\spenvhid
    }
 } else {
    if ($version.Contains("6.1")) {
        .\devcon64.exe install x64\Win7Debug\package\spenvhid.inf root\spenvhid
    } elseif ($version.Contains("6.3")) {
        .\devcon64.exe install x64\Win8.1Debug\package\spenvhid.inf root\spenvhid
    }
 }