using System;
using System.IO;
using System.Linq;
using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

public static class Extensions
{
    public static IResourceBuilder<ProjectResource> WithGrpcUI(this IResourceBuilder<ProjectResource> builder, int port)
    {
        if (!builder.ApplicationBuilder.ExecutionContext.IsRunMode)
        {
            return builder;
        }

        if (!IsGrpcUIBinaryExist())
        {
            Console.WriteLine("""
                grpcui binary does not exist.
                See README.md
                https://github.com/nenoNaninu/AspireExtensions.GrpcUI?tab=readme-ov-file#install
                """);

            return builder;
        }

        builder.ApplicationBuilder.AddExecutable($"{builder.Resource.Name}-grpcui", "grpcui", ".", [])
            .WaitFor(builder)
            .WithParentRelationship(builder)
            .WithHttpEndpoint(port: port, isProxied: false)
            .WithArgs(context =>
            {
                var endpoint = builder.Resource.GetEndpoints()
                    .First(x => x.Scheme is "http" or "https");

                if (endpoint.Scheme == "http")
                {
                    context.Args.Add("-plaintext");
                }

                context.Args.Add("-port");
                context.Args.Add(port.ToString());
                context.Args.Add($"{endpoint.Host}:{endpoint.Port}");
            })
            .WithExplicitStart();

        return builder;
    }

    private static bool IsGrpcUIBinaryExist()
    {
        var separator = OperatingSystem.IsWindows() ? ';' : ':';
        var exe = OperatingSystem.IsWindows() ? "grpcui.exe" : "grpcui";

        var pathString = Environment.GetEnvironmentVariable("PATH");

        if (string.IsNullOrEmpty(pathString))
        {
            return false;
        }

#if NET8_0
        foreach (var path in pathString.Split(separator))
        {
            var exePath = Path.Join(path, exe);
            if (File.Exists(exePath))
            {
                return true;
            }
        }
#endif
#if NET9_0_OR_GREATER
        var paths = pathString.AsSpan();
        var exeSpan = exe.AsSpan();

        foreach (var range in paths.Split(separator))
        {
            var exePath = Path.Join(paths[range], exeSpan);
            if (File.Exists(exePath))
            {
                return true;
            }
        }
#endif

        return false;
    }
}
