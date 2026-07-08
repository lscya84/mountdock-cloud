# MountDock Cloud TODO

## Milestone 0 — Repository scaffold

- [x] Create new repository skeleton.
- [x] Choose C#/.NET as provider-first language.
- [x] Add architecture docs.
- [x] Add Cloud Files API spike plan.
- [x] Add rclone backend design.

## Milestone 1 — CfAPI feasibility spike

- [x] Verify .NET SDK is available for structural builds/tests in the Hermes environment.
- [ ] Verify Windows build/runtime environment for real CfAPI execution.
- [ ] Implement `CloudFilesApi` P/Invoke signatures for sync root registration.
- [ ] Implement test sync root registration.
- [ ] Implement placeholder creation for one file.
- [ ] Implement hydration callback for one file.
- [ ] Implement dehydrate/free-space debug action.
- [ ] Document results in `docs/CLOUD_FILES_SPIKE_RESULTS.md`.

## Milestone 2 — rclone read-only backend

- [x] Implement `RcloneCommandBuilder` for lsjson/copyto/deletefile/moveto.
- [x] Implement `RcloneClient` process runner with cancellation and stdout/stderr capture.
- [ ] Add explicit timeout wrapper policy around hydration/download operations.
- [ ] List a test remote directory with `lsjson`.
- [ ] Create placeholders from remote metadata.
- [ ] Hydrate remote file content through rclone.

## Milestone 3 — metadata state

- [x] Add SQLite state database.
- [x] Add `items` table.
- [x] Add `operations` table.
- [x] Add conflict table.
- [x] Add tests for basic persistence and state transitions.
- [ ] Add full operation queue helpers and conflict helpers.

## Milestone 4 — write-back safety

- [ ] Detect local dirty files.
- [ ] Queue uploads.
- [ ] Check remote metadata before upload.
- [ ] Preserve both versions on conflict.
- [ ] Avoid automatic destructive delete propagation.

## Milestone 5 — Explorer UX

- [ ] Map Cloud Files states to Explorer-visible status.
- [ ] Add pin/offline support.
- [ ] Add dehydrate/free-space support.
- [ ] Validate drive-letter vs sync-root behavior.

## Open questions

- [ ] Does `subst` preserve Cloud Files placeholder behavior and Explorer status icons?
- [ ] Should the provider be C# only or C# GUI + C++ CfAPI helper?
- [ ] Which Windows versions must be supported?
- [ ] How should installer/register/unregister lifecycle be handled?
