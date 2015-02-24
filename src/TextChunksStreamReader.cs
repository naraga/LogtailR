using System.Collections.Generic;

namespace LogtailR
{
    public class TextChunksStreamReader
    {
        private readonly IEnumerable<TextChunk> _chunksSource;
        private OutputSettings _outputSettings;
        private const string DefaultBomRx = "^";   // begining of line

        private class OutputSettings
        {
            public string BeginingOfMessageRx { get; set; }
        }

        public TextChunksStreamReader(IEnumerable<TextChunk> chunksSource)
        {
            _chunksSource = chunksSource;
            _outputSettings = new OutputSettings();
        }

        /// <summary>
        /// Splits into messages. Omits  whitespace messages
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LogMessage> GetMessages()
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var chunk in _chunksSource)
            {
                if(chunk.IsEmpty)
                    continue;

                var bomRx = string.IsNullOrWhiteSpace(_outputSettings.BeginingOfMessageRx)
                    ? DefaultBomRx
                    : _outputSettings.BeginingOfMessageRx;
                var messages = TextChunkSplitter.Split(chunk.Text, bomRx);

                foreach (var m in messages)
                {
                    if(!string.IsNullOrWhiteSpace(m))
                    {
                        yield return new LogMessage
                        {
                            Source = chunk.Source,
                            Content = m.Trim()
                        };
                    }
                }
            }
        }

        public void SetOutput(string beginingOfMessageRx)
        {
            _outputSettings = new OutputSettings
            {
                BeginingOfMessageRx = beginingOfMessageRx,
            };
        }
    }

    public class LogMessage
    {
        public string Content { get; set; }
        public string Source { get; set; }
    }
}