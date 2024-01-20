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
            Assert.AreEqual(null, ArcPaths.GetCleanedFilePath(null));
        }

        [TestMethod]
        public void EmptyString()
        {
            Assert.AreEqual("", ArcPaths.GetCleanedFilePath(""));
        }

        [TestMethod]
        public void RootDirectory()
        {
            Assert.AreEqual("item", ArcPaths.GetCleanedFilePath("item"));
        }

        [TestMethod]
        public void TrailingSlash()
        {
            Assert.AreEqual("b/", ArcPaths.GetCleanedFilePath("b/"));
        }

        [TestMethod]
        public void DoubleForwardSlash()
        {
            Assert.AreEqual("a/b/", ArcPaths.GetCleanedFilePath("a//b/"));
        }

        [TestMethod]
        public void DoubleBackwardSlash()
        {
            Assert.AreEqual("a/b/", ArcPaths.GetCleanedFilePath("a\\b/"));
        }

        [TestMethod]
        public void DoubleForwardAndBackwardSlashes()
        {
            Assert.AreEqual("a/b/c/d/", ArcPaths.GetCleanedFilePath("a\\b//c\\d//"));
        }
    }
}
