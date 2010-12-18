using System;
using System.Threading;
using System.ServiceProcess;
using System.Diagnostics;
using TvLibrary.Log;

namespace MPWebStream.TvServerPlugin {
    class WebappMonitor {
        private bool doMonitor;
        private Process server;
        private Configuration config;

        public void start() {
            config = new LoggingConfiguration();

            // first start TV4Home service
            ServiceController controller = new ServiceController("TV4HomeCoreService");
            if (controller.Status != ServiceControllerStatus.Running) {
                if (!config.ManageTV4Home) {
                    Log.Info("MPWebStream: Starting TV4HomeCoreService, ignoring user preference to not manage it because we need it");
                } else {
                    Log.Info("MPWebStream: Starting TV4HomeCoreService");
                }
                controller.Start();
                controller.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 30));
            }


            // then start Cassini
            if (config.UseWebserver) {
                server = new Process();
                server.StartInfo.Arguments = String.Format(@"{0} ""{1}""", config.Port.ToString(), config.SitePath);
                server.StartInfo.CreateNoWindow = true;
                server.StartInfo.FileName = config.CassiniServerPath;
                server.StartInfo.UseShellExecute = false;
                server.Start();
                Log.Info("MPWebStream: Started Cassini web server");
            } else {
                server = null;
                Log.Info("MPWebStream: Usage of Cassini web server is disabled");
            }
        }

        public void startMonitoring() {
            // start process
            doMonitor = true;
            start();
            if (!config.UseWebserver)
                return;
            Log.Info(String.Format("MPWebStream: Started monitoring Cassini at poll interval {0} seconds", config.MonitorPollInterval));

            // monitor
            while (doMonitor) {
                if (server.HasExited) {
                    // Cassini crashed: log and restart
                    Log.Error(String.Format("MPWebStream: Integrated Cassini web server crashed with exit code {0}, restarting...", server.ExitCode));
                    server.Start();
                }

                Thread.Sleep(config.MonitorPollInterval * 1000);
            }
        }

        public void stop() {
            // first stop Cassini
            if(server != null)
                server.Kill();

            // then stop TV4Home service
            if (config.ManageTV4Home) {
                Log.Info("MPWebStream: Stopping TV4Home service");
                ServiceController controller = new ServiceController("TV4HomeCoreService");
                controller.Stop();
            } else {
                Log.Info("MPWebStream: Managing the TV4Home service is disabled, not stopping");
            }
        }
    }
}
