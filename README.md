# Arc Explorer
A file browser for Smash Ultimate's data.arc file for Windows and Linux.

## Installation
Download and install the Microsoft [.NET Core 3.1 desktop runtime](https://dotnet.microsoft.com/download/dotnet-core/3.1/runtime/?utm_source=getdotnetcore&utm_medium=referral
) if you haven't already. Make sure to install the version to run desktop apps.

The latest version of the program can be downloaded from [releases](https://github.com/ScanMountGoat/ArcExplorer/releases). Report any bugs in [issues](https://github.com/ScanMountGoat/ArcExplorer/issues). 

On Windows, open the program by double clicking ArcExplorer.exe. On Linux, open a terminal in the executable directory and run `dotnet ArcExplorer.dll`. Mac OS support isn't planned at this time, but it should be possible to build ArcExplorer, SmashArcNet, and smash-arc for Mac OS from source. 

## Opening the data.arc
Open an ARC file using File > Open ARC or Ctrl + O on the keyboard. The program supports any ARC file from Smash Ultimate version 4.0.0 (Hero DLC) or later. The program also supports opening an ARC file over the network by connecting to the Switch console using the Switch's IP address. All program features work normally when connecting to the ARC file over the network but expect program performance to be slower. Use ArcCross (Windows only) for opening very old ARC versions. 

## Application Log
If the application encounters an error, click the red error icon to see details on the error. Application errors, warnings, and additional performance information are logged to the `log.txt` file in the executable directory. If an extract operation fails or the data.arc won't open, check the `log.txt` for details. Include the relevant lines from the `log.txt` when reporting issues if possible. 

## File Details Panel
The right panel contains details on the currently selected file or folder. If a file is selected, the details panel displays information such as the file name and full path, compressed size, offset, etc. Shared files use the same data, so edits to one of the files will update all the other files shared with it. Shared files have a link icon in the file tree and a corresponding list of files shared with that file in the details view.  

## Extracting Files
Folders or individual files can be extracted by right clicking the folder or file and selecting extract or by clicking extract under the Arc menu. The export directory can be customized under Settings > Preferences.

## User Preferences
Various aspects of the program can be customized in the preferences window by selecting Settings > Preferences in the top menu. The preferences window allows toggling the dark theme, using hexedecimal or decimal for file properties, etc. 

## Building
Build in Visual Studio 2019 or later or using `dotnet build` from terminal. Requires the .NET Core 3.1 SDK. The Rust lib [smash-arc](https://github.com/jam1garner/smash-arc) is downloaded and built separately using `cargo build --release --features=libzstd`. The Arc Explorer source contains prebuilt binaries for smash-arc for Windows and Linux. 

## Credits
[AvaloniaUI](https://github.com/AvaloniaUI/Avalonia) - cross platform UI Framework  
[SmashArcNet](https://github.com/ScanMountGoat/SmashArcNet) - C# wrapper for smash-arc  
[smash-arc](https://github.com/jam1garner/smash-arc) - Rust lib for interacting with the data.arc  
[ArcCross](https://github.com/Ploaj/ArcCross) - original .Net ARC file browser
