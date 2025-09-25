using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var minio = builder.AddMinio(
        name: "minio",
        userName: builder.AddParameter("MinioUser"),
        password: builder.AddParameter("MinioPassword"),
        port: 9110,
        consolePort: 9111
    )
    .WithImageTag("RELEASE.2025-07-23T15-54-02Z")
    .WithDataVolume("my.minio.volume");

var bucketCreation = minio.AddBucket(["my-bucket-1", "my-bucket-2"])
    .WithImageTag("RELEASE.2025-07-21T05-28-08Z");

builder.AddProject<Projects.Examples_GrpcService>("grpcService")
    .AsHttp2Service()
    .WithReference(minio)
    .WaitFor(minio)
    .WaitForCompletion(bucketCreation)
    .WithGrpcUI(port: 54321);

builder.Build().Run();
