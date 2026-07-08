# Legacy MountDock Feature Parity Plan

## Purpose

`mountdock-cloud` is a Cloud Files API based successor to the existing Python/PyQt `mountdock` app. This document tracks which existing MountDock features must be preserved, reinterpreted, deferred, or dropped while moving from `rclone mount` to OneDrive-style placeholders and hydration.

Reference repository inspected:

```text
/root/mountdock
```

Key reference files:

```text
README.md
main.py
mountdock/config_manager.py
mountdock/rclone_engine.py
mountdock/watcher.py
mountdock/ui_components.py
mountdock/i18n.py
mountdock/rclone_updater.py
mountdock/app_updater.py
mountdock/google_auth.py
mountdock/sync_service.py
mountdock/google_drive_sync.py
mountdock/secure_store.py
mountdock/crypto_utils.py
mountdock/drive_icons.py
mountdock/windows_drive_icons.py
```

## Parity Strategy

The goal is **feature parity at the user workflow level**, not a line-by-line port.

Existing MountDock is:

```text
Python/PyQt UI -> rclone mount -> WinFsp fixed/network drive
```

MountDock Cloud target is:

```text
C#/.NET UI/helper -> Windows Cloud Files API sync root/placeholders -> rclone transfer backend
```

So features are mapped by intent:

- Mount/unmount becomes register/unregister/connect/disconnect sync roots.
- VFS cache settings become placeholder/hydration/cache settings.
- Explorer drive open becomes sync root open.
- Watcher reconnect becomes provider/helper health monitoring.
- rclone.conf backup/restore remains a valid feature.

## Status Legend

| Status | Meaning |
|---|---|
| Keep | Preserve essentially the same user-facing behavior |
| Reinterpret | Preserve intent but implement differently for Cloud Files |
| Defer | Important but not required for first CfAPI spike/MVP |
| Drop | Not applicable to Cloud Files mode |
| Investigate | Needs Windows validation before committing |

## Feature Parity Matrix

### Core app and profile management

| Legacy feature | Reference | Cloud status | Cloud implementation notes | Priority |
|---|---|---|---|---|
| Multiple drive/profile management | `README.md`, `ConfigManager.get_profiles`, `add_profile`, `update_profile`, `delete_profile` | Keep | Store profiles as Cloud profiles with `profile_id`, `display_name`, `remote`, `root_folder`, `sync_root_path`, `backend`. | P0 |
| Profile add/edit/delete UI | `DriveSettingsDialog`, `main.handle_add_drive`, `handle_edit_drive`, `handle_delete_drive` | Keep | Rebuild in C# UI. Replace VFS fields with Cloud Files fields. | P1 |
| Profile card dashboard | `DriveCardWidget`, `LDriveMainWindow` | Keep | Show sync root status, queued ops, conflicts, online/offline state. | P1 |
| Bulk connect/disconnect | `handle_mount_all`, `handle_unmount_all`, `BulkActionDialog` | Keep | Bulk register/connect providers and bulk disconnect helper sessions. | P2 |
| Single instance guard | `QSharedMemory` in `main.py` | Keep | Use .NET mutex/single-instance guard. | P1 |
| Device ID | `ConfigManager.device_id` | Keep | Needed for conflict names, encrypted backup metadata, telemetry-free device identity. | P1 |

### rclone configuration and remote discovery

| Legacy feature | Reference | Cloud status | Cloud implementation notes | Priority |
|---|---|---|---|---|
| rclone path setting | `ConfigManager.resolve_rclone_path` | Keep | Implement in C# settings; prefer bundled `rclone.exe`, then configured path. | P0 |
| rclone.conf path setting | `resolve_rclone_conf_path`, `find_default_rclone_conf` | Keep | Same discovery: managed `.mountdock/rclone.conf`, app dir, `%APPDATA%`, user config. | P0 |
| Import/manage rclone.conf | `import_rclone_conf`, `get_rclone_conf_store_path` | Keep | Preserve managed config storage; do not expose secrets in logs. | P1 |
| Parse rclone.conf remotes | `parse_rclone_conf` | Keep | Use .NET INI parser or simple parser. Needed for fast remote list. | P0 |
| `rclone listremotes` fallback | `RcloneEngine.get_remotes` | Keep | Implement via `RcloneClient`; no shell execution. | P0 |
| Remote type detection | `parse_rclone_conf` returns `type` | Keep | Use for icon/default behavior and capability hints. | P1 |
| Interactive rclone config UI | `RcloneConfigDialog`, `RcloneEngine.start_config_session` | Defer | Can be reimplemented as terminal-like process UI after provider spike. | P3 |
| Custom rclone mount args | `custom_args`, `extra_flags` | Reinterpret | No raw mount args in Cloud mode. Add controlled advanced args for list/transfer operations. | P2 |

### Mount / connect behavior

| Legacy feature | Reference | Cloud status | Cloud implementation notes | Priority |
|---|---|---|---|---|
| Connect drive | `RcloneEngine.mount`, `handle_toggle_mount` | Reinterpret | Register/start Cloud Files sync root and provider helper. | P0 |
| Disconnect drive | `RcloneEngine.unmount`, `handle_toggle_mount(False)` | Reinterpret | Stop provider session; choose whether to unregister sync root; never delete cached data silently. | P1 |
| Drive letter | profile `letter`, rclone mount target | Investigate | CfAPI is sync-root-first. Validate `subst` behavior separately. Do not promise drive-letter parity until tested. | P1 |
| Open mounted drive in Explorer | `handle_open_drive` | Keep | Open sync root folder in Explorer. If drive-letter mode works, open that. | P1 |
| Mount on launch | `mount_on_launch`, `_mount_startup_profiles` | Keep | Auto-connect Cloud profiles on app start. | P2 |
| Auto mount per profile | `auto_mount` | Keep | Rename/keep as `auto_connect`. | P2 |
| Watcher reconnect | `LDriveWatcher` | Reinterpret | Provider/helper health monitor, sync root status, operation queue retry. | P2 |
| WinFsp requirement | README, `_ensure_winfsp_ready` | Drop for Cloud mode | CfAPI does not require WinFsp. Live legacy mount mode may still require it if retained. | P0 |
| Admin mount block | admin warning in `main.py` | Reinterpret | Check provider registration permissions and per-user sync root constraints. Admin warning may differ. | P2 |

### Cloud Files specific replacements

| Legacy concept | Cloud replacement | Notes | Priority |
|---|---|---|---|
| `vfs_mode` | Hydration mode / online-only default | Cloud mode uses placeholder state, not rclone VFS. | P1 |
| `cache_dir` | sync root path + content cache + metadata DB | Naming must change to avoid VFS confusion. | P1 |
| `--network-mode` | Not needed in Cloud mode | Keep only as legacy live mount compatibility option if live mode remains. | P3 |
| rclone VFS cache | Cloud Files placeholder cache | Hydrated files are local content managed by provider. | P1 |
| mount process alive | provider helper alive + sync root registered | Monitor provider process and CfAPI state. | P1 |

### Explorer and file UX

| Legacy feature | Reference | Cloud status | Cloud implementation notes | Priority |
|---|---|---|---|---|
| Explorer opens files | rclone mount filesystem | Keep | Use online-only placeholders; hydrate on open/copy/read. | P0 |
| Explorer icons for remote type | `drive_icons.py`, `windows_drive_icons.py` | Reinterpret | Use sync root icon and Cloud Files states; custom overlay may require shell integration. | P2 |
| Drive card hover/double-click | README, `DriveCardWidget` | Keep | Same UX; opens sync root. | P2 |
| File status icons | not present except drive icon | New | Online-only, hydrating, available offline, pinned, syncing, error, conflict. | P0/P1 |
| Pin/offline keep | not present | New | Cloud Files pin state. | P1 |
| Free up space/dehydrate | not present | New | Cloud Files dehydration. | P1 |
| Conflict visibility | not present as first-class | New | Required for safe write-back. | P2 |

### Google Drive encrypted rclone.conf backup/restore

| Legacy feature | Reference | Cloud status | Cloud implementation notes | Priority |
|---|---|---|---|---|
| Google OAuth sign-in | `google_auth.py` | Keep | Reimplement in C# or reuse OAuth flow; appDataFolder scope remains useful. | P3 |
| Encrypted rclone.conf backup | `sync_service.py`, `crypto_utils.py`, `google_drive_sync.py` | Keep | Preserve security model: passphrase encrypts config before upload. | P3 |
| Restore with backup of existing config | `SyncService.restore_conf` | Keep | Same behavior; back up local config before replacing. | P3 |
| Remember passphrase on device | `secure_store.py` | Keep | Use Windows Credential Manager / DPAPI in .NET. | P3 |
| Auto backup on rclone.conf change | `QFileSystemWatcher`, `_run_auto_google_sync` | Defer | Useful after config management is stable. | P4 |

### Updates and distribution

| Legacy feature | Reference | Cloud status | Cloud implementation notes | Priority |
|---|---|---|---|---|
| rclone updater | `rclone_updater.py` | Keep | Reimplement: check latest rclone release, download Windows amd64 zip, handle locked executable. | P3 |
| MountDock app updater | `app_updater.py` | Reinterpret | Update GitHub repo/release URLs to `mountdock-cloud`; installer flow likely changes. | P4 |
| Update badge | `handle_settings`, version status | Keep | Useful but later. | P4 |
| Portable layout | README, `get_app_dir` | Keep | C# app should support app-relative config/log/rclone for portable install. | P3 |
| Installer EXE | release scripts | Defer | Decide after CfAPI registration/unregistration requirements are known. | P4 |

### UI and settings

| Legacy feature | Reference | Cloud status | Cloud implementation notes | Priority |
|---|---|---|---|---|
| Korean/English UI | `i18n.py` | Keep | Port strings to `.resx` or JSON resource files. | P2 |
| Theme light/dark | `theme`, UI styles | Keep | Rebuild in WPF/WinUI if GUI is added. Not needed for headless spike. | P4 |
| Tray start/minimize/restore | `LDriveTrayIcon`, settings | Keep | Use WPF/WinForms tray icon. | P3 |
| Start minimized | config | Keep | Same user setting. | P3 |
| Minimize to tray | config | Keep | Same user setting. | P3 |
| Auto start registry | `ConfigManager.set_auto_start` | Keep | Use HKCU Run key or StartupTask depending package model. | P3 |
| Settings dialog | `GlobalSettingsDialog` | Keep | Rebuild for Cloud settings. | P3 |
| Log view | `append_log`, `logs/app.log` | Keep | Use structured logs and UI log panel. | P2 |

### Safety and reliability

| Legacy feature | Reference | Cloud status | Cloud implementation notes | Priority |
|---|---|---|---|---|
| Hidden subprocess windows | `_hidden_subprocess_kwargs` | Keep | Use `CreateNoWindow=true`, `UseShellExecute=false`. Already scaffolded in `RcloneClient`. | P0 |
| No shell execution | rclone command lists | Keep | Required. All rclone calls use argument lists. | P0 |
| Process lifecycle tracking | `RcloneEngine._active_mounts` | Reinterpret | Track provider helper and rclone transfer processes. | P1 |
| Retry/backoff | `LDriveWatcher._handle_reconnect` | Reinterpret | Operation queue retries and helper restart policy. | P2 |
| Non-destructive behavior | restore backup before overwrite | Keep/Expand | Applies strongly to sync conflicts and delete propagation. | P0 |
| Sensitive data handling | encrypted backup, no plaintext Drive storage | Keep | Also no token/secret logging. | P0 |

## New Cloud-only Features Required for True Parity+

These are not in legacy MountDock but are required for the OneDrive-style target.

| Feature | Description | Priority |
|---|---|---|
| CfAPI sync root registration | Register profile folder as Windows sync root | P0 |
| Placeholder creation | Create online-only file/folder entries from remote metadata | P0 |
| Hydration callback | Download file content only when opened/copied/read | P0 |
| Dehydration | Free local content while preserving placeholder | P1 |
| Pin/unpin | Always keep on device / online-only intent | P1 |
| Explorer sync states | Online-only, hydrating, offline, pinned, error, conflict | P1 |
| Metadata DB | Track remote/local state, pin, operation queue | P1 |
| Conflict engine | Preserve both versions; no silent overwrite | P2 |
| Lazy directory listing | Avoid full recursive listing for huge remotes | P2 |
| Provider IPC | GUI/service to provider helper contract | P1 |

## Proposed Cloud Profile Schema

Initial target schema:

```json
{
  "profile_id": "uuid",
  "display_name": "Company Docs",
  "backend": "cloud_files",
  "remote": "gdrive",
  "root_folder": "/CompanyDocs",
  "sync_root_path": "%USERPROFILE%\\MountDock\\Company Docs",
  "icon": "auto",
  "auto_connect": false,
  "online_only_by_default": true,
  "allow_pin_offline": true,
  "allow_dehydrate": true,
  "remote_poll_interval_sec": 300,
  "conflict_policy": "keep_both",
  "advanced_rclone_global_args": [],
  "advanced_rclone_list_args": [],
  "advanced_rclone_transfer_args": []
}
```

Legacy profile migration notes:

| Legacy field | Cloud field | Rule |
|---|---|---|
| `id` | `profile_id` | Preserve if importing existing config |
| `remote` | `remote` | Preserve |
| `root_folder` | `root_folder` | Preserve |
| `letter` | optional `drive_letter` | Store for future drive-letter validation only |
| `volname` | `display_name` | Prefer `volname`, fallback to remote |
| `icon` | `icon` | Preserve |
| `auto_mount` | `auto_connect` | Preserve intent |
| `vfs_mode` | none | Not applicable |
| `cache_dir` | `sync_root_path` or content cache | Ask user or migrate conservatively |
| `custom_args` / `extra_flags` | advanced rclone args | Do not blindly migrate to all operations |

## MVP Priority Order

### P0: Must have before Cloud Files spike is meaningful

- rclone path/config discovery plan
- CfAPI sync root spike
- placeholder + hydration spike
- command builder safety
- no secret logging
- feature parity document maintained

### P1: First real read-only MVP

- profile schema
- remote list discovery
- sync root management
- placeholder creation from remote metadata
- hydration from rclone `copyto`
- status/error reporting

### P2: Safe daily-use MVP

- upload queue
- conflict detection
- lazy directory listing
- provider health monitoring
- basic tray/status UI
- Korean/English strings

### P3: Legacy convenience parity

- rclone config UI
- Google encrypted rclone.conf backup/restore
- rclone updater
- auto start / start minimized / minimize to tray

### P4: Release polish

- app updater
- installer
- update badge
- full theme polish
- portable distribution docs

## GitHub Issue Mapping

Existing issues:

| Issue | Covers |
|---|---|
| #1 Spike: prove Windows Cloud Files API placeholder hydration | P0 Cloud Files feasibility |
| #2 Implement rclone backend command runner | P0/P1 rclone backend |
| #3 Design and implement sync metadata database | P1/P2 state tracking |
| #4 Define provider IPC contract | P1 helper/service boundary |
| #5 Validate Explorer sync root vs drive-letter behavior | drive-letter parity investigation |

Suggested additional issues:

- Port legacy rclone config discovery and remote parsing.
- Define Cloud profile schema and legacy config import.
- Port Korean/English localization inventory.
- Design Google encrypted rclone.conf backup parity.
- Design updater parity for rclone and MountDock Cloud.

## Open Questions

1. Should `mountdock-cloud` include a legacy live mount mode, or leave that entirely to the original `mountdock` app?
2. Does Windows Cloud Files API status and hydration work correctly when the sync root is exposed through `subst` as a drive letter?
3. Should the first GUI be WPF, WinUI, or a minimal tray-only WinForms shell?
4. Can Google encrypted backup be delayed until after Cloud Files read/write MVP without hurting user adoption?
5. How much of legacy `rclone config` interactive UI is needed if users can import an existing `rclone.conf`?

## Decision Record

- Preserve legacy user workflows, not legacy internals.
- Do not use `rclone mount` for Cloud mode.
- Do not blindly port `custom_args` because mount flags can be unsafe for `copyto`/`lsjson`.
- Keep original `mountdock` as the live-mount reference/fallback project.
- Treat drive-letter parity as an investigation, not a promise, until Windows CfAPI validation passes.
