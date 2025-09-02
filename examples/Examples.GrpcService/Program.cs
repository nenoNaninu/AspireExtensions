using Examples.GrpcService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddGrpc();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddGrpcReflection();
}

builder.AddServiceDefaults();

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestProperties | HttpLoggingFields.ResponseStatusCode | HttpLoggingFields.Duration;
});

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.UseHttpLogging();

app.MapGrpcService<GreeterService>();

app.Run();
