# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/) 
and this project adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased]
### Added
- Support for .NET Standard 1.3

### Fixed
- Proper use of timeout when mounting (See #144)

## [1.1.0.3] - 2017-03-29
### Added
- [``DokanHelper.DokanIsNameInExpression``][1.1.0.3-DokanIsNameInExpression] to help [``IDokanOperations.FindFilesWithPatter``][1.1.0.3-FindFilesWithPatter] filter the list of possible files.

[1.1.0.3-DokanIsNameInExpression]: https://github.com/dokan-dev/dokan-dotnet/blob/master/DokanNet/DokanHelper.cs#L48
[1.1.0.3-FindFilesWithPatter]:     https://github.com/dokan-dev/dokan-dotnet/blob/master/DokanNet/IDokanOperations.cs#L163

## [1.1.0.1] - 2016-11-01
### Added
- Update documentation for Delete functions according to Dokany changes.

### Fixed
- ``SetFileTimeProxy`` could throw without return a proper error.

## [1.1.0.0] - 2016-09-21
### Added
- This CHANGELOG.md.
- Support for .NET Framework 4.6
- XML comments in code.
- A website with the [documentation](https://dokan-dev.github.io/dokan-dotnet-doc/html/).
- Possibility to redirect log output using the new interface [``ILogger``][1.1.0-ILogger].
- Localized error messages for German, French and Swedish.
- Support to specify UNC name used for network volume using [Dokan.Mount][1.1.0-Mount].
- Support to specify allocation Unit Size of the volume [Dokan.Mount][1.1.0-Mount].
- Support to specify sector Size of the volume [Dokan.Mount][1.1.0-Mount].
- Support for [``IDokanOperations.FindFilesWithPattern``][1.1.0-FindFilesWithPattern].
- Enum [``DokanOptions``][1.1.0-DokanOptions] get following new values: ``WriteProtection``, ``MountManager``, ``CurrentSession`` and ``UserModeLock``.
- Enum ``NtStatus`` get following new value: [``NotADirectory``][1.1.0-NotADirectory].
- Enum [``FileAccess``][1.1.0-FileAccess] get following new values: ``DeleteChild``, ``AccessSystemSecurity``, ``MaximumAllowed`` and ``GenericAll``.

### Changed
- [``DokanResult.AlreadyExists``][1.1.0-AlreadyExists] should be returned instead of using ``SetLastError(ERROR_ALREADY_EXISTS)`` in [``CreateFile``][1.1.0-CreateFile].
- Updated [``FileInformation``][1.1.0-FileInformation] to support for unknown creation, access and modification time by using ``null``.

### Deprecated
- Enum value [``FileAccess.Reserved ``][1.1.0-Reserved]. Use [``FileAccess.AccessSystemSecurity``][1.1.0-AccessSystemSecurity] instead.

[1.1.0-ILogger]:              https://github.com/dokan-dev/dokan-dotnet/blob/v1.1.0.0/DokanNet/Logging/ILogger.cs
[1.1.0-Mount]:                https://github.com/dokan-dev/dokan-dotnet/blob/v1.1.0.0/DokanNet/Dokan.cs#L193
[1.1.0-FindFilesWithPattern]: https://github.com/dokan-dev/dokan-dotnet/blob/v1.1.0.0/DokanNet/IDokanOperations.cs#L161
[1.1.0-DokanOptions]:         https://github.com/dokan-dev/dokan-dotnet/blob/v1.1.0.0/DokanNet/DokanOptions.cs
[1.1.0-NotADirectory]:        https://github.com/dokan-dev/dokan-dotnet/blob/v1.1.0.0/DokanNet/NtStatus.cs#L1349
[1.1.0-FileAccess]:           https://github.com/dokan-dev/dokan-dotnet/blob/v1.1.0.0/DokanNet/FileAccess.cs
[1.1.0-AlreadyExists]:        https://github.com/dokan-dev/dokan-dotnet/blob/v1.1.0.0/DokanNet/DokanResult.cs#L89
[1.1.0-CreateFile]:           https://github.com/dokan-dev/dokan-dotnet/blob/v1.1.0.0/DokanNet/IDokanOperations.cs#L50
[1.1.0-FileInformation]:      https://github.com/dokan-dev/dokan-dotnet/blob/v1.1.0.0/DokanNet/FileInformation.cs
[1.1.0-Reserved]:             https://github.com/dokan-dev/dokan-dotnet/blob/v1.1.0.0/DokanNet/FileAccess.cs#L174
[1.1.0-AccessSystemSecurity]: https://github.com/dokan-dev/dokan-dotnet/blob/v1.1.0.0/DokanNet/FileAccess.cs#L189

## [1.0.8] - 2015-12-09
### Added
- Support for timeout using [``Dokan.Mount``][1.0.8-Mount].
- [``IDokanOperations.FindStreams``][1.0.8-FindStreams].
- [``IDokanOperations.Mounted``][1.0.8-Mounted].
- [``DokanResult``][1.0.8-DokanResult] that replaces enum ``DokanError``.
- Enum value [``FileAccess.Reserved``][1.0.8-Reserved].
- Support for [``DokanMapKernelToUserCreateFileFlags``][1.0.8-MapKernel].
- Enum [``NtStatus``][1.0.8-NtStatus].

### Changed
- Renamed ``IDokanOperations.Unmount`` to [``Unmounted``][1.0.8-Unmounted].
- [``IDokanOperations.CreateFile``][1.0.8-CreateFile] get responsible for directories to.

### Removed
- Removed enum ``DokanError`` that are replaced with class [``DokanResult``][1.0.8-DokanResult].
- Removed ``IDokanOperations.CreateDirectory`` that are replaced with [``IDokanOperations.CreateFile``][1.0.8-CreateFile].
- Removed ``IDokanOperations.OpenDirectory`` that are replaced with [``IDokanOperations.CreateFile``][1.0.8-CreateFile].

[1.0.8-Mount]:       https://github.com/dokan-dev/dokan-dotnet/blob/v1.0.8.0/DokanNet/Dokan.cs#L46
[1.0.8-FindStreams]: https://github.com/dokan-dev/dokan-dotnet/blob/v1.0.8.0/DokanNet/IDokanOperations.cs#L64
[1.0.8-Mounted]:     https://github.com/dokan-dev/dokan-dotnet/blob/v1.0.8.0/DokanNet/IDokanOperations.cs#L60
[1.0.8-DokanResult]: https://github.com/dokan-dev/dokan-dotnet/blob/v1.0.8.0/DokanNet/DokanResult.cs
[1.0.8-Reserved]:    https://github.com/dokan-dev/dokan-dotnet/blob/v1.0.8.0/DokanNet/FileAccess.cs#L24
[1.0.8-MapKernel]:   https://github.com/dokan-dev/dokan-dotnet/blob/v1.0.8.0/DokanNet/Native/NativeMethods.cs#L31
[1.0.8-NtStatus]:    https://github.com/dokan-dev/dokan-dotnet/blob/v1.0.8.0/DokanNet/NtStatus.cs
[1.0.8-Unmounted]:   https://github.com/dokan-dev/dokan-dotnet/blob/v1.0.8.0/DokanNet/IDokanOperations.cs#L62
[1.0.8-CreateFile]:  https://github.com/dokan-dev/dokan-dotnet/blob/v1.0.8.0/DokanNet/IDokanOperations.cs#L10

## [1.0.6] - 2011-01-12
Latest Dokan version from Hiroki Asakawa.
See the [release note](http://web.archive.org/web/20150416102451/http://dokan-dev.net/en/2011/01/12/dokan-net-library-0-6-0-released/) and [source code](https://code.google.com/archive/p/dokan/source/default/source).

### Added
- ``DokanOptions.Version``, ``DokanOptions.RemovableDrive`` and ``DokanOptions.MountPoint``
- ``DokanRemoveMountPoint``

[Unreleased]: https://github.com/dokan-dev/dokan-dotnet/compare/v1.1.0.3...HEAD
[1.1.0.3]:    https://github.com/dokan-dev/dokan-dotnet/compare/v1.1.0.1...v1.1.0.3
[1.1.0.1]:    https://github.com/dokan-dev/dokan-dotnet/compare/v1.1.0.0...v1.1.0.1
[1.1.0.0]:    https://github.com/dokan-dev/dokan-dotnet/compare/v1.0.8.0...v1.1.0.0
[1.0.8]:      https://github.com/dokan-dev/dokan-dotnet/compare/1.0.6.0...v1.0.8.0
[1.0.6]:      http://web.archive.org/web/20150416102451/http://dokan-dev.net/en/2011/01/12/dokan-net-library-0-6-0-released/
