# Dokan.NET Binding
[![Build status](https://ci.appveyor.com/api/projects/status/w707j7xlu21jf3qa?svg=true)](https://ci.appveyor.com/project/Liryna/dokan-dotnet)
[![NuGet downloads](https://img.shields.io/nuget/dt/DokanNet.svg)](https://www.nuget.org/packages/DokanNet)
[![Version](https://img.shields.io/nuget/v/DokanNet.svg)](https://www.nuget.org/packages/DokanNet)

## What is Dokan.NET Binding
By using Dokan library, you can create your own file systems very easily
without writing device driver. Dokan.NET Binding is a library that allows
you to make a file system on .NET environment.

## Install

To install DokanNet, run the following command in the Package Manager Console

    PM> Install-Package DokanNet
    
    //Prerelease 
    PM> Install-Package DokanNet -Pre 

## Licensing
Dokan.NET Binding is distributed under a version of the "MIT License",
which is a BSD-like license. See the 'license.mit.txt' file for details.

## Environment
* Either of Microsoft .NET Framework 4.6, .NET Framework 4.8, .NET Standard 2.0, .NET Standard 2.1, .NET 8.0 or .NET 9.0
* Dokan library

## How to write a file system
To make a file system, an application needs to implement IDokanOperations interface, or the modernized variant, IDokanOperations2.
Once implemented, you can invoke Mount function on your driver instance
to mount a drive. The function blocks until the file system is unmounted.
Semantics and parameters are just like Dokan library. Details are described
at `README.md` file in Dokan library. See sample codes under 'sample'
directory.
Doxygen documentation is also available [![API documentation](https://img.shields.io/badge/Documentation-API-green.svg)](https://dokan-dev.github.io/dokan-dotnet-doc/html/)

## Unmounting
Just run the bellow command or your file system application call Dokan.Unmount
to unmount a drive.

   > dokanctl.exe /u DriveLetter

