using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LogtailR
{
    //TODO: use reactive extensions ?
    public class TailBroadcastingService
    {
        private readonly IEnumerable<LogMessage> _logMessages;
        private readonly TailListeningSessionsRepository _listeningSessions;
        private const RegexOptions RxMatchingOptions = RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.Compiled;

        public event EventHandler<TailNewMessageEventArgs> NewMessage = delegate { };

        public TailBroadcastingService(IEnumerable<LogMessage> logMessages, TailListeningSessionsRepository listeningSessions)
        {
            _logMessages = logMessages;
            _listeningSessions = listeningSessions;
        }

        public static TailBroadcastingService BroadcastDirTail(
            string directoryName, string fileNameFilter, string bomRx, 
            TailListeningSessionsRepository listeningSessions)
        {
            var dirLogSource = new DirectoryLogSource(directoryName, fileNameFilter, true);
            dirLogSource.ScanContent();
            dirLogSource.StartWatching();

            var textChunksStreamReader = new TextChunksStreamReader(dirLogSource.GetChunks());
            textChunksStreamReader.SetOutput(bomRx);
            return new TailBroadcastingService(textChunksStreamReader.GetMessages(), listeningSessions);
        }

        public void StartStreaming()
        {
            foreach (var logMessage in _logMessages)
            {
                var runningSessions = _listeningSessions.GetAllRunningSessions().ToList();

                foreach (var session in runningSessions)
                {
                    if (IsMessageForSession(logMessage, session))
                    {
                        var color = GetMessageColor(logMessage, session);
                        NewMessage(this, new TailNewMessageEventArgs
                        {
                            TargetListeningSession = session,
                            LogMessage = new LogMessageForListener
                            {
                                Message = logMessage,
                                Color = color
                            }
                        });
                    }
                }
                
            }
        }

        private string GetMessageColor(LogMessage m, TailListeningSession session)
        {
            if (session.HasRedColorRx && Regex.IsMatch(m.Content, session.RedColorRx, RxMatchingOptions))
                return "red";

            if (session.HasWhiteColorRx && Regex.IsMatch(m.Content, session.WhiteColorRx, RxMatchingOptions))
                return "white";

            if (session.HasYellowColorRx && Regex.IsMatch(m.Content, session.YellowColorRx, RxMatchingOptions))
                return "yellow";

            return "white";
        }

        private bool IsMessageForSession(LogMessage m, TailListeningSession session)
        {
            if (session.HasIncludeRx)
                return Regex.IsMatch(m.Content, session.IncludeRx, RxMatchingOptions);
            
            if (session.HasExcludeRx)
                return !Regex.IsMatch(m.Content, session.ExcludeRx, RxMatchingOptions);

            return true;
        }
    }

    public class LogMessageForListener
    {
        public LogMessage Message { get; set; }
        public string Color { get; set; }
    }

    public class TailNewMessageEventArgs
    {
        public TailListeningSession TargetListeningSession { get; set; }
        public LogMessageForListener LogMessage { get; set; }
    }
}