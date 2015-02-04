using System;
using Microsoft.Owin.Hosting;

namespace LogtailR
{
    class Program
    {
        static void Main(string[] args)
        {
            const string url = "http://localhost:8080"; //TODO: "http://+:8080"

            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Running on {0}", url);
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
            }
        }
    }
}
