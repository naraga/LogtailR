using System;

namespace LogtailR
{
    /// <summary>
    /// Chunk of text grabbed from tail at once. We always assume that logging framework flushes single message in a single chung (not flushing it multiple times).
    /// </summary>
    public class TextChunk
    {
        public string Text { get; set; }
        public string Source { get; set; }
        public long EndPosition { get; set; }

        public bool IsEmpty { get { return String.IsNullOrEmpty(Text); } }
        public long StartPosition { get; set; }

        public TextChunk()
        {
        }

        public TextChunk(string text, string source, long startPosition)
        {
            Text = text;
            Source = source;
            StartPosition = startPosition;
            EndPosition = StartPosition + text.Length;
        }

        public static TextChunk CreateEmpty(long lastPosition, string source)
        {
            return new TextChunk
            {
                Text = string.Empty,
                Source = source,
                StartPosition = lastPosition,
                EndPosition = lastPosition
            };
        }
    }
}