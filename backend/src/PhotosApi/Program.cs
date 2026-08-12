using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Minio;
using PhotosApi.Contracts;
using PhotosApi.Infrastructure.Data;
using PhotosApi.Infrastructure.Errors;
using PhotosApi.Infrastructure.Storage;
using PhotosApi.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables();

// logging (Serilog)
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.UseInlineDefinitionsForEnums();
});

// error handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Model binding failed: {Errors}", context.ModelState);
        return new BadRequestObjectResult(context.ModelState);
    };
});

builder.Services.AddMemoryCache();

builder.Services.AddDbContext<PhotosDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("PhotosDatabase")));

// minio
builder.Services.Configure<MinioOptions>(builder.Configuration.GetSection("Minio"));

// internal client for api
builder.Services.AddKeyedSingleton<IMinioClient>("internal", (sp, key) =>
{
    var opts = sp.GetRequiredService<IOptions<MinioOptions>>().Value;
    
    return new MinioClient()
        .WithEndpoint(opts.Endpoint, opts.Port)
        .WithCredentials(opts.AccessKey, opts.SecretKey)
        .WithSSL(opts.UseSsl)
        .Build();
});

builder.Services.AddKeyedSingleton<IMinioClient>("public", (sp, key) =>
{
    var opts = sp.GetRequiredService<IOptions<MinioOptions>>().Value;
    
    return new MinioClient()
        .WithEndpoint(opts.PublicEndpoint, 80)
        .WithCredentials(opts.AccessKey, opts.SecretKey)
        .WithSSL(opts.UseSsl)
        .Build();
});

builder.Services.AddScoped<IObjectRepository>(sp => 
    new MinioObjectRepository(
        sp.GetRequiredKeyedService<IMinioClient>("internal"),
        sp.GetRequiredKeyedService<IMinioClient>("public"),
        sp.GetRequiredService<ILogger<MinioObjectRepository>>()
    ));

// infrastructure services
builder.Services.AddScoped<CategoryService>();

builder.Services.AddHostedService<BucketInitializerService>();

builder.Services.AddScoped<PhotoService>();
builder.Services.AddSingleton<ImageSharpPhotoProcessor>();

// registration cqrs services
var assembly = typeof(Program).Assembly;
var serviceTypes = assembly.GetTypes()
    .Where(t => t is { IsClass: true, IsAbstract: false }
                && (t.Name.EndsWith("QueryService") || t.Name.EndsWith("CommandService"))
    );
foreach (var serviceType in serviceTypes)
    builder.Services.AddScoped(serviceType);

// fluent validation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.UseExceptionHandler("/error");

app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseStatusCodePages();

app.MapControllers();

app.Run("http://*:8000");