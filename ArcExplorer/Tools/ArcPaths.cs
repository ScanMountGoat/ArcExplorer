using System.IO;

namespace ArcExplorer.Tools
{
    /// <summary>
    /// Methods for processing paths for ARC nodes.
    /// These methods should be used instead of methods from <see cref="System.IO"/> for proper handling of directories.
    /// </summary>
    public static class ArcPaths
    {
        /// <summary>
        /// Calculates the parent path for <paramref name="absolutePath"/> using '/' (single slash) 
        /// as the path separator. For paths to folders in the root directory, <c>null</c> is returned.
        /// Example: The parent path of "a/b/c/" is "a/b".
        /// </summary>
        /// <param name="absolutePath">The input path</param>
        /// <returns>The parent path or <c>null</c> if there is no parent</returns>
        public static string? GetParentPath(string absolutePath)
        {
            // Find the ending index of the parent path string.
            // This requires removing any trailing slashes first.
            var lastIndex = absolutePath.TrimEnd('/').LastIndexOf('/');

            // There is no parent.
            if (lastIndex <= 0)
                return null;

            return absolutePath.Substring(0, lastIndex);
        }

        /// <summary>
        /// Calculates the directory name from <paramref name="absolutePath"/>.
        /// Example: The name for "a/b/c/" is "c".
        /// </summary>
        /// <param name="absolutePath">The input path</param>
        /// <returns>The name of the directory</returns>
        public static string GetDirectoryName(string absolutePath)
        {
            // DirectoryInfo doesn't handle null or empty strings.
            if (string.IsNullOrEmpty(absolutePath))
                return "";

            return new DirectoryInfo(absolutePath).Name;
        }
    }
}
