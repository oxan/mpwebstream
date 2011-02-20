using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

namespace MPWebStream {
    class TranscoderProfile {
        public string Name { get; set; }
        public bool UseTranscoding { get; set; }
        public string Transcoder { get; set; }
        public string Parameters { get; set; }
        public string InputMethod { get; set; }
        public string OutputMethod { get; set; }
    }

    class Configuration {
        #region Properties
        public string SitePath {
            get {
                return Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"MPWebStream\Site");
            }
        }

        public string CassiniServerPath {
            get {
                return Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"MPWebStream\Cassini.exe");
            }
        }

        public string ConfigPath {
            get {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Team MediaPortal\MediaPortal TV Server\MPWebStream.xml");
            }
        }

        public int Port {
            get;
            set;
        }

        public bool UseWebserver {
            get;
            set;
        }

        public bool ManageTV4Home {
            get;
            set;
        }

        public string Username {
            get;
            set;
        }

        public string Password {
            get;
            set;
        }

        public bool EnableAuthentication {
            get;
            set;
        }

        public List<TranscoderProfile> Transcoders {
            get;
            set;
        }

        public string SiteRoot {
            get { return "http://mediastreamer.lan/MPWebStream/"; }
        }

        public int MonitorPollInterval {
            get { return 30; }
        }
        #endregion

        #region Constructor
        public Configuration() {
            Read();
        }
        #endregion

        #region Persistence
        protected void Read() {
            // default settings
            Port = 8080;
            UseWebserver = true;
            ManageTV4Home = false;
            Username = "admin";
            Password = "admin";
            EnableAuthentication = true;
            Transcoders = new List<TranscoderProfile>();
            Transcoders.Add(new TranscoderProfile() {
                Name = "Direct",
                UseTranscoding = false,
                Transcoder = "",
                Parameters = "",
                InputMethod = "NamedPipe",
                OutputMethod = "NamedPipe"
            });

            // create file if it doesn't exists
            if (!File.Exists(ConfigPath))
                Write();

            XmlDocument doc = new XmlDocument();
            doc.Load(ConfigPath);
            Port = Int32.Parse(doc.SelectSingleNode("/mpwebstream/port").InnerText);
            UseWebserver = doc.SelectSingleNode("/mpwebstream/useWebserver").InnerText == "true";
            ManageTV4Home = doc.SelectSingleNode("/mpwebstream/manageTV4Home").InnerText == "true";
            Username = doc.SelectSingleNode("/mpwebstream/username").InnerText;
            Password = doc.SelectSingleNode("/mpwebstream/password").InnerText;
            if(doc.SelectSingleNode("/mpwebstream/enableAuthentication") != null)
                EnableAuthentication = doc.SelectSingleNode("/mpwebstream/enableAuthentication").InnerText == "true";
            if (doc.SelectSingleNode("/mpwebstream/transcoders") != null) {
                Transcoders = new List<TranscoderProfile>();
                XmlNodeList nodes = doc.SelectNodes("/mpwebstream/transcoderProfiles/transcoder");
                foreach (XmlNode node in nodes) {
                    // Child nodes would be nicer, but that would mean a lot more work here. 
                    Transcoders.Add(new TranscoderProfile() {
                        Name = node.Attributes["name"].Value,
                        UseTranscoding = node.Attributes["useTranscoding"].Value == "true",
                        InputMethod = node.Attributes["inputMethod"].Value,
                        OutputMethod = node.Attributes["outputMethod"].Value,
                        Transcoder = node.Attributes["transcoder"].Value,
                        Parameters = node.Attributes["parameters"].Value
                    });
                }
            }
        }

        public void Write() {
            if (!Directory.Exists(Path.GetDirectoryName(ConfigPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));

            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.CreateElement("mpwebstream");
            AddChild(doc, root, "port", Port);
            AddChild(doc, root, "useWebserver", UseWebserver);
            AddChild(doc, root, "manageTV4Home", ManageTV4Home);
            AddChild(doc, root, "username", Username);
            AddChild(doc, root, "password", Password);
            AddChild(doc, root, "enableAuthentication", EnableAuthentication);
            doc.AppendChild(root);
            doc.Save(ConfigPath);
        }

        private void AddChild(XmlDocument doc, XmlNode parent, string key, string value) {
            XmlNode node = doc.CreateElement(key);
            node.InnerText = value;
            parent.AppendChild(node);
        }

        private void AddChild(XmlDocument doc, XmlNode parent, string key, int value) {
            AddChild(doc, parent, key, value.ToString());
        }

        private void AddChild(XmlDocument doc, XmlNode parent, string key, bool value) {
            AddChild(doc, parent, key, value ? "true" : "false");
        }
        #endregion
    }
}
