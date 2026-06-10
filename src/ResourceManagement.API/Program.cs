using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ResourceManagement.API.Middleware;
using ResourceManagement.Application.Extensions;
using ResourceManagement.Infrastructure.Data;
using ResourceManagement.Infrastructure.Extensions;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

// Bootstrap Serilog early so startup errors log
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting ResourceManagement API...");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ──────────────────────────────────
    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/api-.log", rollingInterval: RollingInterval.Day));

    // ── Application & Infrastructure layers ──────
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // ── JWT Authentication ────────────────────────
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured.");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtSettings["Issuer"],
            ValidAudience            = jwtSettings["Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

    // ── CORS ──────────────────────────────────────
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    });

    // ── API Versioning + ApiExplorer ──────────────
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion                = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions                = true;
        options.ApiVersionReader                 = new UrlSegmentApiVersionReader();
    });

    builder.Services.AddVersionedApiExplorer(options =>
    {
        options.GroupNameFormat           = "'v'VVV";   // e.g. "v1"
        options.SubstituteApiVersionInUrl = true;
    });

    // ── Controllers ───────────────────────────────
    builder.Services.AddControllers();

    // ── Swagger ───────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
    builder.Services.AddSwaggerGen(c =>
    {
        // JWT support in Swagger UI
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header. Enter: Bearer {token}",
            Name        = "Authorization",
            In          = ParameterLocation.Header,
            Type        = SecuritySchemeType.ApiKey,
            Scheme      = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    // 
    var app = builder.Build();
    // 

    // ── DB Initialization ─────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var db     = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        await DbInitializer.InitializeAsync(db, logger);
    }

    // ── Middleware pipeline ───────────────────────
    app.UseMiddleware<ExceptionMiddleware>();
    app.UseSerilogRequestLogging();

    // ── Swagger (always enabled — not just Development) ──
    var apiVersionDescriptionProvider =
        app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // Build one tab per API version
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.Reverse())
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"ResourceManagement API {description.GroupName.ToUpperInvariant()}");
        }

        options.RoutePrefix = "swagger"; // available at /swagger
        options.DocumentTitle = "ResourceManagement API";
    });

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");

    // Security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options",        "DENY");
        context.Response.Headers.Add("X-XSS-Protection",       "1; mode=block");
        await next();
    });

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // Redirect root to swagger for convenience
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed.");
}
finally
{
    Log.CloseAndFlush();
}

// Needed for MVC Testing
public partial class Program { }

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        => _provider = provider;

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title       = "ResourceManagement API",
                Version     = description.ApiVersion.ToString(),
                Description = $"Production-ready RESTful API for Products and Items management. {(description.IsDeprecated ? "⚠️ This API version is deprecated." : string.Empty)}"
            });
        }
    }
}
