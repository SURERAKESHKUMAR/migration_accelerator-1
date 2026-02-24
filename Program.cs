using System.Text.Json;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.EntityFrameworkCore;
using migration_accelerator.Models;
using migration_accelerator.Services;
using SecretsScanner.Gitleaks;

var builder = WebApplication.CreateBuilder(args);
// Read meta config first
var region = builder.Configuration["AWS:Region"];
var secretName = builder.Configuration["AWS:SecretName"];

// Plug AWS secrets into IConfiguration pipeline
builder.Configuration.AddAwsSecrets(region!, secretName!);

AwsSecretsMapper.Apply(builder.Configuration);

// 3. Now EF can read connection string
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// builder.Configuration["ConnectionStrings:DefaultConnection"] =
//     builder.Configuration["AwsSecreteConnection"];
// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseSqlServer(
//         builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));

builder.Services.AddSingleton<GitleaksScanner>();
// Add services to the container.
builder.Services.AddControllersWithViews();
// Register your custom service
builder.Services.AddAWSService<IAmazonSecretsManager>();
builder.Services.AddScoped<AwsSecretsService>();


// GitHub Service Registration with HttpClient
builder.Services.AddHttpClient<IGitHubService, GitHubService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["GitHub:BaseUrl"]);
});
builder.Services.AddSingleton<ScanService>();
builder.Services.AddSingleton<TerraformBackgroundService>();
builder.Services.AddHostedService(provider =>
    provider.GetRequiredService<TerraformBackgroundService>());
builder.Services.AddScoped<TerraformExecutor>();
builder.Services.AddSingleton<TerraformBackgroundService>();
builder.Services.AddHostedService(p =>
    p.GetRequiredService<TerraformBackgroundService>());


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
