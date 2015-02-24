using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogtailR.Tests
{
    [TestClass]
    public class TextChunkSplitterTests
    {
        [TestMethod]
        public void NoBomInNonEmptyMessageTest()
        {
            var result = TextChunkSplitter.Split("hello world", "<bom>").ToList();
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("hello world", result[0]);
        }

        [TestMethod]
        public void EmptyBomInNonEmptyMessageTest()
        {
            var result = TextChunkSplitter.Split("hello world", "").ToList();
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("hello world", result[0]);
        }

        [TestMethod]
        public void Split3MessagesWithPreludeTest()
        {
            var result = TextChunkSplitter.Split("hello world<bom>how are your atoms doing\ndown there?<bom>yooooo\n\n", "<bom>").ToList();
            Assert.AreEqual(3, result.Count());
            Assert.AreEqual("hello world", result[0]);
            Assert.AreEqual("<bom>how are your atoms doing\ndown there?", result[1]);
            Assert.AreEqual("<bom>yooooo\n\n", result[2]);  // must preserve trailing whitespace
        }
    }
}
