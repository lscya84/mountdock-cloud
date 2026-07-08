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
        try
        {
            var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            return new RcloneResult(process.ExitCode, await stdoutTask, await stderrTask);
        }
        catch (OperationCanceledException)
        {
            TryKill(process);
            throw;
        }
    }

    public async Task<RcloneResult> RunWithTimeoutAsync(
        IReadOnlyList<string> command,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));

        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        try
        {
            return await RunAsync(command, linkedCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new RcloneTimeoutException(command, timeout);
        }
    }

    public async Task<RcloneResult> RunRequiredAsync(IReadOnlyList<string> command, CancellationToken cancellationToken)
    {
        var result = await RunAsync(command, cancellationToken);
        if (!result.Success)
        {
            throw new RcloneCommandException(command, result);
        }
        return result;
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // Best-effort cleanup; preserve the original cancellation/timeout signal.
        }
    }
}

public sealed record RcloneResult(int ExitCode, string Stdout, string Stderr)
{
    public bool Success => ExitCode == 0;
}

public sealed class RcloneCommandException : Exception
{
    public RcloneCommandException(IReadOnlyList<string> command, RcloneResult result)
        : base($"rclone command failed with exit code {result.ExitCode}: {string.Join(' ', command)}")
    {
        Command = command;
        Result = result;
    }

    public IReadOnlyList<string> Command { get; }
    public RcloneResult Result { get; }
}

public sealed class RcloneTimeoutException : TimeoutException
{
    public RcloneTimeoutException(IReadOnlyList<string> command, TimeSpan timeout)
        : base($"rclone command timed out after {timeout}: {string.Join(' ', command)}")
    {
        Command = command;
        Timeout = timeout;
    }

    public IReadOnlyList<string> Command { get; }
    public TimeSpan Timeout { get; }
}
