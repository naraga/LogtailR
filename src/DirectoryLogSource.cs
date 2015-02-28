using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LogtailR
{
    class DirectoryLogSource
    {
        private readonly string _fileNameFilter;
        public string DirectoryName { get; set; }
        public bool IncludeSubdirectories { get; set; }
        readonly ConcurrentDictionary<string, FileTailReader> _fileTailReaders = new ConcurrentDictionary<string, FileTailReader>();
        private readonly ConcurrentDictionary<string, DateTime> _recentlyUpdated = new ConcurrentDictionary<string, DateTime>();
        FileSystemWatcher _fsw;

        public DirectoryLogSource(string directoryName, string fileNameFilter, bool includeSubdirectories)
        {
            _fileNameFilter = fileNameFilter;
            DirectoryName = directoryName;
            IncludeSubdirectories = includeSubdirectories;
        }

        public void ScanContent()
        {
            var files = Directory.EnumerateFiles(DirectoryName, _fileNameFilter,
                IncludeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            //todo: handle UnauthorizedAccessException https://msdn.microsoft.com/en-us/library/vstudio/dd997370(v=vs.100).aspx
            foreach (var file in files)
            {
                RegisterFileTailReader(file, true);
            }
        }

        public void StartWatching()
        {
            _fsw = new FileSystemWatcher(DirectoryName, _fileNameFilter)
            {
                IncludeSubdirectories = IncludeSubdirectories
            };
            _fsw.Created += (sender, args) =>
            {
                // dont set to EOF since file is newly created and we want to stream its content
                RegisterFileTailReader(args.FullPath, false);
                
                // we want to read newly created file from the beginning
                RegisterFileUpdate(args.FullPath);
            };

            _fsw.Deleted += (sender, args) =>
            {
                // remove from updates first
                DateTime time;
                _recentlyUpdated.TryRemove(args.FullPath, out time);

                // then from readers reference list
                FileTailReader reader;
                _fileTailReaders.TryRemove(args.FullPath, out reader);
            };

            _fsw.Changed += (sender, args) => RegisterFileUpdate(args.FullPath);

            _fsw.Renamed += (sender, args) =>
            {
                // todo: think this through a little bit more
                FileTailReader oldReader;
                if (_fileTailReaders.TryRemove(args.OldFullPath, out oldReader))
                {
                    RegisterFileTailReader(args.FullPath, oldReader.LastPosition);

                    DateTime lastUpdate;
                    if (_recentlyUpdated.TryRemove(args.OldFullPath, out lastUpdate))
                    {
                        RegisterFileUpdate(args.FullPath, lastUpdate);
                    }
                }
            };

            _fsw.EnableRaisingEvents = true;
        }

        public IEnumerable<TextChunk> GetChunks()
        {
            while (true)
            {
                var startOfCheckAt = DateTime.Now;
                var chunkYielded = false;
                var updatedFiles = GetRecentlyUpdatedFiles();
                foreach (var fn in updatedFiles)
                {
                    FileTailReader reader;
                    if (_fileTailReaders.TryGetValue(fn, out reader))
                    {
                        var chunk = reader.ReadTailAsync().Result;
                        if (chunk != null && !chunk.IsEmpty)
                        {
                            yield return chunk;
                            chunkYielded = true;
                        }


                        // remove file from recently updated list so that we
                        // do not activelly check for its new content anymore
                        DateTime updatedAt;
                        if (_recentlyUpdated.TryGetValue(fn, out updatedAt))
                        {
                            if (!IsUpdateRecentEnough(updatedAt))
                                _recentlyUpdated.TryRemove(fn, out updatedAt);
                        }
                    }
                }

                //todo: think this through a little bit more
                var endOfCheckAt = DateTime.Now;
                // if there were no changes to files and whole loop took less then X ms, then wait some time
                if (!chunkYielded && endOfCheckAt.Subtract(startOfCheckAt) < TimeSpan.FromMilliseconds(100))
                {
                    Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();
                }
            }
        // ReSharper disable once FunctionNeverReturns
        }

        

        bool IsUpdateRecentEnough(DateTime lastUpdateAt)
        {
            return DateTime.Now.Subtract(lastUpdateAt) < TimeSpan.FromMinutes(5);
        }


        private void RegisterFileTailReader(string file, bool setToEof)
        {
            var fileTailReader = new FileTailReader(file);
            if (setToEof)
            {
                fileTailReader.SetToEof();
            }
            _fileTailReaders.AddOrUpdate(file, fileTailReader, (s, reader) => reader);
        }

        private void RegisterFileTailReader(string file, long position)
        {
            var fileTailReader = new FileTailReader(file) { LastPosition = position };
            _fileTailReaders.AddOrUpdate(file, fileTailReader, (s, reader) => reader);
        }

        public void RegisterFileUpdate(string fileName)
        {
            RegisterFileUpdate(fileName, DateTime.Now);
        }

        public void RegisterFileUpdate(string fileName, DateTime updateAt)
        {
            _recentlyUpdated.AddOrUpdate(fileName, updateAt, (s, time) => updateAt);
        }

        /// <summary>
        /// Returns filenames of recently updated files.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GetRecentlyUpdatedFiles()
        {
            return _recentlyUpdated.Keys.ToList();
        }
    }
}