# Arc Explorer [![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/ScanMountGoat/ArcExplorer?include_prereleases)](https://github.com/ScanMountGoat/ArcExplorer/releases/latest) [![Github All Releases](https://img.shields.io/github/downloads/ScanMountGoat/ArcExplorer/total.svg)](https://github.com/ScanMountGoat/ArcExplorer/releases/latest)
![image](https://user-images.githubusercontent.com/23301691/218875745-058955f7-9bb5-4ce5-9e22-821ccd338cfb.png)

A file browser and extractor for Smash Ultimate's data.arc file for Windows, MacOS, and Linux. The latest version of the program can be downloaded from [releases](https://github.com/ScanMountGoat/ArcExplorer/releases). Report any bugs or request new features in [issues](https://github.com/ScanMountGoat/ArcExplorer/issues). 

## Opening the data.arc
Open an ARC file using File > Open ARC or Ctrl + O on the keyboard. The program supports any ARC file from Smash Ultimate version 4.0.0 (Hero DLC) or later. Use ArcCross (Windows only) for opening very old ARC versions. 

## Opening Over The Network
The program also supports opening an ARC file over the network by connecting to the Switch console using the Switch's IP address. All program features work normally when connecting to the ARC file over the network but file extraction will likely be slower. 

1. Install Skyline if you haven't already
2. Download [the companion skyline plugin, arc-network](https://github.com/jam1garner/arc-network/releases/tag/master) and install it as you would any other skyline plugin
3. Run Smash and keep it open
4. Click File > Connect to ARC in ArcExplorer
5. Input your Switch's IP address. This can be found under System Settings > Internet on the Switch itself.
6. Click Connect

## Navigation
Navigating the ARC file can be done with the mouse or the keyboard. Double click a folder to open it. Click the "Exit Folder" button to exit the current folder and go the parent folder. 

The textbox above the file list shows the current folder. Modify the path in the textbox to navigate directly to the desired folder. For example, copy paste `fighter/mario/model/body/c00` into the textbox to avoid having to click `fighter` > `mario` > `model` > `body` > `c00` individually.   

Keyboard shortcuts are listed below. 
| Action | Keys |
| --- | --- |
| Enter Selected Folder | Enter, Right Arrow |
| Move Selection Up | Up Arrow | 
| Move Selection Down | Down Arrow |
| Exit Selected Folder | Left Arrow, Alt + Up Arrow |

## Searching
Type paths or parts of a path into the search box to search the entire ARC. This works if the ARC is opened from a file or when connecting over the network. The search is "fuzzy", so small inaccuracies in spelling don' t matter like "mrio" instead of "mario". To clear the search results and return to the ARC root, delete the text in the search bar.

## Updating Hashes
<img src="https://github.com/ScanMountGoat/ArcExplorer/blob/master/hash_update.jpg" align="top" height="auto" width="auto">  
When the program launches, it will check Github for any hash label updates. If an update is available, a dialog will appear with details on the new commit. Click the update button to download and load the new hashes. 
If any errors occur, check the log for details.  

## Application Log
If the application encounters an error, click the red error icon to see details on the error. Application errors, warnings, and additional performance information are logged to the `log.txt` file in the executable directory. If an extract operation fails or the data.arc won't open, check the `log.txt` for details. Include the relevant lines from the `log.txt` when reporting issues if possible. 

## File Details Panel
The right panel contains details on the currently selected file or folder. If a file is selected, the details panel displays information such as the file name and full path, compressed size, offset, etc. Shared files use the same data, so edits to one of the files will update all the other files shared with it. Shared files have a link icon in the file tree and a corresponding list of files shared with that file in the details view.  

## Extracting Files
Folders or individual files can be extracted by right clicking the folder or file and selecting extract or by clicking extract under the Arc menu. The export directory can be customized under Settings > Preferences.

## User Preferences
Various aspects of the program can be customized in the preferences window by selecting Settings > Preferences in the top menu. The preferences window allows toggling the dark theme, using hexedecimal or decimal for file properties, etc. 

## Building (WIP)
This project contains submodules, so clone with  
`git clone --recurse-submodules https://github.com/ScanMountGoat/ArcExplorer`.  

The smash-arc library is built using cross with the following commands. This improves compatibility compared to building locally and eliminates the need to install runtime components. The downside is increased binary sizes.  
`cross build --target x86_64-unknown-linux-gnu --release --features=libzstd,search,ffi-bindings`  
`cross build --target x86_64-pc-windows-gnu --release --features=libzstd,search,ffi-bindings`  

Build in Visual Studio 2022 or later or using `dotnet build` from terminal. This requires the .NET 6.0 SDK and a recent Rust toolchain. The Rust toolchain can be installed via rustup. 

## Credits
[AvaloniaUI](https://github.com/AvaloniaUI/Avalonia) - cross platform UI Framework  
[SmashArcNet](https://github.com/ScanMountGoat/SmashArcNet) - C# wrapper for smash-arc  
[smash-arc](https://github.com/jam1garner/smash-arc) - Rust lib for interacting with the data.arc  
[ArcCross](https://github.com/Ploaj/ArcCross) - original .Net ARC file browser  
[archive-hashes](https://github.com/ultimate-research/archive-hashes) - hashes for file and directory paths  
Application icons except for the main application icon and some file format specific icons are from the [Visual Studio 2019 Image Library](https://www.microsoft.com/en-us/download/details.aspx?id=35825)
