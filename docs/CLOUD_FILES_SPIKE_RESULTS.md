# Cloud Files API Spike Results

## Current status

Scaffold created. Real CfAPI execution is still pending on Windows.

## Environment used for scaffold verification

- Hermes host: Linux/Debian
- .NET SDK: installed locally under `/root/.dotnet` for structural build/test
- CfAPI runtime: unavailable on Linux

## Next Windows validation checklist

- [ ] Run `dotnet run --project experiments/CfApiSpike/CfApiSpike.csproj` on Windows.
- [ ] Implement `CloudFilesApi` P/Invoke bindings.
- [ ] Register `%USERPROFILE%\MountDockCloudSpike` as a sync root.
- [ ] Create `hello.txt` placeholder.
- [ ] Confirm Explorer shows online-only state.
- [ ] Hydrate `hello.txt` on open.
- [ ] Dehydrate `hello.txt` back to online-only.
- [ ] Record screenshots/logs and API caveats here.

## Notes

This file intentionally remains open-ended until the spike is run on a Windows machine.
