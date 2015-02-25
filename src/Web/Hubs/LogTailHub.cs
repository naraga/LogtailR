using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace LogtailR.Web.Hubs
{
    //TODO: strongly typed hub
    public class LogTailHub : Hub
    {
        private readonly TailListeningSessionsRepository _listeningSessionsRepo;
        private readonly string _defaultRedColorRx;
        private readonly string _defaultWhiteColorRx;
        private readonly string _defaultYellowColorRx;

        public LogTailHub(TailListeningSessionsRepository listeningSessionsRepo,
            string defaultRedColorRx = null, string defaultWhiteColorRx = null, string defaultYellowColorRx = null)
        {
            _listeningSessionsRepo = listeningSessionsRepo;
            _defaultRedColorRx = defaultRedColorRx;
            _defaultWhiteColorRx = defaultWhiteColorRx;
            _defaultYellowColorRx = defaultYellowColorRx;
        }

        public LogTailHub()
        {
        }

        public override Task OnConnected()
        {
            var sess = TailListeningSession.CreateDefault(Context.ConnectionId);

            if (!string.IsNullOrWhiteSpace(_defaultRedColorRx))
                sess.RedColorRx = _defaultRedColorRx;

            if (!string.IsNullOrWhiteSpace(_defaultWhiteColorRx))
                sess.WhiteColorRx = _defaultWhiteColorRx;

            if (!string.IsNullOrWhiteSpace(_defaultYellowColorRx))
                sess.YellowColorRx = _defaultYellowColorRx;

            _listeningSessionsRepo.AddSession(sess);

            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            //todo: when does this get called?
            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            _listeningSessionsRepo.RemoveSession(Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }

        //TODO: use dedicated VM - not TailListeningSession directly
        public void StartStreaming(TailSessionSettingsViewModel tailSesisonSettings)
        {
            var sess = _listeningSessionsRepo.GetSession(Context.ConnectionId);

            sess.IncludeRx = tailSesisonSettings.IncludeRx;
            sess.ExcludeRx = tailSesisonSettings.ExcludeRx;
            sess.RedColorRx = tailSesisonSettings.RedColorRx;
            sess.WhiteColorRx = tailSesisonSettings.WhiteColorRx;
            sess.YellowColorRx = tailSesisonSettings.YellowColorRx;

            sess.Start();
        }

        public void StopStreaming()
        {
            _listeningSessionsRepo.GetSession(Context.ConnectionId).Stop();
        }

        public TailSessionSettingsViewModel GetTailSettings()
        {
            var sess = _listeningSessionsRepo.GetSession(Context.ConnectionId);
            return new TailSessionSettingsViewModel
            {
                IncludeRx = sess.IncludeRx,
                ExcludeRx = sess.ExcludeRx,
                RedColorRx = sess.RedColorRx,
                WhiteColorRx = sess.WhiteColorRx,
                YellowColorRx = sess.YellowColorRx
            };
        }
    }
}