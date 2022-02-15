using ArcExplorer.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArcExplorer.Test
{
    [TestClass]
    public class GetCleanedDirectoryPath
    {
        [TestMethod]
        public void NullPath()
        {
            Assert.AreEqual(null, ArcPaths.GetCleanedDirectoryPath(null));
        }

        [TestMethod]
        public void EmptyString()
        {
            Assert.AreEqual("", ArcPaths.GetCleanedDirectoryPath(""));
        }

        [TestMethod]
        public void RootDirectory()
        {
            Assert.AreEqual("item", ArcPaths.GetCleanedDirectoryPath("item"));
        }

        [TestMethod]
        public void TrailingSlash()
        {
            Assert.AreEqual("b/", ArcPaths.GetCleanedDirectoryPath("b/"));
        }

        [TestMethod]
        public void DoubleForwardSlash()
        {
            Assert.AreEqual("a/b/", ArcPaths.GetCleanedDirectoryPath("a//b/"));
        }

        [TestMethod]
        public void DoubleBackwardSlash()
        {
            Assert.AreEqual("a/b/", ArcPaths.GetCleanedDirectoryPath("a\\b/"));
        }

        [TestMethod]
        public void DoubleForwardAndBackwardSlashes()
        {
            Assert.AreEqual("a/b/c/d/", ArcPaths.GetCleanedDirectoryPath("a\\b//c\\d//"));
        }
    }
}
