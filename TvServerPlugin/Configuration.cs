using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

namespace MPWebStream {
    public enum TransportMethod {
        Filename,
        NamedPipe,
        StandardIn,
        StandardOut
    }

    public class TranscoderProfile {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool UseTranscoding { get; set; }
        public string Transcoder { get; set; }
        public string Parameters { get; set; }
        public TransportMethod InputMethod { get; set; }
        public TransportMethod OutputMethod { get; set; }
    }

    public class Configuration {
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
            get;
            set;
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
            ManageTV4Home = true;
            Username = "admin";
            Password = "admin";
            EnableAuthentication = true;
            SiteRoot = "http://" + System.Environment.MachineName + "/";
            Transcoders = new List<TranscoderProfile>();
            Transcoders.Add(new TranscoderProfile() {
                Name = "Direct",
                UseTranscoding = false,
                Transcoder = "",
                Parameters = "",
                InputMethod = TransportMethod.NamedPipe,
                OutputMethod = TransportMethod.NamedPipe,
                Id = 1
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
            if (doc.SelectSingleNode("/mpwebstream/enableAuthentication") != null)
                EnableAuthentication = doc.SelectSingleNode("/mpwebstream/enableAuthentication").InnerText == "true";
            if (doc.SelectSingleNode("/mpwebstream/siteroot") != null)
                SiteRoot = doc.SelectSingleNode("/mpwebstream/siteroot").InnerText;
            if (doc.SelectSingleNode("/mpwebstream/transcoderProfiles") != null) {
                Transcoders = new List<TranscoderProfile>();
                XmlNodeList nodes = doc.SelectNodes("/mpwebstream/transcoderProfiles/transcoder");
                foreach (XmlNode node in nodes) {
                    // TODO: this can be done easier
                    TranscoderProfile transcoder = new TranscoderProfile();
                    foreach (XmlNode child in node.ChildNodes) {
                        if (child.Name == "name") transcoder.Name = child.InnerText;
                        if (child.Name == "useTranscoding") transcoder.UseTranscoding = child.InnerText == "true";
                        if (child.Name == "inputMethod") transcoder.InputMethod = (TransportMethod)Enum.Parse(typeof(TransportMethod), child.InnerText, true);
                        if (child.Name == "outputMethod") transcoder.OutputMethod = (TransportMethod)Enum.Parse(typeof(TransportMethod), child.InnerText, true);
                        if (child.Name == "transcoder") transcoder.Transcoder = child.InnerText;
                        if (child.Name == "parameters") transcoder.Parameters = child.InnerText;
                        if (child.Name == "id") transcoder.Id = Int32.Parse(child.InnerText);
                    }
                    Transcoders.Add(transcoder);
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
            AddChild(doc, root, "siteroot", SiteRoot);

            XmlNode transcoders = doc.CreateElement("transcoderProfiles");
            foreach (TranscoderProfile profile in Transcoders) {
                XmlNode thisTranscoder = doc.CreateElement("transcoder");
                AddChild(doc, thisTranscoder, "name", profile.Name);
                AddChild(doc, thisTranscoder, "useTranscoding", profile.UseTranscoding);
                AddChild(doc, thisTranscoder, "inputMethod", Enum.GetName(typeof(TransportMethod), profile.InputMethod));
                AddChild(doc, thisTranscoder, "outputMethod", Enum.GetName(typeof(TransportMethod), profile.OutputMethod));
                AddChild(doc, thisTranscoder, "transcoder", profile.Transcoder);
                AddChild(doc, thisTranscoder, "parameters", profile.Parameters);
                AddChild(doc, thisTranscoder, "id", profile.Id);
                transcoders.AppendChild(thisTranscoder);
            }
            root.AppendChild(transcoders);
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

        #region Easy access methods
        public TranscoderProfile GetTranscoder(int transcoderId) {
            foreach (TranscoderProfile profile in Transcoders) {
                if (profile.Id == transcoderId)
                    return profile;
            }
            return null;
        }
        #endregion
    }
}
