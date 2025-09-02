using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

public sealed class MinioResource(string name, ParameterResource user, ParameterResource password)
    : ContainerResource(name), IResourceWithConnectionString
{
    public const string PrimaryEndpointName = "http";
    public const string ConsoleEndpointName = "console";

    internal const int PrimaryTargetPort = 9000;
    internal const int ConsoleTargetPort = 9001;

    internal ParameterResource User { get; } = user;
    internal ParameterResource Password { get; } = password;

    public ReferenceExpression ConnectionStringExpression
        => this.TryGetLastAnnotation<ConnectionStringRedirectAnnotation>(out var connectionStringAnnotation)
            ? connectionStringAnnotation.Resource.ConnectionStringExpression
            : ReferenceExpression.Create($"Endpoint={this.GetEndpoint(PrimaryEndpointName).Url};User={this.User};Password={this.Password}");
}
