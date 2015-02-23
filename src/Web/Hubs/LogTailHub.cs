using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace LogtailR.Web.Hubs
{
    //TODO: strongly typed hub
    //TODO: use VM instead of TailListeningSession directly
    public class LogTailHub : Hub
    {
        private readonly TailListeningSessionsRepository _listeningSessionsRepo;

        public LogTailHub(TailListeningSessionsRepository listeningSessionsRepo)
        {
            _listeningSessionsRepo = listeningSessionsRepo;
        }

        public LogTailHub()
        {
        }

        public override Task OnConnected()
        {
            _listeningSessionsRepo.AddSession(TailListeningSession.CreateDefault(Context.ConnectionId));
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