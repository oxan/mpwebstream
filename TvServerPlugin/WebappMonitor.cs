#region Copyright
/* 
 *  Copyright (C) 2010, 2011 Oxan
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#endregion

using System;
using System.Threading;
using System.ServiceProcess;
using System.Diagnostics;
using TvLibrary.Log;

namespace MPWebStream.TvServerPlugin {
    class WebappMonitor {
        private bool doMonitor;
        private bool shouldStart = false;
        private Process server;
        private Configuration config;

        public void start() {
            config = new LoggingConfiguration();

            // first start TV4Home service
            try {
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
            } catch (InvalidOperationException e) {
                Log.Error("MPWebStream: TV4Home Core Service not installed; not starting.");
                return;
            }


            // then start Cassini
            if (config.UseWebserver) {
                try {
                    this.shouldStart = true;
                    server = new Process();
                    server.StartInfo.Arguments = String.Format(@"{0} ""{1}""", config.Port.ToString(), config.SitePath);
                    server.StartInfo.CreateNoWindow = true;
                    server.StartInfo.FileName = config.CassiniServerPath;
                    server.StartInfo.UseShellExecute = false;
                    server.Start();
                    Log.Info("MPWebStream: Started Cassini web server.");
                } catch (Exception e) {
                    Log.Error("MPWebStream: Failed to start webserver.");
                    Log.Write(e.ToString());
                    Log.Error("MPWebStream: Not trying to restart.");
                    this.shouldStart = false;
                }
            } else {
                server = null;
                Log.Info("MPWebStream: Usage of Cassini web server is disabled.");
            }
        }

        public void startMonitoring() {
            // start process
            doMonitor = true;
            start();
            if (!this.shouldStart)
                return;
            Log.Info(String.Format("MPWebStream: Started monitoring Cassini at poll interval {0} seconds", config.MonitorPollInterval));

            // monitor
            try {
                while (doMonitor) {
                    if (server.HasExited) {
                        // Cassini crashed: log and restart
                        Log.Error(String.Format("MPWebStream: Integrated Cassini web server crashed with exit code {0}, restarting...", server.ExitCode));
                        server.Start();
                    }

                    Thread.Sleep(config.MonitorPollInterval * 1000);
                }
            } catch (ThreadAbortException) {
                // we should stop
                stop();
            }
        }

        protected void stop() {
            // first stop Cassini
            if (server != null) {
                Log.Info("MPWebStream: killing Cassini");
                server.Kill();
            }

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
