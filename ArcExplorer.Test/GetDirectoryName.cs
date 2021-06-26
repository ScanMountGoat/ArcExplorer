using ArcExplorer.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArcExplorer.Test
{
    [TestClass]
    public class GetDirectoryName
    {
        [TestMethod]
        public void RootDirectory()
        {
            Assert.AreEqual("item", ArcPaths.GetDirectoryName("item"));
        }

        [TestMethod]
        public void TrailingSlash()
        {
            Assert.AreEqual("b", ArcPaths.GetDirectoryName("a/b/"));
        }

        [TestMethod]
        public void NoTrailingSlash()
        {
            Assert.AreEqual("b", ArcPaths.GetDirectoryName("a/b"));
        }

        [TestMethod]
        public void EmptyString()
        {
            Assert.AreEqual("", ArcPaths.GetDirectoryName(""));
        }
    }
}
