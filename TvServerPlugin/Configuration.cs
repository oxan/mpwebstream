using System;
using System.IO;

namespace MPWebStream.TvServerPlugin {
    class Configuration {
        public static string SitePath {
            get {
                return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\MPWebStream\\Site";
            }
        }

        public static string CassiniServerPath {
            get {
                return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\MPWebStream\\Cassini.exe";
            }
        }

        public static int Port {
            get { return (int)Properties.MPWebStream.Default.Port; }
            set { Properties.MPWebStream.Default.Port = (uint)value; }
        }

        public static bool UseWebserver {
            get { return Properties.MPWebStream.Default.UseWebserver; }
            set { Properties.MPWebStream.Default.UseWebserver = value; }
        }

        public static bool ManageTV4Home {
            get { return Properties.MPWebStream.Default.ManageTV4Home; }
            set { Properties.MPWebStream.Default.ManageTV4Home = value; }
        }

        public static int MonitorPollInterval {
            get { return 30; }
        }
    }
}
