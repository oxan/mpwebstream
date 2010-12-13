using System;
using System.Threading;

namespace MPWebStream.TvServerPlugin {
    class WebappMonitor {
        private bool doMonitor;
        private Cassini.Server serv;

        public void start() {
            // TODO: first start TV4Home service

            // then start Cassini
            // FIXME: detect path dynamic
            // FIXME: protect tv server thread for crashes
            serv = new Cassini.Server(9000, "/", "C:\\TVServer\\MPWebStream");
            serv.Start();
        }

        public void startMonitoring() {
            // start monitoring
            doMonitor = true;
            start();

            // monitor
            while (doMonitor) {
                // TODO: check if webserver is still running

                Thread.Sleep(30000);
            }
        }

        public void stop() {
            // first stop Cassini
            serv.Stop();

            // then stop TV4Home service
        }
    }
}
