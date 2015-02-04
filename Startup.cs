using Microsoft.AspNet.SignalR;
using Owin;

namespace LogtailR
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR(new HubConfiguration{EnableDetailedErrors = true});
        }
    }
}