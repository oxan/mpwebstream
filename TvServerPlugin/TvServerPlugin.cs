using System;
using SetupTv;
using TvEngine;
using TvControl;
using System.Threading;

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
            monitor = new WebappMonitor();
            webthread = new Thread(new ThreadStart(monitor.startMonitoring));
            webthread.Start();
        }

        public void Stop() {
            // stop the thread
            monitor.stop();
        }

        public SectionSettings Setup {
            get { return null; }
        }
    }
}
