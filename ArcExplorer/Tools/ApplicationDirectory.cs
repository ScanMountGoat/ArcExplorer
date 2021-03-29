using System.IO;

namespace ArcExplorer.Tools
{
    internal static class ApplicationDirectory
    {
        /// <summary>
        /// Converts the given <paramref name="path"/> to an absolute path.
        /// <paramref name="path"/> is assumed to be relative to the executable directory.
        /// If <paramref name="path"/> is already an absolute path, <paramref name="path"/> will be returned instead.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>The resulting absolute path</returns>
        public static string CreateAbsolutePath(string path)
        {
            var executableDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "";
            return Path.Combine(executableDirectory, path);
        }
    }
}
