# Path to SoundVolumeView
$svvPath = ".\3rdParty\svcl.exe"

# Export devices to CSV
$csvPath = "$env:TEMP\svv_devices.csv"
& $svvPath /scomma $csvPath

# Read CSV
$devices = Import-Csv $csvPath

# Find current default output (Default=Yes for Console role)
$defaultDevice = $devices | Where-Object { $_.Default -eq "Render" } | Select-Object -First 1 -ExpandProperty Name

# Output the default device name
Write-Output $defaultDevice

# Clean up
Remove-Item $csvPath