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
            get { return 9000; }
        }
    }
}
