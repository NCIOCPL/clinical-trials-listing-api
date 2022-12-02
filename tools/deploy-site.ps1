<#
	.SYNOPSIS
	Creates the file system folder structure along with IIS website and application for the given API.
	.DESCRIPTION
	Creates the file system folder structure for an API.
	Creates the IIS website for the API.
	Creates the application for the specified version of the API.
	.PARAMETER site_name
	The site name for the API. Required.
	.PARAMETER app_name
	The application name for the API. Required.
	.PARAMETER app_version
	The version of the API for the application. Required.
	.PARAMETER site_hostname
	The hostname of te API website. Required.
	.PARAMETER code_folder
	The folder from which to copy the latest code. Required.
#>

Param(
	[Parameter(mandatory=$true)]
	[string]$site_name = "",
	
	[Parameter(mandatory=$true)]
	[string]$app_name = "",
	
	[Parameter(mandatory=$true)]
	[string]$app_version = "", 
	
	[Parameter(mandatory=$true)]
	[string]$site_hostname = "",
	
	[Parameter(mandatory=$true)]
	[string]$code_folder = ""
);

## Named constants
New-Variable -Name "CONTENT_ROOT" -Value "D:\Content"			-Option Constant;
New-Variable -Name "SITES_ROOT"   -Value "D:\Content\APISites"	-Option Constant;
New-Variable -Name "BACKUP_ROOT"  -Value "D:\backups"			-Option Constant;

function Main() {
	## The WebAdministration module requires elevated privileges.
	$isAdmin = Is-Admin;
	if( $isAdmin ) {
		Write-Host -foregroundcolor 'green' "Starting...";
		Import-Module WebAdministration;
		CreateSite;
	} else {
		Write-Host -foregroundcolor 'red' "This script must be run from an account with elevated privileges.";
	}
}

function Is-Admin {
 $id = New-Object Security.Principal.WindowsPrincipal $([Security.Principal.WindowsIdentity]::GetCurrent());
 $id.IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator);
}

function CreateSite() {
	## Check parameters
	if ($site_name -eq "")
	{
		Write-Host -foregroundcolor 'red' "ERROR: Site Name must not be null.";
		exit;
	}

	if ($app_name -eq "")
	{
		Write-Host -foregroundcolor 'red' "ERROR: App Name must not be null.";
		exit;
	}

	if ($app_version -eq "")
	{
		Write-Host -foregroundcolor 'red' "ERROR: App Version must not be null.";
		exit;
	}

	if ($site_hostname -eq "")
	{
		Write-Host -foregroundcolor 'red' "ERROR: Site Host Name must not be null.";
		exit;
	}

	if ($code_folder -eq "")
	{
		Write-Host -foregroundcolor 'red' "ERROR: Code Folder must not be null.";
		exit;
	}

	## Set up folder names for file system folder structure
	$sitePath = $SITES_ROOT + "\" + $site_name;
	$sitePhysicalPath = $sitePath + "\root";
	$appPath = $sitePath + "\" + $app_name;
	$versionAppPath = $sitePath + "\" + $app_version;

	## Make site directories
	## Should check non-existence first
	foreach ( $folder in (
			$CONTENT_ROOT,
			$SITES_ROOT,
			$sitePath,
			$sitePhysicalPath,
			$appPath
			)
		)
	{
		if (Test-Path $folder)
		{
			Write-Host "WARNING: The path ${folder} already exists. Skipping creation.";
		}
		else
		{
			Write-Host "INFO: Creating folder ${folder}.";
			New-Item -ItemType directory -Path $folder;     
		}
	}

	## Stop web site so updated files can be copied.
	$webSiteExists = (Get-WebSite -Name $site_name) -ne $null
	if( $webSiteExists -eq $true ){
		Stop-Website -Name $site_name
	}
	else {
		write-host "*********** False *************";
	}

	
	## Make version directory and copy files
	if (Test-Path $versionAppPath)
	{
		Write-Host "WARNING: The ${app_version} folder path ${versionAppPath} already exists. Skipping creation.";

		if(!(Test-Path $code_folder))
		{
			Write-Host -foregroundcolor 'red' "ERROR: Code folder ${code_folder} does not exist.";
			Exit;
		}
		else
		{
		    # Create a backup before overwriting existing files.
			BackupFiles $site_name $versionAppPath;
		
			Write-Host "INFO: Copying files from ${code_folder} to ${versionAppPath}.";
			
			## Clear contents of version folder (in case there are extraneous files from an old build)
			Get-ChildItem $versionAppPath -Recurse | Foreach-Object {Remove-Item -Recurse -path $_.FullName }

			## Copy files
			Get-ChildItem $code_folder | Copy-Item -Destination $versionAppPath -force -Recurse
		}
	}
	else
	{
		Write-Host "INFO: Creating folder ${versionAppPath}.";
		New-Item -ItemType directory -Path $versionAppPath;
		
		if (!(Test-Path($code_folder)))
		{
			Write-Host -foregroundcolor 'red' "ERROR: Code folder ${code_folder} does not exist.";
			Exit;
		}
		else
		{
			Write-Host "INFO: Copying files from ${code_folder} to ${versionAppPath}.";
			
			## Clear contents of version folder (in case there are extraneous files from an old build)
			Get-ChildItem $versionAppPath -Recurse | Foreach-Object {Remove-Item -Recurse -path $_.FullName }

			## Copy files
			Get-ChildItem $code_folder | Copy-Item -Destination $versionAppPath -force -Recurse;
		}
	}

	## Restart web site.
	if( $webSiteExists -eq $true ){
		Start-Website -Name $site_name
	}

	###### Setup Web Site ######

	## Set up app pool/website variables
	$siteAppPool = $site_name;
	$siteIISAppPool = "IIS:\AppPools\" + $siteAppPool;
	$siteIISPath = "IIS:\Sites\" + $site_name;
	$appAppPool = $site_name + "-" + $app_version;
	$appIISAppPool = "IIS:\AppPools\" + $appAppPool;
	$appIISPath = $siteIISPath + "\" + $app_name + "\" + $app_version;
	$appVD = $app_name + "/" + $app_version;

	## Create application pool for website
	if((Test-Path $siteIISAppPool) -eq 0)
	{
		New-WebAppPool -Name $siteAppPool;
		Set-ItemProperty -Path $siteIISAppPool -Name managedRuntimeVersion -Value "";
	}
	else
	{
		Write-Host "WARNING: Application pool ${siteAppPool} already exists. Skipping creation.";
	}

	## Create application pool for version application
	if((Test-Path $appIISAppPool) -eq 0)
	{
		New-WebAppPool -Name $appAppPool;
		Set-ItemProperty -Path $appIISAppPool -Name managedRuntimeVersion -Value "";
	}
	else
	{
		Write-Host "WARNING: Application pool ${appAppPool} already exists. Skipping creation.";
	}

	## Create site and application
	if((Get-Item $siteIISPath -ErrorAction SilentlyContinue) -eq $null)
	{
		## If website doesn't already exist, create it
		New-Website -Name $site_name -Port 443 -HostHeader $site_hostname -PhysicalPath $sitePhysicalPath -ApplicationPool $siteAppPool -Ssl;
		New-WebBinding -Name $site_name -Protocol "http" -Port 80 -HostHeader $site_hostname;
	}
	else
	{
		Write-Host "WARNING: Website ${site_name} already exists. Skipping creation.";
	}
	
	## If web application for version doesn't already exist, create it
	if((Get-WebVirtualDirectory -Site $site_name -Name $app_name) -eq $null)
	{
		New-WebVirtualDirectory -Site $site_name -Name $app_name -PhysicalPath $appPath;
	}
	else
	{
		Write-Host "WARNING: Virtual directory ${app_name} already exists. Skipping creation.";
	}
	
	if((Get-WebApplication -Site $site_name -Name $appVD) -eq $null)
	{
		New-WebApplication -Name $appVD -Site $site_name -PhysicalPath $versionAppPath -ApplicationPool $appAppPool;
	}
	else
	{
		Write-Host "WARNING: Application ${appVD} already exists. Skipping creation.";
	}
}

function BackupFiles($siteName, $sitePath) {
	$datestring = Get-Date -Format "yyyy-MM-dd-HH-mm_s";
	$backupLocation = $BACKUP_ROOT + "\" + $siteName + "\" + $datestring;
	Write-Host "INFO: Backing up to" $backupLocation;

	New-Item -ItemType directory -Path $backupLocation
	Get-ChildItem $sitePath | Copy-Item -Destination $backupLocation -force;
}

Main;
Read-Host -Prompt "Press Enter to exit."
