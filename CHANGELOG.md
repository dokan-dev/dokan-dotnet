# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/) 
and this project adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased]
### Added
- This CHANGELOG.md.
- Support for .NET Framework 4.6
- XML comments in code.
- A website with the [documentation](https://dokan-dev.github.io/dokan-dotnet-doc/html/).
- Possibility to redirect log output.
- Localized error messages for German, French and Swedish.
- Support to specify UNC name used for network volume.
- Support to specify allocation Unit Size of the volume.
- Support to specify sector Size of the volume.
- Support for ``IDokanOperations.FindFilesWithPattern``.
- Enum ``DokanOptions`` get following new values: ``WriteProtection``, ``MountManager``, ``CurrentSession`` and ``UserModeLock``.
- Enum value ``NtStatus.NotADirectory``.

### Changed
- ``DokanResult.AlreadyExists`` should be returned instead of using ``SetLastError(ERROR_ALREADY_EXISTS)`` in ``CreateFile``.
- Updated ``DokanFileInfo`` to support for unknown creation, access and modification time.
 
## [1.0.8] - 2015-12-09
### Added
- Support for timeout.
- ``IDokanOperations.FindStreams``.
- ``IDokanOperations.Mounted``.
- ``DokanResult`` that replaces enum ``DokanError``.
- Enum value ``FileAccess.Reserved``.
- Support for ``DokanMapKernelToUserCreateFileFlags``.
- Enum ``NtStatus``.

### Changed
- Renamed ``IDokanOperations.Unmount`` to ``Unmounted``.
- ``IDokanOperations.CreateFile`` get responsible for Directory to.

### Removed
- Removed enum ``DokanError`` that are replaced with class ``DokanResult``.
- Removed ``IDokanOperations.OpenDirectory`` that are replaced with ``IDokanOperations.CreateFile``.
- Removed ``IDokanOperations.OpenDirectory`` that are replaced with ``IDokanOperations.CreateFile``.

## [1.0.6] - 2011-01-12
Latest Dokan version from Hiroki Asakawa.
See the [release note](http://web.archive.org/web/20150416102451/http://dokan-dev.net/en/2011/01/12/dokan-net-library-0-6-0-released/) and [source code](https://code.google.com/archive/p/dokan/source/default/source).

### Added
- ``DokanOptions.Version``, ``DokanOptions.RemovableDrive`` and ``DokanOptions.MountPoint``
- ``DokanRemoveMountPoint``

[Unreleased]: https://github.com/dokan-dev/dokan-dotnet/compare/v1.0.8.0...HEAD
[1.0.8]: https://github.com/dokan-dev/dokan-dotnet/compare/1.0.6.0...v1.0.8.0
[1.0.6]: http://web.archive.org/web/20150416102451/http://dokan-dev.net/en/2011/01/12/dokan-net-library-0-6-0-released/