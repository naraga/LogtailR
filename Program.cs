using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using LogtailR.Web;
using LogtailR.Web.Hubs;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Security.Provider;

namespace LogtailR
{
    class Program
    {
        static void Main(string[] args)
        {
            //var tmpRndLog = new RandomLogger(@"c:\temp\logtailrlogs", 500);
            //Task.Factory.StartNew(tmpRndLog.StartWriting, TaskCreationOptions.LongRunning);

            var dir = Path.GetDirectoryName(args[0]);
            var filter = Path.GetFileName(args[0]);

            const string url = "http://localhost:8080"; //TODO: "http://+:8080"

            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Running on {0}", url);

                //var logFileName = tmpRndLog.LogFileName;

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
