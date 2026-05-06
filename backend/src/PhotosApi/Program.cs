using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minio;
using PhotosApi.Contracts;
using PhotosApi.Infrastructure.Data;
using PhotosApi.Infrastructure.Storage;
using PhotosApi.Services;
using PhotosApi.Services.Behaviors;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables();

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

builder.Services.Configure<MinioOptions>(
    builder.Configuration.GetSection(MinioOptions.SectionName));

builder.Services.AddMinio(cfg =>
{
    var opts = builder.Configuration.GetSection(MinioOptions.SectionName).Get<MinioOptions>()!;
    
    cfg.WithEndpoint(opts.GetContainerConnection)
        .WithCredentials(opts.AccessKey, opts.SecretKey);

    cfg.WithSSL(opts.UseSsl);
});



builder.Services.AddScoped<CategoryService>();
builder.Services.AddTransient<IStorageRepository, MinioStorageRepository>();
builder.Services.AddSingleton<BucketInitializerService>();
builder.Services.AddHostedService<MinioHealthCheckService>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly);
});



builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

var app = builder.Build();

app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseSerilogRequestLogging();
app.UseStatusCodePages();

app.MapControllers();

app.Run("http://*:8000");