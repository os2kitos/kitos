using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Presentation.Web;
using Presentation.Web.Infrastructure.Configuration;
using Presentation.Web.Infrastructure.DI;
using Presentation.Web.Models.Application.RuntimeEnv;
using System;

// Must be set before any Npgsql type is loaded (including Hangfire's PostgreSQL storage).
// Allows writing DateTime with Kind=UTC to 'timestamp without time zone' columns.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Digst.OioIdws.* assemblies are IL-patched local DLLs (not NuGet packages) referenced
// as "type:reference" in deps.json. The runtime's assembly loader skips those entries
// and the AppContext.BaseDirectory fallback can fail under some hosting models (IIS,
// dotnet watch). Registering an explicit resolver here ensures they are always found.
System.Runtime.Loader.AssemblyLoadContext.Default.Resolving += static (context, name) =>
{
    if (name.Name?.StartsWith("Digst.OioIdws.", StringComparison.Ordinal) != true)
        return null;
    var path = System.IO.Path.Combine(AppContext.BaseDirectory, name.Name + ".dll");
    return System.IO.File.Exists(path) ? context.LoadFromAssemblyPath(path) : null;
};

var builder = WebApplication.CreateBuilder(args);

builder.AddKitosSerilog();

var configuration = builder.Configuration;
var services = builder.Services;

services.AddKitosMvc();
services.AddRouting();
var signingKey = services.AddKitosAuthentication(configuration);
var kitosEnv = KitosEnvironmentConfiguration.FromConfiguration(configuration).Environment;
services.AddKitosSwagger(kitosEnv is not (KitosEnvironment.Production or KitosEnvironment.Staging));
services.AddKitosHangfire(configuration);

// AutoMapper - using explicit assembly scanning
var mapperConfig = new MapperConfiguration(cfg => cfg.AddMaps(typeof(MappingConfig).Assembly), NullLoggerFactory.Instance);
services.AddSingleton(mapperConfig.CreateMapper());

// HttpContext accessor
services.AddHttpContextAccessor();

// KITOS services (DI registrations)
KitosServiceRegistration.Register(services, configuration, signingKey);

var app = builder.Build();

// Initialize the SAML library's static HTTP context accessor so it can access HttpContext.Current
// during SAML flows without requiring DI injection into the (statically-instantiated) handler classes.
dk.nita.saml20.Utils.SamlHttpContextAccessor.Configure(
    app.Services.GetRequiredService<IHttpContextAccessor>());

app.EnsurePostgreSqlExtensions();
app.UseKitosPipeline();
app.MapKitosEndpoints();
app.InitializeHangfireJobs();

app.Run();
