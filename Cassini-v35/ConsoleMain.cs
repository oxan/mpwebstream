/* **********************************************************************************
 *
 * Copyright (c) 2010, 2011 Oxan. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License (Ms-PL). A copy of the license can be found in the license.htm file
 * included in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

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
            Server server = new Server(port, "/", physicalPath, false);
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
