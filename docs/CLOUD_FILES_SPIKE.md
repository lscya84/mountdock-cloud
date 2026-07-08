# Cloud Files API Spike Plan

## Objective

Prove that MountDock Cloud can use Windows Cloud Files API to provide OneDrive-style online-only placeholders.

## Acceptance criteria

1. Register a test sync root.
2. Create one online-only placeholder file.
3. Confirm Explorer shows the placeholder under the sync root.
4. Opening/copying the file triggers hydration.
5. Hydration fills content from a local test payload.
6. File can be dehydrated back to online-only.
7. Findings are documented in `docs/CLOUD_FILES_SPIKE_RESULTS.md`.

## Spike phases

### Phase 0 — Windows API baseline

- Identify exact CfAPI functions and signatures needed.
- Decide whether C# P/Invoke is sufficient or whether a C++ helper is needed.
- Document minimum Windows version.

### Phase 1 — Sync root registration

Implement `SyncRootRegistrar` for a test root:

```text
%USERPROFILE%\MountDockCloudSpike
```

### Phase 2 — Placeholder creation

Create one file placeholder:

```text
%USERPROFILE%\MountDockCloudSpike\hello.txt
```

Expected logical content:

```text
Hello from MountDock Cloud hydration.
```

### Phase 3 — Hydration callback

When Explorer/Notepad opens `hello.txt`, provider supplies content from a local test payload.

### Phase 4 — Dehydration

Add a debug command to dehydrate `hello.txt` back to online-only.

## Out of scope

- rclone remote integration
- recursive directory listing
- uploads
- conflict handling
- installer
