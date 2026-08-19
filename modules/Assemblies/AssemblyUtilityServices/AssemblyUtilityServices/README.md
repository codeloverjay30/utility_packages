# AssemblyUtilityServices

Utilities for discovering, loading, and reading metadata from .NET assemblies.

## 2.1.0-preview-1.0.0

### Changed

- Replaced regex-based informational-version parsing with `NuGet.Versioning`.
- Removed the dependency on `RegexUtilityServices`.
- `GetInformationalVersion()` now returns only `AssemblyInformationalVersionAttribute`; it no longer silently falls back to `AssemblyName.Version`.
- Added `GetAssemblyVersion()` for assembly identity versions (`System.Version`).
- Added explicit informational-version and assembly-identity version matchers.
- Changed path-based assembly loading to use `Assembly.LoadFrom`.
- Added `System.IO.Abstractions` injection for deterministic file-system tests.
- Added input validation and deterministic path ordering.

### Compatibility

`_IsValidVersion()` is retained temporarily and marked obsolete. Migrate to
`IsValidInformationalVersion()` before the next major version.
