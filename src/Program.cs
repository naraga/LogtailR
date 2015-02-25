using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using clipr;
using LogtailR.Web.Hubs;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;

namespace LogtailR
{
    [ApplicationInfo(Description = "This is a set of options.")]
    public class Options
    {
        [NamedArgument("logs", Action = ParseAction.Append, Description = "Log sources. Example: --logs \"c:\\logs\\*.log|^\\d{2}\\s[a-zA-Z]{3}\\s\\d{4}, \\d{2}:\\d{2}:\\d{2}\\.\\d{3}:\"")]
        public List<string> Logs { get; set; }

        [NamedArgument("url", Description = "")]
        public string Url { get; set; }

        [NamedArgument("red", Description = "")]
        public string Red { get; set; }

        [NamedArgument("white", Description = "")]
        public string White { get; set; }

        [NamedArgument("yellow", Description = "")]
        public string Yellow { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var opt = CliParser.Parse<Options>(args);

            string url = opt.Url ?? "http://+:8080/logtailr";

            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Running on {0}", url);

                var sessionRepo = new TailListeningSessionsRepository();

                GlobalHost.DependencyResolver.Register(typeof(LogTailHub), () => new LogTailHub(sessionRepo, opt.Red, opt.White, opt.Yellow));
                var hub = GlobalHost.ConnectionManager.GetHubContext<LogTailHub>();

                foreach (var log in opt.Logs)
                {
                    if (string.IsNullOrWhiteSpace(log))
                        continue;

                    var logWithBomSplitted = log.Split('|');
                    var dirWithMask = logWithBomSplitted[0];

                    var bom = logWithBomSplitted.Length > 1 ? logWithBomSplitted[1] : null;

                    var tailService = TailBroadcastingService.BroadcastDirTail(
                        Path.GetDirectoryName(dirWithMask), 
                        Path.GetFileName(dirWithMask), 
                        bom, 
                        sessionRepo);
                    
                    // send single message to specific listener session
                    tailService.NewMessage +=
                        (sender, eventArgs) => hub.Clients.Client(eventArgs.TargetListeningSession.ConnectionId)
                        .NewMessage(eventArgs.LogMessage);

                    Task.Factory.StartNew(tailService.StartStreaming, TaskCreationOptions.LongRunning);
                }
                


                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
            }
        }
    }

    

    
}
