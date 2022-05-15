using ArtifactClientService;
using DictionaryClientService;
using HtmlCreatorService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PdfCreatorService;
using System;
using System.Diagnostics;
using WordDbClientService;

var host = new HostBuilder()
	.ConfigureFunctionsWorkerDefaults()
	.ConfigureServices(s =>
	{		
		s.AddSingleton<IWordDbClient>(client => new WordDbClient(
			Environment.GetEnvironmentVariable(Debugger.IsAttached ? "ConnectionStrings:CosmosDB" : "CUSTOMCONNSTR_CosmosDB"),
			Environment.GetEnvironmentVariable("DatabaseId"),
			Environment.GetEnvironmentVariable("ContainerId")
			));
		s.AddSingleton<IArtifactClient>(client => new ArtifactClient(
			Environment.GetEnvironmentVariable(Debugger.IsAttached ? "ConnectionStrings:BlobStorage" : "CUSTOMCONNSTR_BlobStorage")));
		s.AddSingleton<IDictionaryClient, DictionaryClient>();
		s.AddSingleton<IPdfCreator, PdfCreator>();
		s.AddSingleton<IHtmlCreator, HtmlCreator>();
	})
	.Build();

await host.RunAsync();