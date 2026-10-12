namespace Tests.Container.Helpers;

internal enum ContainerRuntime
{
    Docker,
    Podman
}

/// <summary>
/// Detects which container runtime (Docker or Podman) is currently available and
/// returns the corresponding DOCKER_HOST URI.
/// </summary>
internal static class ContainerRuntimeHelper
{
    public static (ContainerRuntime Runtime, string DockerHost) Detect()
    {
        var explicitHost = Environment.GetEnvironmentVariable("DOCKER_HOST");
        if (!string.IsNullOrEmpty(explicitHost))
        {
            var runtime = explicitHost.Contains("podman", StringComparison.OrdinalIgnoreCase)
                ? ContainerRuntime.Podman
                : ContainerRuntime.Docker;
            return (runtime, explicitHost);
        }

        return OperatingSystem.IsWindows() ? DetectWindows() : DetectUnix();
    }

    private static (ContainerRuntime, string) DetectWindows()
    {
        if (NamedPipeExists("docker_engine"))
            return (ContainerRuntime.Docker, "npipe://./pipe/docker_engine");

        if (NamedPipeExists("podman-machine-default"))
            return (ContainerRuntime.Podman, "npipe://./pipe/podman-machine-default");

        throw new InvalidOperationException(
            "No container runtime detected. Start Docker Desktop or Podman Desktop before running these tests.");
    }

    private static (ContainerRuntime, string) DetectUnix()
    {
        if (File.Exists("/var/run/docker.sock"))
            return (ContainerRuntime.Docker, "unix:///var/run/docker.sock");

        foreach (var sock in PodmanSocketCandidates())
        {
            if (File.Exists(sock))
                return (ContainerRuntime.Podman, $"unix://{sock}");
        }

        throw new InvalidOperationException(
            "No container runtime detected. Ensure Docker or Podman is running before running these tests.");
    }

    private static bool NamedPipeExists(string pipeName)
    {
        try
        {
            return Directory.GetFiles(@"\\.\pipe\")
                .Any(p => p.EndsWith(pipeName, StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }

    private static IEnumerable<string> PodmanSocketCandidates()
    {
        var xdgRuntime = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
        if (!string.IsNullOrEmpty(xdgRuntime))
            yield return Path.Combine(xdgRuntime, "podman", "podman.sock");

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        yield return Path.Combine(home, ".local", "share", "containers", "podman", "machine", "qemu", "podman.sock");
        yield return Path.Combine(home, ".local", "share", "containers", "podman", "machine", "podman.sock");
        yield return "/tmp/podman.sock";
    }
}
