using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LogtailR
{
    internal class FileTailReader
    {
        private readonly string _fileName;

        public FileTailReader(string fileName)
        {
            _fileName = fileName;
        }

        public void SetToEof()
        {
            LastPosition = new FileInfo(_fileName).Length;
        }

        public async Task<TextChunk> ReadTailAsync()
        {
            try
            {
                using (var reader = new StreamReader(new FileStream(_fileName,
                    FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    if (reader.BaseStream.Length == LastPosition)
                        return TextChunk.CreateEmpty(LastPosition, _fileName);
                    
                    var startPosition = LastPosition;
                    reader.BaseStream.Position = startPosition;
                    var text = await reader.ReadToEndAsync();

                    LastPosition = reader.BaseStream.Position;
                    LastNonEmptyReadAt = DateTime.Now;

                    var textChunk = new TextChunk
                    {
                        StartPosition = startPosition,
                        Text = text,
                        EndPosition = LastPosition,
                        Source = _fileName
                    };
                    return textChunk;
                }
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

//        public IEnumerable<TextChunk> GetChunks()
//        {
//            while (true)
//            {
//                var chunk = ReadTailAsync().Result;
                
//                // unable to read (file does not exist anymore?)
//                if(chunk == null)
//                    yield break;

//                yield return chunk;
//            }
//// ReSharper disable once FunctionNeverReturns
//        }


        public long LastPosition { get; set; }
        public DateTime? LastNonEmptyReadAt { get; set; }


    }

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