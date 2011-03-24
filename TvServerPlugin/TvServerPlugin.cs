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
            get { return "0.9.9.0"; }
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
            Configuration config = new LoggingConfiguration();
            Log.Info("MPWebStream: version {0} starting", Version);
            Log.Info("MPWebStream: server {0}, site {1}", config.CassiniServerPath, config.SitePath);
            monitor = new WebappMonitor();
            webthread = new Thread(new ThreadStart(monitor.startMonitoring));
            webthread.Name = "MPWebStream";
            webthread.Start();
        }

        public void Stop() {
            // stop the thread
            Log.Info("MPWebStream: stopping");
            webthread.Abort();
        }

        public SectionSettings Setup {
            get { return new ConfigurationInterface(); }
        }
    }
}
