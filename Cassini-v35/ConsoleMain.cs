using System;
using System.Threading;

namespace Cassini {
    class ConsoleMain {
        public static int Main(string[] args) {
            // parse arguments
            if (args.Length != 2) {
                System.Console.WriteLine("Usage: Cassini.exe [port] [path]");
                return 1;
            }
            int port = Int32.Parse(args[0]);
            string physicalPath = args[1];

            // start server
            Server server = new Server(port, "/", physicalPath);
            server.Start();

            // loop until forever
            while (true) {
                // TODO: maybe something to break out earlier? 
                Thread.Sleep(60000);
            }

            // stop server
            server.Stop();

            return 0;
        }
    }
}
