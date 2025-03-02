using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Examples_GrpcService>("grpcservice")
    .WithGrpcUI(port: 54321);

builder.Build().Run();
