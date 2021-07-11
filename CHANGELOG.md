# Change Log
### Version 1.3.0
* Fixed an issue where the application failed to find configuration files such as preferences or hashes
* The progress bar now shows the progress percentage when extracting multiple files
* Added an option to open an ARC file on startup. This can be configured in Settings > Preferences.
* Added an option to extract the entire ARC file by selecting ARC > Extract All Files
* Reworked the file viewer to show a list of files and folders in the current directory. Navigation works similar to File Explorer. See the ReadMe for [README](https://github.com/ScanMountGoat/ArcExplorer) for usage instructions. 
* Added a navigation bar at the top. Enter a path to navigate to the specified folder. 
* Added keyboard navigation. See the [README](https://github.com/ScanMountGoat/ArcExplorer) for a list of shortcuts.
* Added an option to Settings > Preferences to merge directories that differ only by a trailing slash. This is enabled by default, so `render/shader/` and `render/shader/` will be merged in the user interface.
* Drastically improved performance when scrolling large lists of files or opening large folders

### Version 1.2.0
* Added an option to preferences to control whether the application starts maximized
* Added the ability to keep hashes up to date. A dialog will appear if there is an update available. Click update to download the latest hashes file and update the hashes
* Changed formatting for file sizes to now also display kilobytes, megabytes, or gigabytes to more accurately reflect the displayed file size by the OS.

### Version 1.1.0
* Added support for `stream:` files.
* Added support for regional files. Select a region from the dropdown to choose the region for extracting files. 
* Minor improvements to user interface and file details panel. Uncompressed files now only show the actual size instead of identical values for compressed/uncompressed size.

### Version 1.0.1
* Fixed exporting files from `Prebuilt:`
* Adjusted output paths for files with missing hashes to use the proper directory structure and include the extension
* Separate downloads for Linux x64 and Windows x64