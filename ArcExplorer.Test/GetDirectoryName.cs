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
            Assert.AreEqual("item", ArcPaths.GetDirectoryName("item", true));
            Assert.AreEqual("item", ArcPaths.GetDirectoryName("item", false));
        }

        [TestMethod]
        public void TrailingSlash()
        {
            Assert.AreEqual("b/", ArcPaths.GetDirectoryName("a/b/", true));
            Assert.AreEqual("b", ArcPaths.GetDirectoryName("a/b/", false));
        }

        [TestMethod]
        public void NoTrailingSlash()
        {
            Assert.AreEqual("b", ArcPaths.GetDirectoryName("a/b", true));
            Assert.AreEqual("b", ArcPaths.GetDirectoryName("a/b", false));
        }

        [TestMethod]
        public void EmptyString()
        {
            Assert.AreEqual("", ArcPaths.GetDirectoryName("", true));
            Assert.AreEqual("", ArcPaths.GetDirectoryName("", false));
        }
    }
}
