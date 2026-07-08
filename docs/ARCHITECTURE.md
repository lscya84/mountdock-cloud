# MountDock Cloud Architecture

## Goal

Build a OneDrive-style on-demand file provider for arbitrary rclone remotes.

## Non-goals for the first spike

- Full bidirectional sync
- Delete propagation
- Conflict UI
- Production installer
- Drive-letter parity

## Components

### MountDock.CloudProvider

A Windows provider/helper process responsible for:

- registering Cloud Files sync roots
- creating and updating placeholders
- receiving hydration callbacks
- updating pin/dehydrate states
- coordinating with the rclone backend adapter
- maintaining sync state

### CloudFiles layer

Wrapper around Windows Cloud Files API concepts:

- `SyncRootRegistrar`
- `PlaceholderManager`
- `HydrationManager`
- `CloudFilesApi`

The initial code is intentionally interface-first so the CfAPI spike can replace stubs with real P/Invoke bindings.

### Rclone layer

rclone is a transfer/listing backend only. It must not mount a filesystem.

Operations:

- `lsjson`
- `copyto remote -> local`
- `copyto local -> remote`
- `deletefile`
- `moveto`

### State layer

SQLite-backed metadata and queue state in production. The initial scaffold only defines the boundary.

## Runtime flow

### Register profile

```text
profile config
  -> register sync root
  -> list remote metadata
  -> create placeholders
```

### Hydrate file

```text
Explorer opens online-only file
  -> CfAPI hydration callback
  -> rclone copyto remote:file temp:file
  -> provider fills placeholder
  -> Explorer receives local content
```

### Upload dirty file

```text
local file changed
  -> queue upload
  -> check remote metadata
  -> upload or create conflict
```

## Language decision

Core provider: C#/.NET first.

Reasoning:

- safer and faster than C++ for product code
- better long-running service, logging, JSON, SQLite, IPC ergonomics
- practical Win32 interop via P/Invoke for CfAPI spike

Python/PyQt MountDock remains a legacy/reference project, not the Cloud Files provider runtime.
