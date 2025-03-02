# AspireExtensions.GrpcUI

gRPC UI support for .NET Aspire.

## Table of Contents

- [Install](#install)
- [Usage](#usage)

## Install

Install [grpcui](https://github.com/fullstorydev/grpcui) in your local machine.

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

## Usage

Use `WithGrpcUI()` method!

```cs
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MyGrpcService>("grpcservice")
    .WithGrpcUI(port: 54321); // <- Add this!

builder.Build().Run();
```

