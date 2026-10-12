namespace Tests.Container.Helpers;

/// <summary>
/// xUnit collection fixture that auto-detects whether Docker or Podman is running
/// and configures Testcontainers accordingly before any test in the collection starts.
/// </summary>
public sealed class ContainerRuntimeFixture : IAsyncLifetime
{
    private string? _previousDockerHost;
    private string? _previousRyukDisabled;

    public Task InitializeAsync()
    {
        var (runtime, dockerHost) = ContainerRuntimeHelper.Detect();

        _previousDockerHost = Environment.GetEnvironmentVariable("DOCKER_HOST");
        _previousRyukDisabled = Environment.GetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED");

        Environment.SetEnvironmentVariable("DOCKER_HOST", dockerHost);

        // Ryuk (the Testcontainers resource reaper) requires privileged mode which
        // rootless Podman does not support by default.
        if (runtime == ContainerRuntime.Podman)
            Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Environment.SetEnvironmentVariable("DOCKER_HOST", _previousDockerHost);
        Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", _previousRyukDisabled);
        return Task.CompletedTask;
    }
}
