.\devcon64.exe remove root\wudfvhidmini
$pnputil = (pnputil -e ) | Out-String
$inflist = New-Object System.Collections.Specialized.StringCollection
$regex = [regex] '(?m)^.*(oem\d+\.inf).*\r\n.*f3flight'
$match = $regex.Match($pnputil)
while ($match.Success) {
	$inflist.Add($match.Groups[1].Value) | Out-Null
	$match = $match.NextMatch()
}

foreach ($inf in $inflist)
{
    echo $inf
	pnputil -d $inf
}

if (Test-Path "C:\Windows\System32\drivers\UMDF\WUDFvhidmini.dll") {
    Remove-Item C:\Windows\System32\drivers\UMDF\WUDFvhidmini.dll
}