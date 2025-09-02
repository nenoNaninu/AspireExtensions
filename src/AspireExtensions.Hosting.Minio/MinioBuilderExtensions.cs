using System.Collections.Generic;
using System.Text;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

public static class MinioIntegrationExtensions
{
    public static IResourceBuilder<MinioResource> AddMinio(
        this IDistributedApplicationBuilder builder,
        string name,
        IResourceBuilder<ParameterResource> userName,
        IResourceBuilder<ParameterResource> password,
        int? port = null,
        int? consolePort = null)
    {
        var resource = new MinioResource(name, userName.Resource, password.Resource);

        return builder.AddResource(resource)
            .WithImage("minio/minio")
            .WithImageRegistry("docker.io")
            .WithHttpEndpoint(name: MinioResource.PrimaryEndpointName, port: port, targetPort: MinioResource.PrimaryTargetPort)
            .WithHttpEndpoint(name: MinioResource.ConsoleEndpointName, port: consolePort, targetPort: MinioResource.ConsoleTargetPort)
            .WithUrlForEndpoint(MinioResource.PrimaryEndpointName, annot =>
            {
                annot.DisplayText = "Primary";
            })
            .WithUrlForEndpoint(MinioResource.ConsoleEndpointName, annot =>
            {
                annot.DisplayText = "Console";
            })
            .WithEnvironment("STORAGE_TYPE", "minio")
            .WithEnvironment("MINIO_HOST", "localhost")
            .WithEnvironment("MINIO_PORT", MinioResource.PrimaryTargetPort.ToString())
            .WithEnvironment("MINIO_ROOT_USER", userName)
            .WithEnvironment("MINIO_ROOT_PASSWORD", password)
            .WithHttpHealthCheck("/minio/health/live", 200, MinioResource.PrimaryEndpointName)
            .WithArgs("server", "/data", "--console-address", $":{MinioResource.ConsoleTargetPort}");
    }

    public static IResourceBuilder<MinioResource> WithDataVolume(this IResourceBuilder<MinioResource> builder, string? name = null)
    {
        return builder.WithVolume(name ?? VolumeNameGenerator.Generate(builder, "data"), "/data");
    }

    public static IResourceBuilder<ContainerResource> AddBucket(this IResourceBuilder<MinioResource> builder, string bucketName)
    {
        return builder.AddBucket(
            name: $"{builder.Resource.Name}-create-bucket-{bucketName}",
            bucketNames: [bucketName]
        );
    }

    public static IResourceBuilder<ContainerResource> AddBucket(this IResourceBuilder<MinioResource> builder, IReadOnlyList<string> bucketNames)
    {
        return builder.AddBucket(
            name: $"{builder.Resource.Name}-create-buckets-{bucketNames[0]}",
            bucketNames: bucketNames
        );
    }

    public static IResourceBuilder<ContainerResource> AddBucket(
        this IResourceBuilder<MinioResource> builder,
        string name,
        IReadOnlyList<string> bucketNames)
    {
        return builder.ApplicationBuilder
            .AddContainer(name, "minio/mc")
            .WithImageRegistry("docker.io")
            // When using WithParentRelationship, WaitFor doesn't work.
            // issue: https://github.com/dotnet/aspire/issues/9163
            //.WithParentRelationship(builder)
            .WithReference(builder)
            .WaitFor(builder)
            .WithEntrypoint("/bin/sh")
            .WithArgs(async ctx =>
            {
                var minio = builder.Resource;

                var user = await minio.User.GetValueAsync(ctx.CancellationToken);
                var password = await minio.Password.GetValueAsync(ctx.CancellationToken);

                var sb = new StringBuilder();

                sb.Append($"mc alias set minio {GetMinioPrimaryUri(minio)} {user} {password};");

                foreach (var bucket in bucketNames)
                {
                    if (!string.IsNullOrWhiteSpace(bucket))
                    {
                        sb.Append($"mc mb minio/{bucket} --ignore-existing;");
                    }
                }

                ctx.Args.Add("-c");
                ctx.Args.Add(sb.ToString());
            });

        static string GetMinioPrimaryUri(MinioResource minio)
        {
            EndpointReference endpoint = minio.GetEndpoint(MinioResource.PrimaryEndpointName);
            return $"{endpoint.Scheme}://{minio.Name}:{endpoint.TargetPort}";
        }
    }
}
