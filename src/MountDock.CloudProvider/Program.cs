using MountDock.CloudProvider.Rclone;

Console.WriteLine("MountDock Cloud Provider scaffold");
Console.WriteLine("This executable is a placeholder until the Windows CfAPI spike is implemented.");

var command = RcloneCommandBuilder.BuildLsJson("rclone", "remote:path", recursive: false, configPath: null);
Console.WriteLine(string.Join(" ", command));
