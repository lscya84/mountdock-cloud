using MountDock.CloudProvider.CloudFiles;

Console.WriteLine("MountDock Cloud CfAPI spike harness");
Console.WriteLine("This spike must be run on Windows with real CfAPI bindings implemented.");

var syncRoot = args.Length > 0
    ? args[0]
    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "MountDockCloudSpike");

Console.WriteLine($"Planned test sync root: {syncRoot}");
Console.WriteLine("Next implementation steps:");
Console.WriteLine("1. Replace CloudFilesApi stubs with CfAPI P/Invoke calls.");
Console.WriteLine("2. Register the sync root.");
Console.WriteLine("3. Create hello.txt as an online-only placeholder.");
Console.WriteLine("4. Hydrate hello.txt with local test payload on open.");
Console.WriteLine("5. Dehydrate hello.txt back to online-only.");

var registrar = new SyncRootRegistrar(new CloudFilesApi());
try
{
    registrar.Register("mountdock-cloud-spike", "MountDock Cloud Spike", syncRoot);
}
catch (NotImplementedException exc)
{
    Console.WriteLine($"Expected scaffold stop: {exc.Message}");
}
