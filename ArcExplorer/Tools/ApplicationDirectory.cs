using System;
using System.IO;

namespace ArcExplorer.Tools
{
    internal static class ApplicationDirectory
    {
        /// <summary>
        /// Converts <paramref name="fileName"/> to an absolute path in the application's data directory.
        /// </summary>
        /// <param name="fileName">The file name and extension like "file.txt"</param>
        /// <returns>The resulting absolute path</returns>
        public static string CreateAbsolutePath(string fileName)
        {
            // Store ArcExplorer files for the local user.
            // This makes it easier to work with bundles that can't be modified internally.
            // On Windows, this will be a path like "C:\\Users\\UserName\\AppData\\Local\ArcExplorer\file.txt";
            var applicationDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(applicationDirectory, "ArcExplorer", fileName);
        }
    }
}
