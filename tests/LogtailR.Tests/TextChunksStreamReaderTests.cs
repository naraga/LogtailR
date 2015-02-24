using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogtailR.Tests
{
    [TestClass]
    public class TextChunksStreamReaderTests
    {
        [TestMethod]
        public void OmitEmpty_TrimAndParseNonempty_Tests()
        {
            var reader = new TextChunksStreamReader(new[]
            {
                TextChunk.CreateEmpty(42, "xx"),
                new TextChunk("hello world<bom>how are your atoms doing\ndown there?<bom>yooooo\n\n", "src", 42),
                TextChunk.CreateEmpty(42, "xx"),
            });

            reader.SetOutput("<bom>");

            var result = reader.GetMessages().ToList();
            
            Assert.AreEqual(3, result.Count);

            Assert.AreEqual("hello world", result[0].Content);
            Assert.AreEqual("src", result[0].Source);

            Assert.AreEqual("<bom>how are your atoms doing\ndown there?", result[1].Content);
            Assert.AreEqual("src", result[1].Source);

            Assert.AreEqual("<bom>yooooo", result[2].Content);      // trimmed
            Assert.AreEqual("src", result[2].Source);
        }

    }
}
