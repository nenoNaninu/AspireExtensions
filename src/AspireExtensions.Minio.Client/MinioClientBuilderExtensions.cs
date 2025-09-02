using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Minio;

namespace AspireExtensions.Minio.Client;

public static class MinioClientBuilderExtensions
{
    public static void AddMinioClient(this IHostApplicationBuilder builder, string name)
    {
        var connectionString = builder.Configuration.GetConnectionString(name)
            ?? throw new InvalidOperationException($"Connection string '{name}' not found.");

        builder.Services.AddMinioClient(connectionString);
    }

    public static IServiceCollection AddMinioClient(this IServiceCollection services, string connectionString)
    {
        var parsed = Parse(connectionString);

        return services.AddMinio(client =>
            client.WithEndpoint(parsed.Endpoint)
                .WithCredentials(parsed.User, parsed.Password)
                .WithSSL(parsed.Endpoint!.Scheme == "https")
                .Build()
        );
    }

    private readonly record struct ParsedConnectionString(Uri Endpoint, string User, string Password);

    private static ParsedConnectionString Parse(string connectionString)
    {
        Uri? endpoint = null;
        string? user = null;
        string? password = null;

        var span = connectionString.AsSpan();

        foreach (var range in span.Split(';'))
        {
            var keyValuePair = span[range];

            if (keyValuePair.StartsWith("endpoint", StringComparison.OrdinalIgnoreCase))
            {
                endpoint = new Uri(GetValueString(keyValuePair));
                continue;
            }

            if (keyValuePair.StartsWith("user", StringComparison.OrdinalIgnoreCase))
            {
                user = GetValueString(keyValuePair);
                continue;
            }

            if (keyValuePair.StartsWith("password", StringComparison.OrdinalIgnoreCase))
            {
                password = GetValueString(keyValuePair);
                continue;
            }
        }

        return new ParsedConnectionString(endpoint!, user ?? string.Empty, password ?? string.Empty);
    }

    private static string GetValueString(ReadOnlySpan<char> keyValuePair)
    {
        var index = keyValuePair.IndexOf('=') + 1;
        var value = keyValuePair[index..];
        return value.ToString();
    }
}
