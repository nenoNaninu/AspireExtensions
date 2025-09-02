using System.IO;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;

namespace Examples.GrpcService.Services;

internal sealed class GreeterService : Greeter.GreeterBase
{
    private readonly IMinioClient _minioClient;
    private readonly ILogger<GreeterService> _logger;

    public GreeterService(IMinioClient minioClient, ILogger<GreeterService> logger)
    {
        _minioClient = minioClient;
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }

    public override async Task<Empty> SaveBlob(SaveBlobRequest request, ServerCallContext context)
    {
        var bytes = Encoding.UTF8.GetBytes(request.Value);

        var argContent = new PutObjectArgs()
            .WithBucket("my-bucket-1")
            .WithObject(request.Name)
            .WithStreamData(new MemoryStream(bytes))
            .WithObjectSize(bytes.Length)
            .WithContentType("text/plain");

        await _minioClient.PutObjectAsync(argContent, context.CancellationToken);

        return new Empty();
    }
}
