using System.Diagnostics;

namespace MountDock.CloudProvider.Rclone;

public sealed class RcloneClient
{
    public async Task<RcloneResult> RunAsync(IReadOnlyList<string> command, CancellationToken cancellationToken)
    {
        if (command.Count == 0) throw new ArgumentException("Command must include executable path.", nameof(command));

        var startInfo = new ProcessStartInfo
        {
            FileName = command[0],
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        foreach (var arg in command.Skip(1))
        {
            startInfo.ArgumentList.Add(arg);
        }

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start rclone process.");
        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        return new RcloneResult(process.ExitCode, await stdoutTask, await stderrTask);
    }
}

public sealed record RcloneResult(int ExitCode, string Stdout, string Stderr)
{
    public bool Success => ExitCode == 0;
}
