
=== CodeFluent Runtime Client ===

The CodeFluent Runtime Client is a library of utilities which is usable across all types of .NET applications (WPF, WinForms, ASP.NET, console, service, etc.) and aims to ease the developer's life.


== Classes of interest ==

Please note that the following list of utilities is not exhaustive and we encourage you to browse the library using tools as Visual Studio's Object Browser or Reflector to find more. Nonetheless, here are the major ones:

 * AssemblyUtilities: represents a set of reflection utilities, e.g. getting assembly paths, getting attributes or members, or parsing assembly qualified names.
 * Authenticode: a utility class to sign file using authenticode and check if files are signed using in-process code, without using the signcode.exe or signtool.exe tools.
 * AutoObject: a utility class to implement objects with typed properties without private fields. Furthermore this class supports automatically property change notifications and error validations.
 * BitsServer: a Backgroung Intelligent Transfer Service (BITS) implementation independent from IIS. Supports range download and upload (BITS Upload Protocol 1.5).
 * CabFile: a utility to open and save .CAB files from physical file or in memory streams. CAB files can also be signed using authenticode.
 * ChunkedMemoryStream: a class to manipulate chunked memory streams.
 * CommandLineUtilities: utilities to parse command lines, extremely handy to create console applications.
 * CompoundFile & CompoundStorage: classes to open and write OLE compound files.
 * ContentType: represents a set of MIME content type utilities.
 * ConvertUtilities: provides conversion utilities from one type to another with default values.
 * Country: a utility to retrieve all countries or locations.
 * Crc16, Crc32: classes to compute Cyclic Redundancy Check 16 & 32
 * CustomThreadPool: provides a pool of threads that can be used to post work items and which is specific to your process. Customizable COM apartment state, culture info and processor priority per thread.
 * DistinctDictionary<T>: provides a class for a collection whose keys are the same as values. It is used for computing distinct values from a collection. 
 * DiffUtilities: a helper class to compute differences between two versions of a text or a text file.
 * DockPanel: a simple dock panel control for WinForms.
 * EditableUri: provides an editable object representation of a uniform resource identifier (URI) and easy access to the parts of the URI.
 * GrammaticalNumberConverter: provides plural to singular and singular to plural conversion of nouns in a specific culture. This implementation is not strictly a general inflector, as it's been designed for table name conversion. For instance, it doesn't change a table named "People" in a table named "Person".
 * IOUtilities: provides IO utilites, e.g. wrapping sharing violations and automatically re-trying operation a specific number of times, or automatically creating a directory if it does not exist.
 * JobObject: a utility class to manipulate Windows Job Objects (limit CPU, memory, etc.)
 * JsonUtilities: a light yet powerful JSON serializer/deserializer supporting dates, generics, the object type, as well as all classic features.  
 * NtfsAlternateStream: a utility class to read NTFS alternate streams data. 
 * PerceivedType : provides a programmatic access to Windows "perceived type" concept.
 * Privilege: a utility class to enable or disable Windows security privileges (SeTakeOwnershipPrivilege, SeRestorePrivilege, SeBackupPrivilege, etc.).
 * RibbonControl: a simple ribbon control for WinForms.
 * RtfReader: a utility class that allows to parse an RTF input stream.
 * ScriptEngine: a utility class that allows .NET code to use an ActiveX scripting engine (IE's javascript, IE9+'s chakra, VBScript, etc.)
 * SingleInstance: a utility class to ensure only one instance of a WPF or Winforms application is running.
 * Template: a full blown Javascript template engine. Supports RTF files.
 * UITypeEditors & TypeConverters: numerous UITypeEditors and TypeConverters to edit specific types in property grids (EnumEditor with flag support, CultureComboBox, OpenWithEditor, etc.).
 * UniversalConverter: a pretty generic WPF IValueConverter that focuses on declarative XAML. Alleviates the need to write custom .NET code for WPF converters.
 * WizardForm: a Form class to create visual wizard using Windows Forms. Also ships with associated pre-built pages (progress, configuration, report, summary, etc.).
 * XmlLineInfoDocument: an XmlDocument that stores line information for elements.
 * XmlUtilities: provides a set of XML utilities, e.g. get/set attributes using default values.
 * ZipFile: a simple yet powerful and efficient class to zip/unzip archives.
   Note: Using the ZipFile class requires you to place the "CodeFluent.Runtime.Compression.dll" assembly (native DLL shipped within this NuGet package) in the output directory.
   Use the x86 or x64 version depending on your target platform.

For more information you can:
 - Visit our website (http://www.softfluent.com),
 - Read our product blog (http://blog.codefluententities.com) and company blog (http://blog.softfluent.com),
 - Contact us via our forums (http://forums.softfluent.com).

 
Copyright (C) 2005-2015 SoftFluent S.A.S.
All rights reserved.