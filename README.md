# AspireExtensions.GrpcUI

gRPC UI support for .NET Aspire.

## Table of Contents

- [Install](#install)
- [API](#api)
- [Usage](#usage)

## Install

Install [grpcui](https://github.com/fullstorydev/grpcui) on your local machine.

- Windows
  - Download from [grpcui/releases](https://github.com/fullstorydev/grpcui/releases)
  - **Add PATH to `grpcui.exe`**
- Mac
  - `brew install grpcui`

Please check if grpcui is available by using the following command.

```
$ grpcui -version
```

And add package to your Aspire AppHost project.

```
$ dotnet add package AspireExtensions.GrpcUI
```

## API

`AspireExtensions.GrpcUI` provide `WithGrpcUI()` method.
Use `WithGrpcUI()` in your Aspire AppHost project.

```cs
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MyGrpcService>("grpcservice")
    .WithGrpcUI(port: 54321); // <- Add this!

builder.Build().Run();
```

## Usage

gRPC Reflection must be enabled to use the gRPC UI.

First, add `Grpc.AspNetCore.Server.Reflection` to your gRPC service project.

```
$ dotnet add package Grpc.AspNetCore.Server.Reflection
```

Then use `AddGrpcReflection()` and `MapGrpcReflectionService()`.

```cs
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddGrpc();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddGrpcReflection(); // <- Add this!
}

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService(); // <- Add this!
}

app.MapGrpcService<GreeterService>();

app.Run();
```

Next, use `WithGrpcUI()` in your Aspire AppHost project.

```cs
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MyGrpcService>("grpcservice")
    .WithGrpcUI(port: 54321); // <- Add this!

builder.Build().Run();
```

