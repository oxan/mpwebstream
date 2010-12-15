﻿using System;
using SetupTv;
using TvEngine;
using TvControl;
using System.Threading;
using TvLibrary.Log;

namespace MPWebStream.TvServerPlugin {
    public class MPWebStreamPlugin : ITvServerPlugin {
        public string Name {
            get { return "MPWebStream"; }
        }

        public string Version {
            get { return "0.1.0.0"; }
        }

        public string Author {
            get { return "Oxan"; }
        }

        public bool MasterOnly {
            get { return true; }
        }

        private WebappMonitor monitor = null;
        private Thread webthread = null;

        public void Start(IController controller) {
            // start the webserver in a separate thread
            Log.Info("MPWebStream: version {0} starting", Version);
            Log.Info("MPWebStream: server {0}, site {1}", Configuration.CassiniServerPath, Configuration.SitePath);
            monitor = new WebappMonitor();
            webthread = new Thread(new ThreadStart(monitor.startMonitoring));
            webthread.Start();
        }

        public void Stop() {
            // stop the thread
            Log.Info("MPWebStream: stopping");
            monitor.stop();
        }

        public SectionSettings Setup {
            get { return null; }
        }
    }
}