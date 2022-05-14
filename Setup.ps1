<#
	This PowerShell script is a collection of tasks for setting up the Azure resources.
	Ensure that you have updated the values of the variables appropriately before running.

	Prerequisites:
	- Azure Cosmos DB free-tier does not use the free-tier Azure CosmosDB.
	- .NET SDK 6.0 is installed on the system that this script is used.
	- Register the Azure event grid resource provider to the subscription if you haven't done yet.
#>

$subscriptionId = "<Your subscription ID>"
$resourceGroupName = "rg-aed-demo-001"
$location = "australiaeast"
$skuName = "Standard_LRS" 
$storageName = "staeddemo001"
$pdfContainerName = "pdf"
$htmlContainerName = "html"
$functionAppName = "func-aed-demo-001"
$functionAppSourceFolder = "<Function app source folder>"
$cosmosDbAccountName = "cosmos-aed-demo-001"
$databaseName = "cosmosdb-aed-demo-001"
$containerName = "cosmosct-aed-demo-001"
$partitionKeyPath = "/id"
$throughput = 400
$topicName = "evt-aed-demo-001"
$appServicePlan = "plan-aed-demo-001"
$webAppName = "app-ade-demo-001"
$webAppSourceFolder = "<Webdashboard source folder>"

# 1. Connect to Azure.

Connect-AzAccount -SubscriptionId $subscriptionId

# 2. Create a new resource group.

New-AzResourceGroup -Name $resourceGroupName -Location $location

# 3. Create a new storage account and containers.

$storageAccount = New-AzStorageAccount `
	-ResourceGroupName $resourceGroupName `
	-Name $storageName `
	-SkuName $skuName `
	-Location $location

New-AzStorageContainer -Name $pdfContainerName -Context $storageAccount.Context -Permission Off
New-AzStorageContainer -Name $htmlContainerName -Context $storageAccount.Context -Permission Off

$blobStorageAccessKey= (Get-AzStorageAccountKey `
	-ResourceGroupName $resourceGroupName `
	-Name $storageName).Value[0]

$blobStorageConnectionString = "DefaultEndpointsProtocol=https;" +
"AccountName=$($storageName);" +
"AccountKey=$($blobStorageAccessKey);" +
"EndpointSuffix=core.windows.net"

# 4. Create a Cosmos DB account, database and container.

New-AzCosmosDBAccount -ResourceGroupName $resourceGroupName `
	-Name $cosmosDbAccountName `
	-Location $location `
	-ApiKind "SQL" `
	-DefaultConsistencyLevel "Session"
	-EnableFreeTier $true

New-AzCosmosDBSqlDatabase `
	-ResourceGroupName $resourceGroupName `
	-AccountName $cosmosDbAccountName `
	-Name $databaseName

New-AzCosmosDBSqlContainer `
	-ResourceGroupName $resourceGroupName `
	-AccountName $cosmosDbAccountName `
	-DatabaseName $databaseName `
	-Name $containerName `
	-PartitionKeyKind Hash `
	-PartitionKeyPath $partitionKeyPath `
	-Throughput $throughput

$cosmosDbConnectionString = (Get-AzCosmosDBAccountKey `
	-ResourceGroupName $resourceGroupName `
	-Name $cosmosDbAccountName `
	-Type "ConnectionStrings")["Primary SQL Connection String"]

# 5. Create a function app.

New-AzFunctionApp `
	-Name $functionAppName `
	-ResourceGroupName $resourceGroupName `
	-StorageAccount $storageName `
	-Runtime dotnet `
	-FunctionsVersion 4 `
	-RuntimeVersion 6 `
	-OSType Windows `
	-Location $location

$functionUrl = "https://$($functionAppName).azurewebsites.net"

# 6. Compile and deploy the zipped Azure functions.

$publishFolder = Join-Path $functionAppSourceFolder publish
dotnet publish "$(Join-Path $functionAppSourceFolder FunctionApp.csproj)" -f net6.0 -o $publishFolder -c Release

$zippedPublishFile = Join-Path $functionAppSourceFolder publish.zip
Compress-Archive -Path (Join-Path $publishFolder "*") -DestinationPath $zippedPublishFile

Publish-AzWebapp -ResourceGroupName $resourceGroupName -Name $functionAppName -ArchivePath $zippedPublishFile

# 7. Create an Azure event topic.

$topicEndpoint = (New-AzEventGridTopic -ResourceGroupName $resourceGroupName -Name $topicName -Location $location).EndPoint 
$topicAccessKey = (Get-AzEventGridTopicKey -ResourceGroupName $resourceGroupName -Name $topicName).Key1

$eventSubscriptionName = "event-grid-handler-to-create-pdf"
$endpoint = "/subscriptions/$($subscriptionId)/resourceGroups/$($resourceGroupName)/providers/Microsoft.Web/sites/$($functionAppName)/functions/create_pdf"
New-AzEventGridSubscription `
  -EventSubscriptionName $eventSubscriptionName `
  -EndpointType "azurefunction" `
  -Endpoint $endpoint `
  -ResourceGroupName $resourceGroupName `
  -TopicName $topicName

$eventSubscriptionName = "event-grid-handler-to-create-html"
$endpoint = "/subscriptions/$($subscriptionId)/resourceGroups/$($resourceGroupName)/providers/Microsoft.Web/sites/$($functionAppName)/functions/create_html"
New-AzEventGridSubscription `
  -EventSubscriptionName $eventSubscriptionName `
  -EndpointType "azurefunction" `
  -Endpoint $endpoint `
  -ResourceGroupName $resourceGroupName `
  -TopicName $topicName

# 8. Add the Azure function app configurations and connection strings.

$appset = Get-AzFunctionAppSetting `
	-Name $functionAppName `
	-ResourceGroupName $resourceGroupName

$appset.add("ContainerId", $containerName)
$appset.add("DatabaseId", $databaseName)
$appset.add("HtmlBlobContainer", $htmlContainerName)
$appset.add("PdfBlobContainer", $pdfContainerName)
$appset.add("TopicEndpointUri", $topicEndpoint)
$appset.add("TopicKeySetting", $topicAccessKey)
$appset["FUNCTIONS_WORKER_RUNTIME"] = "dotnet-isolated"

$connectionStrings = @{
	BlobStorage = @{Type = "Custom"; Value=$blobStorageConnectionString}
	CosmosDB = @{Type = "Custom"; Value=$cosmosDbConnectionString}}

Set-AzWebApp `
	-Name $functionAppName `
	-ResourceGroupName $resourceGroupName `
	-AppSettings $appset `
	-ConnectionStrings $connectionStrings

# 9. Compile deploy zipped the Azure web app.

New-AzAppServicePlan -Name $appServicePlan -ResourceGroupName $resourceGroupName -Location $location -Tier 'Free'
New-AzWebApp -Name $webAppName -Location $location -AppServicePlan $appServicePlan -ResourceGroupName $resourceGroupName

$publishFolder = Join-Path $webAppSourceFolder publish
dotnet publish "$(Join-Path $webAppSourceFolder WebDashboard.csproj)" -f net6.0 -o $publishFolder -c Release

$zippedPublishFile = Join-Path $webAppSourceFolder publish.zip
Compress-Archive -Path (Join-Path $publishFolder "*") -DestinationPath $zippedPublishFile

Publish-AzWebapp -ResourceGroupName $resourceGroupName -Name $webAppName -ArchivePath $zippedPublishFile

# 10. Add the app configurations.

$appset = Get-AzFunctionAppSetting `
	-Name $webAppName `
	-ResourceGroupName $resourceGroupName

$appset.add("AppSettings:BlobStorageConnectionString", $blobStorageConnectionString)
$appset.add("AppSettings:ContainerId", $containerName)
$appset.add("AppSettings:DatabaseConnectionString", $cosmosDbConnectionString)
$appset.add("AppSettings:DatabaseId", $databaseName)
$appset.add("AppSettings:FunctionAppUri", $functionUrl)
$appset.add("AppSettings:HtmlBlobContainer", "html")
$appset.add("AppSettings:PdfBlobContainer", "pdf")

Set-AzWebApp `
	-Name $webAppName `
	-ResourceGroupName $resourceGroupName `
	-AppSettings $appset

Write-Host "Done."