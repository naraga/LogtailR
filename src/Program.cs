using System;
using System.IO;
using System.Threading.Tasks;
using LogtailR.Web.Hubs;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;

namespace LogtailR
{
    class Program
    {
        static void Main(string[] args)
        {
            //var tmpRndLog = new RandomLogger(@"c:\temp\logtailrlogs", 500);
            //Task.Factory.StartNew(tmpRndLog.StartWriting, TaskCreationOptions.LongRunning);

            //todo: how to specify BOM? 
            //  c:\logs\*.log:BOM       ?
            //  c:\logs\*.log|BOM       ?

            //todo: how to specify list of different sources?
            //  c:\logs\*.log,c:\temp\logs\*.log    ?

            var dir = Path.GetDirectoryName(args[0]);
            var filter = Path.GetFileName(args[0]);


            const string url = "http://localhost:8080/logtailr";

            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Running on {0}", url);

                var sessionRepo = new TailListeningSessionsRepository();
                var tailService = TailBroadcastingService.BroadcastDirTail(dir, filter, string.Empty, sessionRepo);

                GlobalHost.DependencyResolver.Register(typeof(LogTailHub), () => new LogTailHub(sessionRepo));

                var hub = GlobalHost.ConnectionManager.GetHubContext<LogTailHub>();

                // send single message to specific listener session
                tailService.NewMessage += 
                    (sender, eventArgs) => hub.Clients.Client(eventArgs.TargetListeningSession.ConnectionId)
                    .NewMessage(eventArgs.LogMessage);

                Task.Factory.StartNew(tailService.StartStreaming, TaskCreationOptions.LongRunning);

                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
            }
        }
    }

    

    
}
