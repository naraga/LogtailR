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


        public long LastPosition { get; set; }
        public DateTime? LastNonEmptyReadAt { get; set; }
    }
}