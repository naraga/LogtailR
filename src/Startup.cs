using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

namespace LogtailR
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR(new HubConfiguration {EnableDetailedErrors = true});

            app.Use((context, next) =>
            {
                if (!context.Request.Path.HasValue || context.Request.Path.Value == "/")
                    context.Request.Path = new PathString("/Web/App/Logtail.html");

                return next();
            });

            app.UseStaticFiles("/Web/App");
            app.UseStaticFiles("/Web/Scripts");
            app.UseStaticFiles("/Web/Styles");
            app.UseStaticFiles("/Web/Fonts");
        }
    }
}