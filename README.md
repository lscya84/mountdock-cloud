# MountDock Cloud

MountDock Cloud is a Windows Cloud Files API based successor/spike project for MountDock.

The goal is a real OneDrive-style on-demand drive experience backed by `rclone`:

- online-only placeholders in Windows Explorer
- hydrate/download only when a file is opened, copied, or explicitly pinned
- dehydrate/free local space while keeping metadata visible
- Explorer status icons for online-only, syncing, offline, pinned, error, and conflict states
- rclone as the remote transport layer, not as a mounted filesystem

## Repository status

This repository starts as an architecture and feasibility spike. The first milestone is not a full product; it is a Windows CfAPI proof of concept.

## Target architecture

```text
Windows Explorer
  -> Windows Cloud Files API sync root / placeholders
    -> MountDock Cloud Provider helper
      -> metadata DB + operation queue
        -> rclone backend adapter
          -> Google Drive / OneDrive / WebDAV / S3 / Dropbox / etc.
```

## Projects

```text
src/MountDock.CloudProvider/        Native-facing provider host and core service skeleton
src/MountDock.CloudProvider.Tests/  Unit tests for command builders and pure logic
docs/                              Architecture, spike plan, and TODOs
```

## Development prerequisites

- Windows 10/11 for real CfAPI testing
- .NET 8 SDK or newer
- rclone installed for backend integration tests

This Linux/Hermes environment currently does not have `dotnet`, so the initial scaffold is verified structurally here and must be compiled on a Windows/.NET machine.

## First milestone

See [`docs/CLOUD_FILES_SPIKE.md`](docs/CLOUD_FILES_SPIKE.md).
