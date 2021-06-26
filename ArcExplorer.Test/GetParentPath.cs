using ArcExplorer.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArcExplorer.Test
{
    [TestClass]
    public class GetParentPath
    {
        [TestMethod]
        public void RootDirectory()
        {
            Assert.AreEqual(null, ArcPaths.GetParentPath("item"));
        }

        [TestMethod]
        public void TrailingSlash()
        {
            Assert.AreEqual("a", ArcPaths.GetParentPath("a/b/"));
        }

        [TestMethod]
        public void NoTrailingSlash()
        {
            Assert.AreEqual("a", ArcPaths.GetParentPath("a/b"));
        }

        [TestMethod]
        public void EmptyString()
        {
            Assert.AreEqual(null, ArcPaths.GetParentPath(""));
        }
    }
}
