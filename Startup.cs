using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace LogtailR
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR(new HubConfiguration {EnableDetailedErrors = true});
            app.UseStaticFiles("/Web/App");
            app.UseStaticFiles("/Web/Scripts");
            app.UseStaticFiles("/Web/Styles");
            app.UseStaticFiles("/Web/Fonts");

            app.Map("/xxx", map =>
            {
                map.Use((context, next) =>
                {
                    context.Request.Path = new PathString("/App/Logtail.html");

                    return next();
                });

                map.UseStaticFiles();
            });
        }
    }
}