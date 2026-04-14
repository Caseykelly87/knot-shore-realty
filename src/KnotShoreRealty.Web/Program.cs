using System.Text.Json;
using KnotShoreRealty.Core.Interfaces;
using KnotShoreRealty.Data;
using KnotShoreRealty.Data.Repositories;
using KnotShoreRealty.Data.Seed;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, config) =>
        config.ReadFrom.Configuration(context.Configuration)
              .ReadFrom.Services(services));

    builder.Services.AddDbContext<KnotShoreRealtyDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddHealthChecks()
        .AddDbContextCheck<KnotShoreRealtyDbContext>();

    builder.Services.AddScoped<IListingRepository, ListingRepository>();
    builder.Services.AddScoped<IAgentRepository, AgentRepository>();
    builder.Services.AddScoped<INeighborhoodRepository, NeighborhoodRepository>();
    builder.Services.AddScoped<IInquiryRepository, InquiryRepository>();

    builder.Services.AddScoped<SeedDataLoader>(sp =>
    {
        var configured = builder.Configuration["SeedData:Path"] ?? "seed-data";
        // When the configured path is relative, walk up from the content root until a
        // matching folder is found. This lets seed data live at the repo root rather than
        // inside the web project directory without requiring an absolute path in config.
        var resolved = Path.IsPathRooted(configured)
            ? configured
            : FindSeedDataFolder(builder.Environment.ContentRootPath, configured);
        var logger = sp.GetRequiredService<ILogger<SeedDataLoader>>();
        return new SeedDataLoader(resolved, logger);
    });
    builder.Services.AddScoped<DbInitializer>();

    builder.Services.AddControllersWithViews();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
        await initializer.InitializeAsync();
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthorization();

    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";

            var result = JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description
                })
            });

            await context.Response.WriteAsync(result);
        }
    });

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Walks up from startDir looking for a subdirectory named folderName.
// Returns the first match found, or startDir/folderName if nothing is found
// (letting SeedDataLoader log a clear "file not found" warning).
static string FindSeedDataFolder(string startDir, string folderName)
{
    var dir = new DirectoryInfo(startDir);
    while (dir != null)
    {
        var candidate = Path.Combine(dir.FullName, folderName);
        if (Directory.Exists(candidate))
            return candidate;
        dir = dir.Parent;
    }
    return Path.Combine(startDir, folderName);
}
