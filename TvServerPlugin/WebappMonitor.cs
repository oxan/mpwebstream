using System;
using System.Threading;
using System.Diagnostics;

namespace MPWebStream.TvServerPlugin {
    class WebappMonitor {
        private bool doMonitor;
        private Process server;

        public void start() {
            // TODO: first start TV4Home service

            // then start Cassini
            server = new Process();
            server.StartInfo.Arguments = "9000 C:\\";
            server.StartInfo.CreateNoWindow = true;
            server.StartInfo.FileName = "X:\\MPWebStream\\Cassini-v35\\bin\\Debug\\Cassini.exe"; // FIXME
            server.StartInfo.UseShellExecute = false;
            server.Start();
        }

        public void startMonitoring() {
            // start monitoring
            doMonitor = true;
            start();

            // monitor
            while (doMonitor) {
                if (server.HasExited) {
                    // Cassini crashed: log and restart
                    //Log.Warning("Integrated Cassini webserver crashed, restarting");
                    server.Start();
                }

                Thread.Sleep(30000);
            }
        }

        public void stop() {
            // first stop Cassini
            server.Kill();

            // then stop TV4Home service
        }
    }
}
