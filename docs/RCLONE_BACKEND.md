# rclone Backend Design

## Principle

rclone is the remote transport backend, not the mounted filesystem.

MountDock Cloud should call rclone commands explicitly and keep Explorer isolated from remote latency.

## Initial operations

### List remote directory

```bash
rclone lsjson remote:path --recursive --files-only
```

For huge remotes, prefer lazy directory listing instead of full recursive listing.

### Download one file for hydration

```bash
rclone copyto remote:path/file.ext C:\Temp\MountDockCloud\file.ext
```

### Upload one dirty file

```bash
rclone copyto C:\Local\file.ext remote:path/file.ext
```

### Delete remote file

```bash
rclone deletefile remote:path/file.ext
```

## Command safety rules

- Never use `shell=true` equivalent.
- Always pass arguments as arrays.
- Do not log secrets or tokens.
- Capture stdout/stderr.
- Timeouts and cancellation are mandatory for hydration.
- User-requested hydration has higher priority than background sync.
