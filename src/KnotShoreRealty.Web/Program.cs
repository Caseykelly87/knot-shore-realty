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
        var path = builder.Configuration["SeedData:Path"] ?? "seed-data";
        var logger = sp.GetRequiredService<ILogger<SeedDataLoader>>();
        return new SeedDataLoader(path, logger);
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
