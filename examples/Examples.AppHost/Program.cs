using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDotnetProject("grpcService", "../Examples.GrpcService/Examples.GrpcService.csproj")
    .AsHttp2Service()
    .WithGrpcUI(port: 54321);

builder.Build().Run();
