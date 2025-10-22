# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/) 
and this project adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased]

## [2.3.0.3] - 2025-10-22
- Library - Brings DokanOptions flags up to date and in sync with changes in native library.
  * The `EnableNotificationAPI` is no longer necessary and has been removed, just like in native library.
  * Several `DokanOptions` flag values have changed, rebuilt implementations to get the correct values!
  * Added missing `DokanOptions.AllowIpcBatching`

## [2.3.0.1] - 2025-05-11

### Changed
- Library - IDokanOperations2 is a new way to implement Dokan file systems in .NET with less CPU and memory pressure.
  * Includes several other minor optimizations as well.
  * Existing `IDokanOperation` and `IDokanOperationUnsafe` implementations get wrapped in a compatibility layer called `DokanOperationsAdapter`, that in turn implements the new `IDokanOperations2` interface.
- Library - Removed "-windows" suffix from target frameworks to allow library to be consumed by applicaitons that chose between Windows and other OS implementations at runtime.
- Library - Removed .NET Framework 4.0 from target frameworks and added 4.6 and 4.8 (last supported on Windows Vista and last supported on Windows 7).
- Library - Added some logic to keep a reference to `DokanInstance` and associated operations alive while file system is mounted, to avoid crashes in native code if the last reference is collected by GC.

### Added
- Sample - New DokanNetMirror with `IDokanOperations2`. Old `DokanNetMirror` moved to ``DokanNetMirrorLegacy`.

## [2.3.0.0] - 2025-04-26

### Changed
- Library - `IDokanFileInfo.DeleteOnClose` was renamed `IDokanFileInfo.DeletePending`. Same expectation (remove the object) but is set when last handle on the object is being closed. See [here](https://github.com/dokan-dev/dokany/issues/883).

## [2.2.1.0] - 2025-03-29

### Added
- Library - Allow passing a date time format info for console and debug logger

### Changed
- Library - `Read` - Only copy the actual read data by the operation to the destination buffer
- Library - Hold `Dokan` reference in `DokanInstance` as it cannot live without it. Avoid impromptu native resources being released.

## [2.1.0.0] - 2023-12-22

### Added
- Library - New API `WaitForFileSystemClosedAsync` that wait async for the FileSystem to unmount. It requires Dokany 2.1.0

## [2.0.5.2] - 2023-09-28

### Changed
- Library - Add `GenerateDocumentationFile` to project file to make it generate documentation XML file in the Nuget package.

### Fixed
- Library - Crash in `ReadFileProxy` when debug logging is enabled.
- Mirror - Fixes a bug where certain pieces of software (Libre office/Free office) try to open a filestream with `FileMode.CreateNew` and `FileShare.Read` which throws an IO exception.

## [2.0.5.1] - 2022-07-04

### Changed
- Library - It is now again possible to return `NotImplemented` for `FindFilesWithPattern` to force `FindFiles` usage instead. See [here](https://github.com/dokan-dev/dokany/wiki/Update-Dokan-1.1.0-application-to-Dokany-2.0.0#moving-from-dokan-2xx-to-205)

## [2.0.4.1] - 2022-04-30

### Changed
- Moved .net46 to .net 462 and added .net6.0-window in the TFM.
- Use fluent API to configure how dokan should behave and avoid blocking apis.
- Deduplicate code of `Mount` and `CreateFileSystem`.

### Fixed
- Support pagingIo in mirror.net
- Unsafe mirror yields wrong file content using .net6

## [2.0.1.1] - 2022-02-20

### Fixed
- Library - Avoid GC of delegates in optimized builds
- RegistryFS - Skip empty name keys

## [2.0.1.0] - 2022-01-01

### Added
- Support dokany 2.1.0 and the new async mount API.

## [1.5.0.0] - 2021-05-30

### Added
- Add `ILogger.DebugEnabled` to reduce memory allocation produce by logs like `BufferPool`. 

## [1.4.0.0] - 2020-06-02

### Added
- Replace `DokanOptions` of dokany 1.4.0 `OptimizeSingleNameSearch` to `EnableFCBGC`.
- Support for .NET Standard 2.0

## [1.3.0.0] - 2019-10-09

### Added
- Support dokany 1.3.0 `DokanNotify` feature in `Dokan.Notify` class.
- New `DokanOptions` of dokany 1.3.0 `DisableOplocks` and `OptimizeSingleNameSearch`.

### Changed
- Improve Buffer Handling to Reduce GC Pressure.
- Improve `ConsoleLogger` speed.
- `DokanFileInfo` inherit from `IDokanFileInfo`
- `IDokanFileInfo` is now the final parameter for `IDokanOperations` for allowing self-forge of `DokanFileInfo` for testing purpose.

### Fixed
- Avoid `ConsoleLogger` to switch color and never go back to the original due to multi threading.

## [1.1.2.1] - 2018-12-20

### Changed
- Improve Buffer Handling to Reduce GC Pressure.

## [1.1.2.0] - 2018-08-10

### Added
- Included a strongly typed error code in ``DokanException`` to better communicate the reason why mounting failed.
- [``NotADirectory``][1.1.2.0-NotADirectory] enum value to ``DokanResult``.

[1.1.2.0-NotADirectory]: https://github.com/dokan-dev/dokan-dotnet/blob/v1.1.2.0/DokanNet/DokanResult.cs#L110

### Changed
- Status error message is now dispatched by ``DokanException`` itself instead of ``Dokan`` class.
- Replaced usages of ``NtStatus`` with ``DokanResult`` in DokanNetMirror. 

### Fixed
- Leak of the token handle in ``GetRequestor()``
- ``DokanFileInfo.Context`` leaks GCHandle if not set ``null``
- ``DokanMain`` throw wrongly when success

## [1.1.1.1] - 2018-04-25

### Added
- Enum value [``FileAccess.None``][1.1.1.1-FileAccess.None].
- Handle unknown errors from Dokan.

[1.1.1.1-FileAccess.None]: https://github.com/dokan-dev/dokan-dotnet/blob/v1.1.1.1/DokanNet/FileAccess.cs#L23

### Changed
- Make ``MaximumComponentLength`` param available for ``GetVolumeInformation``.

## [1.1.1.0] - 2017-12-01

### Added
- Support for .NET Standard 1.3

### Changed
- Migrate to Visual Studio 2017
- Update the NuGet icon to the new icon 
- Library - Adapt to API changes for dokany 1.1.0 

### Fixed
- Library -Proper use of timeout when mounting (See #144)
- Mirror - Implementation of SetTime to work on open files
- Mirror - Only SetAttributes when attributes not 0 
- Test - Appveyor is now Green !

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

[Unreleased]: https://github.com/dokan-dev/dokan-dotnet/compare/v2.3.0.3...HEAD
[2.3.0.1]:    https://github.com/dokan-dev/dokan-dotnet/compare/v2.3.0.1...v2.3.0.3
[2.3.0.1]:    https://github.com/dokan-dev/dokan-dotnet/compare/v2.3.0.0...v2.3.0.1
[2.3.0.0]:    https://github.com/dokan-dev/dokan-dotnet/compare/v2.2.1.0...v2.3.0.0
[2.2.1.0]:    https://github.com/dokan-dev/dokan-dotnet/compare/v2.1.0.0...v2.2.1.0
[2.1.0.0]:    https://github.com/dokan-dev/dokan-dotnet/compare/v2.0.5.2...v2.1.0.0
[2.0.5.2]:    https://github.com/dokan-dev/dokan-dotnet/compare/v2.0.5.1...v2.0.5.2
[2.0.5.1]:    https://github.com/dokan-dev/dokan-dotnet/compare/v2.0.4.1...v2.0.5.1
[2.0.4.1]:    https://github.com/dokan-dev/dokan-dotnet/compare/v2.0.1.1...v2.0.4.1
[2.0.1.1]:    https://github.com/dokan-dev/dokan-dotnet/compare/v2.0.1.0...v2.0.1.1
[2.0.1.0]:    https://github.com/dokan-dev/dokan-dotnet/compare/v1.5.0.0...v2.0.1.0
[1.5.0.0]:    https://github.com/dokan-dev/dokan-dotnet/compare/v1.4.0.0...v1.5.0.0
[1.4.0.0]:    https://github.com/dokan-dev/dokan-dotnet/compare/v1.3.0.0...v1.4.0.0
[1.3.0.0]:    https://github.com/dokan-dev/dokan-dotnet/compare/v1.1.2.1...v1.3.0.0
[1.1.2.1]:    https://github.com/dokan-dev/dokan-dotnet/compare/v1.1.2.0...v1.1.2.1
[1.1.2.0]:    https://github.com/dokan-dev/dokan-dotnet/compare/v1.1.1.1...v1.1.2.0
[1.1.1.1]:    https://github.com/dokan-dev/dokan-dotnet/compare/v1.1.1.0...v1.1.1.1
[1.1.1.0]:    https://github.com/dokan-dev/dokan-dotnet/compare/v1.1.0.3...v1.1.1.0
[1.1.0.3]:    https://github.com/dokan-dev/dokan-dotnet/compare/v1.1.0.1...v1.1.0.3
[1.1.0.1]:    https://github.com/dokan-dev/dokan-dotnet/compare/v1.1.0.0...v1.1.0.1
[1.1.0.0]:    https://github.com/dokan-dev/dokan-dotnet/compare/v1.0.8.0...v1.1.0.0
[1.0.8]:      https://github.com/dokan-dev/dokan-dotnet/compare/1.0.6.0...v1.0.8.0
[1.0.6]:      http://web.archive.org/web/20150416102451/http://dokan-dev.net/en/2011/01/12/dokan-net-library-0-6-0-released/
