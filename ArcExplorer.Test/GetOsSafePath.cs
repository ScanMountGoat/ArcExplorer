using ArcExplorer.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArcExplorer.Test
{
    [TestClass]
    public class GetOsSafePath
    {
        [TestMethod]
        public void PrebuiltFile()
        {
            Assert.AreEqual("prebuilt/final_00.h264", 
                ArcPaths.GetOsSafePath("prebuilt:/final_00.h264", "final_00.h264", "h264"));
        }

        [TestMethod]
        public void StreamFile()
        {
            Assert.AreEqual("stream/movie/c2_howtoplay.webm", ArcPaths.GetOsSafePath("stream:/movie/c2_howtoplay.webm", "c2_howtoplay.webm", "webm"));
        }
        
        [TestMethod]
        public void MissingHash()
        {
            Assert.AreEqual("render/pipeline/0x21d8282e94.nurpdb", ArcPaths.GetOsSafePath("render/pipeline/0x21d8282e94", "0x21d8282e94", "nurpdb"));
        }
    }
}
