using System;
using System.IO;
using System.Xml;
using TvLibrary.Log;

namespace MPWebStream.TvServerPlugin {
    class Configuration {
        #region Properties
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

        public static string ConfigPath {
            get {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Team MediaPortal\\MediaPortal TV Server\\MPWebStream.xml");
            }
        }

        public static int Port {
            get;
            set;
        }

        public static bool UseWebserver {
            get;
            set;
        }

        public static bool ManageTV4Home {
            get;
            set; 
        }

        public static int MonitorPollInterval {
            get { return 30; }
        }
        #endregion

        #region Persistence
        public static void Read() {
            if (!File.Exists(ConfigPath)) {
                // create default file
                Port = 8080;
                UseWebserver = true;
                ManageTV4Home = false;
                Write();
            }

            try {
                XmlDocument doc = new XmlDocument();
                doc.Load(ConfigPath);
                Port = Int32.Parse(doc.SelectSingleNode("/mpwebstream/port").InnerText);
                UseWebserver = doc.SelectSingleNode("/mpwebstream/useWebserver").InnerText == "true";
                ManageTV4Home = doc.SelectSingleNode("/mpwebstream/manageTV4Home").InnerText == "true";
            } catch (Exception ex) {
                Log.Write(ex);
            }
        }

        public static void Write() {
            try {
                if (!Directory.Exists(Path.GetDirectoryName(ConfigPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));

                XmlDocument doc = new XmlDocument();
                XmlNode root = doc.CreateElement("mpwebstream");
                AddChild(doc, root, "port", Port);
                AddChild(doc, root, "useWebserver", UseWebserver);
                AddChild(doc, root, "manageTV4Home", ManageTV4Home);
                doc.AppendChild(root);
                doc.Save(ConfigPath);
                Log.Info("MPWebStream: wrote config file!");
            } catch (Exception ex) {
                Log.Write(ex);
            }
        }

        private static void AddChild(XmlDocument doc, XmlNode parent, string key, string value) {
            XmlNode node = doc.CreateElement(key);
            node.InnerText = value;
            parent.AppendChild(node);
        }

        private static void AddChild(XmlDocument doc, XmlNode parent, string key, int value) {
            AddChild(doc, parent, key, value.ToString());
        }

        private static void AddChild(XmlDocument doc, XmlNode parent, string key, bool value) {
            AddChild(doc, parent, key, value ? "true" : "false");
        }
        #endregion
    }
}
