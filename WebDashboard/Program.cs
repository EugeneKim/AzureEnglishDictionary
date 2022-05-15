using WordDbClientService;
using WebDashboard;
using ArtifactClientService;

var builder = WebApplication.CreateBuilder(args);

IConfiguration config = new ConfigurationBuilder()
	.AddJsonFile("appsettings.json")
	.AddJsonFile($"appsettings.Development.json", optional: true)
	.AddEnvironmentVariables()
	.Build();

var settings = config.GetRequiredSection("AppSettings").Get<AppSettings>();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddSingleton<IWordDbClient, WordDbClient>(client => 
	new WordDbClient(settings.DatabaseConnectionString, settings.DatabaseId, settings.ContainerId));
builder.Services.AddSingleton<IArtifactClient>(client => new ArtifactClient(
	settings.BlobStorageConnectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
	app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
