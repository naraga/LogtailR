using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        [NamedArgument("log", Action = ParseAction.Append, Description = "Log source. Example: --log \"c:\\logs\\*.log")]
        public List<string> Logs { get; set; }

        [NamedArgument("bom", Action = ParseAction.Append, Description = "Begining of message regex. Example: --bom \"^(?<at>\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2}\\.\\d{3}):(?<level>.+?):(?<logger>.+?):")]
        public List<string> Boms { get; set; }

        [NamedArgument("snrx", Action = ParseAction.Append, Description = "Source name message regex. All captured groups are concatenated. Example: --snrx \"^(?:\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2}.\\d{3}):(?:.+?):(?<logger>.+?):\"")]
        public List<string> SourceNameRegexes { get; set; }


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

                var logBroadcasters = new List<LogBroadcastingService>();

                //
                for (int i = 0; i < opt.Logs.Count; i++)
                {
                    var log = opt.Logs[i];
                    if (string.IsNullOrWhiteSpace(log))
                        continue;
                  
                    // if BOM specified, then the number of BOMs must be equal to number of LOG sources
                    var bom = opt.Boms.Any() ? opt.Boms[i] : null;

                    var logBroadcaster = LogBroadcastingService.BroadcastDirTail(
                        Path.GetDirectoryName(log),
                        Path.GetFileName(log),
                        bom,
                        sessionRepo);

                    // send single message to specific listener session
                    logBroadcaster.NewMessage +=
                        (sender, eventArgs) => hub.Clients.Client(eventArgs.TargetListeningSession.ConnectionId)
                            .NewMessage(eventArgs.LogMessage);

                    Task.Factory.StartNew(logBroadcaster.StartStreaming, TaskCreationOptions.LongRunning);
                    
                    logBroadcasters.Add(logBroadcaster);
                }


                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
            }
        }
    }

    

    
}
