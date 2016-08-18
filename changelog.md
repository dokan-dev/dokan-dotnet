# Change Log
All notable changes to this project will be documented in this file.
This project adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased]
### Added
- XML comments in code.
- A website with the [documentation](https://dokan-dev.github.io/dokan-dotnet-doc/html/).
- Posibiles to redirect log output.
- Localizated error messages for DE and FR.
- Support to specify UNC name used for network volume.
- Support to specify allocation Unit Size of the volume.
- Support to specify sector Size of the volume.
- Support for ``IDokanOperations.FindFilesWithPattern``.
- Enum ``DokanOptions`` get following new values: ``WriteProtection``, ``MountManager``, ``CurrentSession`` and ``UserModeLock``.
- Enum value ``NtStatus.NotADirectory``.
### Changed
- ``DokanResult.AlreadyExists`` should be returnerd insted of using ``SetLastError(ERROR_ALREADY_EXISTS)`` in ``CreateFile``.
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

## [1.0.6] - 2015-05-23
### Added
- Enum value ``DokanError.Undefined``.
- Enum value ``FileSystemFeatures.None``.

[Unreleased]: https://github.com/dokan-dev/dokan-dotnet/compare/v1.0.8.0...HEAD
[1.0.8]: https://github.com/dokan-dev/dokan-dotnet/compare/1.0.6.0...v1.0.8.0
[1.0.6]: https://github.com/dokan-dev/dokan-dotnet/compare/19fb1d6cdad87fa6185626677fc5f44733af9896...1.0.6.0