// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InstallScriptBlocks.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Core
{
    internal static class InstallScriptBlocks
    {
        public static string InstallWeb
        {
            get
            {
                return @"
{
param(
    [string] $WebRootPath,
	[string] $Domain,
	[string] $CertPath,
	[SecureString] $CertPassword,
    [string] $AppPoolStartMode,
    [string] $AutoStartProviderName,
    [string] $AutoStartProviderType,
	[switch] $EnableSNI,
	[switch] $AddHostHeaders
	)

try
{
	Write-Output ""Beginning Deployment of a Website:""
	Write-Output ""    WebRootPath: $WebRootPath""
	Write-Output ""         Domain: $Domain""
	Write-Output ""       CertPath: $CertPath""
	Write-Output ""   CertPassword: $CertPassword""
	Write-Output ''
	
	# Add IIS and suppporting features
	Add-WindowsFeature -IncludeManagementTools -Name Web-Default-Doc, Web-Dir-Browsing, Web-Http-Errors, Web-Static-Content, Web-Http-Redirect, Web-Http-Logging, Web-Custom-Logging, Web-Log-Libraries, Web-Request-Monitor, Web-Http-Tracing, Web-Basic-Auth, Web-Digest-Auth, Web-Windows-Auth, Web-Net-Ext, Web-Net-Ext45, Web-Asp-Net, Web-Asp-Net45, Web-ISAPI-Ext, Web-ISAPI-Filter, Web-Scripting-Tools, NET-Framework-45-ASPNET
	# Set IIS Service to restart on failure and reboot on 3rd failure
	$services = Get-WMIObject win32_service | Where-Object {$_.name -imatch ""W3SVC"" -and $_.startmode -eq ""Auto""}; foreach ($service in $services){sc.exe failure $service.name reset= 86400 actions= restart/5000/restart/5000/reboot/5000}

	Import-Module WebAdministration

	Write-Output ""Using site path for IIS at $WebRootPath""

	Write-Output ""Removing default site if present to avoid any potential conflicts""
	if (Test-Path 'IIS:\Sites\Default Web Site'){ Remove-Item 'IIS:\Sites\Default Web Site' -Force -Recurse}

	if (-not (Test-Path $WebRootPath))
	{
		throw ""Site missing at $WebRootPath""
	}
	
	if (-not (Test-Path $CertPath))
	{
		throw ""Cert missing at $CertPath""
	}
	
	$certStoreLocation = 'cert:\LocalMachine\My'
	Write-Output ""Installing cert at $certStoreLocation""
	$certResult = Import-PfxCertificate -FilePath $CertPath -Password $CertPassword -CertStoreLocation $certStoreLocation -Exportable
	rm $CertPath -Force
	Write-Output ""Cert installed with Thumbprint: $($certResult.Thumbprint) - Deleted file""

	$appPoolName = ""$($Domain)_AppPool""
	Write-Output ""Creating Application Pool: $appPoolName""
	New-Item ""IIS:\AppPools\$appPoolName"" | Out-Null
	Set-ItemProperty ""IIS:\AppPools\$appPoolName"" managedRuntimeVersion v4.0 | Out-Null
	Set-ItemProperty ""IIS:\AppPools\$appPoolName"" startMode $AppPoolStartMode | Out-Null
    Set-ItemProperty ""IIS:\AppPools\$appPoolName"" autoStart true | Out-Null

	$sslFlags = 0
	if ($EnableSNI)
	{
		$sslFlags = 1
		Write-Output ""SNI Enabled (can use multiple host names on same machine""
	}
	else
	{
		Write-Output ""SNI is NOT Enabled (can NOT use multiple host names on same machine""
	}

    $sitePath = ""IIS:\Sites\$Domain""
	if ($AddHostHeaders)
	{
		Write-Output ""Creating site at $WebRootPath for domain $Domain WITH hostHeaders""
		New-Item -Path $sitePath -bindings @{protocol=""http"";bindingInformation="":80:$Domain""} -physicalPath $WebRootPath -applicationPool $appPoolName
		New-WebBinding -name $Domain -Protocol https -HostHeader ""$Domain"" -Port 443 -SslFlags $sslFlags
	}
	else
	{
		Write-Output ""Creating site at $WebRootPath for domain $Domain WITH OUT hostHeaders""
		New-Item -Path $sitePath -bindings @{protocol=""http"";bindingInformation="":80:""} -physicalPath $WebRootPath -applicationPool $appPoolName
		New-WebBinding -name $Domain -Protocol https -Port 443 -SslFlags $sslFlags
	}

	$cert = Get-Item $(Join-Path $certStoreLocation $certResult.Thumbprint)
	New-Item -Path ""IIS:\SslBindings\!443!$Domain"" -Value $cert -SSLFlags $sslFlags
	
	Write-Output ""Performing IIS RESET to make sure everything is up and running correctly""
	iisreset
	
	$site = Get-Item $sitePath -ErrorAction SilentlyContinue
	$newSitePath = $site.physicalPath
	
	if ($newSitePath -ne $WebRootPath)
	{
		throw ""Failed to correctly deploy site to $WebRootPath, instead it got configured to $newSitePath""
	}

    if ((-not [String]::IsNullOrEmpty($AutoStartProviderName)) -and (-not ([String]::IsNullOrEmpty($AutoStartProviderType))))
    {
        Set-ItemProperty $sitePath serverAutoStart true
        Set-ItemProperty $sitePath 'applicationDefaults.serviceAutoStartEnabled' true
        Set-ItemProperty $sitePath 'applicationDefaults.serviceAutoStartProvider' $AutoStartProviderName

        [xml]$appHost = New-Object xml
        $appHost.psbase.PreserveWhitespace = $true

        $configPath = ""C:\Windows\System32\inetsrv\config\applicationHost.config""
        $appHost.Load($configPath)

        $appHost.configuration.'system.applicationHost'
        $autoStartProviders = $appHost.configuration.'system.applicationHost'.serviceAutoStartProviders
        if ($autoStartProviders -eq $null)
        {
	        $autoStartProviders = $appHost.CreateElement(""serviceAutoStartProviders"")
	        $appHost.configuration.'system.applicationHost'.AppendChild($autoStartProviders)
        }

        $existingProvider = $autoStartProviders.add | ?{$_.name -eq $AutoStartProviderName}
        if ($existingProvider -eq $null)
        {
	        $provider = $appHost.CreateElement(""add"")
	        $provider.SetAttribute(""name"", $AutoStartProviderName)
	        $provider.SetAttribute(""type"", $AutoStartProviderType)
	        $autoStartProviders.AppendChild($provider)
        }

    	$appHost.Save($configPath)
    }

	$deploymentInfo = join-path $WebRootPath ""deploymentInfo.xml""
	Write-Output ""Writing deployment info to $deploymentInfo""

	$xmlWriter = New-Object System.XML.XmlTextWriter($deploymentInfo,$Null)
	# choose a pretty formatting:
	$xmlWriter.Formatting = 'Indented'
	$xmlWriter.Indentation = 1
	$xmlWriter.IndentChar = ""	""
	$xmlWriter.WriteStartDocument()
	$xmlWriter.WriteStartElement('Publish')
	$xmlWriter.WriteElementString('SitePath',$WebRootPath)
	$xmlWriter.WriteElementString('Domain',$Domain)
	$xmlWriter.WriteElementString('AppPool',$appPoolName)
	$xmlWriter.WriteElementString('Username',[Environment]::Username)
	$xmlWriter.WriteElementString('UserDomainName',[Environment]::UserDomainName)
	$xmlWriter.WriteElementString('MachineName',[Environment]::MachineName)
	$xmlWriter.WriteElementString('PublishDate',[System.DateTime]::Now.ToString('yyyyMMdd-HHmm'))
	$xmlWriter.WriteEndElement()
	$xmlWriter.WriteEndDocument()
	$xmlWriter.Flush()
	$xmlWriter.Close()

	Write-Output ""Finished successfully""
}
catch
{
    Write-Error """"
    Write-Error ""ERROR DURING EXECUTION @ $([DateTime]::Now.ToString('yyyyMMdd-HHmm'))""
    Write-Error """"
    Write-Error ""  BEGIN Error Details:""
    Write-Error """"
    Write-Error ""   $_""
    Write-Error ""   IN FILE: $($_.InvocationInfo.ScriptName)""
    Write-Error ""   AT LINE: $($_.InvocationInfo.ScriptLineNumber) OFFSET: $($_.InvocationInfo.OffsetInLine)""
    Write-Error """"
    Write-Error ""  END   Error Details:""
    Write-Error """"
    Write-Error ""ERROR DURING EXECUTION""
    Write-Error """"
    
    throw
}
}
";
            }
        }

        public static string UnzipFile
        {
            get
            {
                return @"
{
param(
	[string] $FilePath,
	[string] $TargetDirectoryPath
	)

try
{
	$shell_app=new-object -com shell.application
	$zip_file = $shell_app.namespace($FilePath)
	$destination = $shell_app.namespace($TargetDirectoryPath)
	$destination.Copyhere($zip_file.items())
	Write-Output ""Finished successfully""
}
catch
{
    Write-Error """"
    Write-Error ""ERROR DURING EXECUTION @ $([DateTime]::Now.ToString('yyyyMMdd-HHmm'))""
    Write-Error """"
    Write-Error ""  BEGIN Error Details:""
    Write-Error """"
    Write-Error ""   $_""
    Write-Error ""   IN FILE: $($_.InvocationInfo.ScriptName)""
    Write-Error ""   AT LINE: $($_.InvocationInfo.ScriptLineNumber) OFFSET: $($_.InvocationInfo.OffsetInLine)""
    Write-Error """"
    Write-Error ""  END   Error Details:""
    Write-Error """"
    Write-Error ""ERROR DURING EXECUTION""
    Write-Error """"
    
    throw
}
}
";
            }
        }

        public static string CreateDirectoryWithFullControl
        {
            get
            {
                return @"
{
param(
	[string] $DirectoryPath,
	[string] $UserToGiveFullControlTo
	)

try
{
    md $DirectoryPath
    $acl = Get-Acl $DirectoryPath
    $permission = $UserToGiveFullControlTo, 'FullControl', 'Allow'
    $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
    $acl.SetAccessRule($accessRule)
    $acl | Set-Acl $DirectoryPath
}
catch
{
    Write-Error """"
    Write-Error ""ERROR DURING EXECUTION @ $([DateTime]::Now.ToString('yyyyMMdd-HHmm'))""
    Write-Error """"
    Write-Error ""  BEGIN Error Details:""
    Write-Error """"
    Write-Error ""   $_""
    Write-Error ""   IN FILE: $($_.InvocationInfo.ScriptName)""
    Write-Error ""   AT LINE: $($_.InvocationInfo.ScriptLineNumber) OFFSET: $($_.InvocationInfo.OffsetInLine)""
    Write-Error """"
    Write-Error ""  END   Error Details:""
    Write-Error """"
    Write-Error ""ERROR DURING EXECUTION""
    Write-Error """"
    
    throw
}
}
";
            }
        }

        public static string UpdateItsConfigPrecedence
        {
            get
            {
                return @"
{
    param(
	    [string] $FilePath,
	    [string] $Environment
	    )

    try
    {
	    $appSettingsNodeName = 'appSettings'
	    [xml] $c = Get-Content $FilePath
	    # should only be one so this 'hack' works
	    $appSettingsNode = $c.configuration.GetElementsByTagName($appSettingsNodeName) | %{$_}
	    if ($appSettingsNode -eq $null)
	    {
		    $appSettingsNode = $c.CreateElement($appSettingsNodeName)
		    $c.configuration.AppendChild($appSettingsNode)
	    }

	    $itsConfigPrecedenceKey = 'Its.Configuration.Settings.Precedence'
	    $n = $appSettingsNode.GetElementsByTagName('add') | ?{$_.key -eq $itsConfigPrecedenceKey}
	
	    if ($n -eq $null)
	    {
		    $n = $c.CreateElement('add') 
		    $appSettingsNode.AppendChild($n)
		    $n.SetAttribute('key',  $itsConfigPrecedenceKey)
		    $n.SetAttribute('value',  $Environment)
	    }
	    else
	    {
		    $n.value = $Environment
	    }

	    $c.Save($FilePath)
    }
    catch
    {
        Write-Error """"
        Write-Error ""ERROR DURING EXECUTION @ $([DateTime]::Now.ToString('yyyyMMdd-HHmm'))""
        Write-Error """"
        Write-Error ""  BEGIN Error Details:""
        Write-Error """"
        Write-Error ""   $_""
        Write-Error ""   IN FILE: $($_.InvocationInfo.ScriptName)""
        Write-Error ""   AT LINE: $($_.InvocationInfo.ScriptLineNumber) OFFSET: $($_.InvocationInfo.OffsetInLine)""
        Write-Error """"
        Write-Error ""  END   Error Details:""
        Write-Error """"
        Write-Error ""ERROR DURING EXECUTION""
        Write-Error """"
    
        throw
    }
}
";
            }
        }
    }
}