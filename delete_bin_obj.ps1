Get-ChildItem .\ -include bin,obj -Recurse | foreach ($_) { remove-item $_.fullname -Force -Recurse }
Write-Host "Done"
Read-Host "Press Enter to exit"