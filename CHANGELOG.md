# Change Log
## v1.4.4 - 2023-02-14
* Fixed an issue where keyboard navigation and text boxes did not work properly.

## v1.4.2 - 2023-02-13
* Moved the search UI to better indicate that searching works over the entire ARC file.
* Application files are now stored in a user directory rather than the executable directory to work consistently across platforms.
* Added a precompiled binary for MacOS that works on Intel or Apple silicon macs.

## v1.4.2 - 2022-06-23
* Fixed an issue where selecting files with the arrow keys while sorting would select the wrong file.
* Fixed an issue where localized files failed to extract.

## v1.4.1 - 2021-12-27
* Upgraded to .NET 6.0. The releases are now self-contained, which eliminates the need to install any extra components like .NET or C/C++ runtimes. This resolves most issues installing and running the application.
* Fixed an issue where the files failed to clear when opening a new ARC.
* Changed layouts to improve UI consistency.
* Fixed a potential crash when entering invalid paths in the navigation bar.
* Added an option to display file sizes in the file view.
* Fixed an issue where file metadata didn't reflect the currently selected ARC region.
* Updated the included hashes to the latest version.

## v1.4.0 - 2021-07-20
* Added search support. Type paths or parts of a path into the search box to search the entire ARC. This works if the ARC is opened from a file or when connecting over the network. The search is "fuzzy", so small inaccuracies in spelling don' t matter like "mrio" instead of "mario". 
* Added an option to remember the IP address when connecting to the ARC over the network.
* Minor improvements to performance and memory usage
* The application now shows the current version for the main window's title

## v1.3.0 - 2021-07-11
* Fixed an issue where the application failed to find configuration files such as preferences or hashes
* The progress bar now shows the progress percentage when extracting multiple files
* Added an option to open an ARC file on startup. This can be configured in Settings > Preferences.
* Added an option to extract the entire ARC file by selecting ARC > Extract All Files
* Reworked the file viewer to show a list of files and folders in the current directory. Navigation works similar to File Explorer. See the ReadMe for [README](https://github.com/ScanMountGoat/ArcExplorer) for usage instructions. 
* Added a navigation bar at the top. Enter a path to navigate to the specified folder. 
* Added keyboard navigation. See the [README](https://github.com/ScanMountGoat/ArcExplorer) for a list of shortcuts.
* Added an option to Settings > Preferences to merge directories that differ only by a trailing slash. This is enabled by default, so `render/shader/` and `render/shader/` will be merged in the user interface.
* Drastically improved performance when scrolling large lists of files or opening large folders

## v1.2.0 - 2021-03-14
* Added an option to preferences to control whether the application starts maximized
* Added the ability to keep hashes up to date. A dialog will appear if there is an update available. Click update to download the latest hashes file and update the hashes
* Changed formatting for file sizes to now also display kilobytes, megabytes, or gigabytes to more accurately reflect the displayed file size by the OS.

## v1.1.0 - 2021-01-22
* Added support for `stream:` files.
* Added support for regional files. Select a region from the dropdown to choose the region for extracting files. 
* Minor improvements to user interface and file details panel. Uncompressed files now only show the actual size instead of identical values for compressed/uncompressed size.

## v1.0.1 - 2021-01-08
* Fixed exporting files from `Prebuilt:`
* Adjusted output paths for files with missing hashes to use the proper directory structure and include the extension
* Separate downloads for Linux x64 and Windows x64

## v1.0.0 - 2021-01-05
* First public release!
