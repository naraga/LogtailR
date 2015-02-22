using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace LogtailR
{
    public class TailListeningSessionsRepository
    {
        public static readonly TailListeningSessionsRepository Instance = new TailListeningSessionsRepository();

        readonly ConcurrentDictionary<string, TailListeningSession> _sessions = new ConcurrentDictionary<string, TailListeningSession>();

        public TailListeningSession GetSession(string connectionId)
        {
            return _sessions[connectionId];
        }

        public void AddSession(TailListeningSession listeningSession)
        {
            if (!_sessions.TryAdd(listeningSession.ConnectionId, listeningSession))
                throw new InvalidOperationException("Unable to add listeningSession " + listeningSession.ConnectionId);
        }

        public void RemoveSession(string connectionid)
        {
            TailListeningSession value;
            _sessions.TryRemove(connectionid, out value);
        }

        public IEnumerable<TailListeningSession> GetAllRunningSessions()
        {
            return _sessions.Values.Where(ts => ts.IsRunning);
        } 
    }

    public class TailListeningSession
    {
        public string ConnectionId { get; set; }
        public bool IsRunning { get; set; }

        public string IncludeRx { get; set; }
        public string ExcludeRx { get; set; }

        public string RedColorRx { get; set; }
        public string WhiteColorRx { get; set; }
        public string YellowColorRx { get; set; }

        public bool HasIncludeRx { get { return !string.IsNullOrWhiteSpace(IncludeRx); } }
        public bool HasExcludeRx { get { return !string.IsNullOrWhiteSpace(ExcludeRx); } }

        public bool HasRedColorRx { get { return !string.IsNullOrWhiteSpace(RedColorRx); } }
        public bool HasWhiteColorRx { get { return !string.IsNullOrWhiteSpace(WhiteColorRx); } }
        public bool HasYellowColorRx { get { return !string.IsNullOrWhiteSpace(YellowColorRx); } }

        public void Start()
        {
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public static TailListeningSession CreateDefault(string connectionId)
        {
            return new TailListeningSession
            {
                ConnectionId = connectionId,

                IsRunning = true,

                IncludeRx = "",
                ExcludeRx = "",

                RedColorRx = "warn",
                WhiteColorRx = "info",
                YellowColorRx = "debug"
            };
        }
    }
}