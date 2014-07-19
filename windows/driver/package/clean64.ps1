.\devcon64.exe remove root\wudfvhidmini
$infs = (pnputil -e) | Out-String
$resultlist = New-Object System.Collections.Specialized.StringCollection
$regex = [regex] '(?m)^.*(oem\d+\.inf).*\r\n.*f3flight'
$match = $regex.Match($subject)
while ($match.Success) {
	$resultlist.Add($match.Groups[1].Value) | Out-Null
	$match = $match.NextMatch()
}
foreach ($inf in $infs)
{
	pnputil -d $inf
}
Remove-Item C:\Windows\System32\drivers\UMDF\WUDFvhidmini.dll