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
using System.IO;
using System.Xml;
using System.Collections.Generic;
using MPWebStream.MediaTranscoding;

namespace MPWebStream {
    internal enum TranscoderProfileType {
        System,
        User
    }

    internal class ExtendedTranscoderProfile : TranscoderProfile {
        public int Id { get; set; }
        public TranscoderProfileType Type { get; set; }

        public bool DataEquals(ExtendedTranscoderProfile cmp) {
            if (cmp == null)
                return false;
            return cmp.Name == this.Name &&
                cmp.UseTranscoding == this.UseTranscoding &&
                cmp.Transcoder == this.Transcoder &&
                cmp.Parameters == this.Parameters &&
                cmp.InputMethod == this.InputMethod &&
                cmp.OutputMethod == this.OutputMethod &&
                cmp.MIME == this.MIME &&
                cmp.Id == this.Id;
        }
    }

    class Configuration {
        #region Properties
        public string BasePath {
            get {
                // different detection needed from MPWebStream and the TvServerPlugin
                if (AppDomain.CurrentDomain.BaseDirectory.IndexOf(@"MPWebStream\Site") == -1)
                    // called from the TvServerPlugin in MP\Plugins, so add MPWebStream to it
                    return Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "MPWebStream");

                // called from the ASP.NET app (in MP\Plugins\MPWebStream\Site\), so remove the Site part
                string sitedir = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
                return Path.GetDirectoryName(sitedir.Substring(0, sitedir.Length - 4)); // sanitize
            }
        }

        public string SitePath {
            get {
                return Path.Combine(BasePath, "Site");
            }
        }

        public string CassiniServerPath {
            get {
                return Path.Combine(BasePath, "Cassini.exe");
            }
        }

        public string ConfigPath {
            get {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Team MediaPortal\MediaPortal TV Server\MPWebStream.xml");
            }
        }

        public string LogFile {
            get;
            set;
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

        public List<ExtendedTranscoderProfile> Transcoders {
            get;
            set;
        }

        public string SiteRoot {
            get;
            set;
        }

        public bool TranscoderLog {
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
            LogFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Team MediaPortal", "MediaPortal TV Server", "log", "mpwebstream.log");
            TranscoderLog = true;
            Transcoders = new List<ExtendedTranscoderProfile>();
            Transcoders.Add(new ExtendedTranscoderProfile() {
                Name = "Direct",
                UseTranscoding = false,
                Transcoder = "",
                Parameters = "",
                InputMethod = TransportMethod.NamedPipe,
                OutputMethod = TransportMethod.NamedPipe,
                Id = 1,
                MIME = "video/MP2T"
            });

            // default transcoders
            Dictionary<string, string> configurations = new Dictionary<string, string>();
            configurations["H264, 426x240, 250Kbit/s"] = "-y -i %in -s 426x240 -b 175k -vcodec libx264 -vprofile baseline -preset ultrafast -acodec aac -strict experimental -ab 64k -threads %threads -async 1 -f mpegts %out";
            configurations["H264, 426x240, 500Kbit/s"] = "-y -i %in -s 426x240 -b 400k -vcodec libx264 -vprofile baseline -preset ultrafast -acodec aac -strict experimental -ab 64k -threads %threads -async 1 -f mpegts %out";
            configurations["H264, 426x240, 1Mbit/s"] = "-y -i %in -s 426x240 -b 700k -vcodec libx264 -vprofile baseline -preset ultrafast -acodec aac -strict experimental -ab 128k -threads %threads -async 1 -f mpegts %out";
            configurations["H264, WVGA, 1.5Mbit/s"] = "-y -i %in -s wvga -b 1400k -vcodec libx264 -vprofile baseline -preset ultrafast -acodec aac -strict experimental -ab 128k -threads %threads -async 1 -f mpegts %out";
            configurations["H264, WVGA, 2Mbit/s"] = "-y -i %in -s wvga -b 1800k -vcodec libx264 -vprofile baseline -preset ultrafast -acodec aac -strict experimental -ab 128k -threads %threads -async 1 -f mpegts %out";
            configurations["H264, 720p, 5Mbit/s"] = "-y -i %in -s hd720 -b 4700k -vcodec libx264 -vprofile baseline -preset ultrafast -acodec aac -strict experimental -ab 192k -threads %threads -async 1 -f mpegts %out";
            configurations["H264, 5Mbit/s"] = "-y -i %in -b 4700k -vcodec libx264 -vprofile baseline -preset ultrafast -acodec aac -strict experimental -ab 192k -threads %threads -async 1 -f mpegts %out";
            configurations["H264, 10Mbit/s"] = "-y -i %in -b 9500k -vcodec libx264 -vprofile baseline -preset ultrafast -acodec aac -strict experimental -ab 256k -threads %threads -async 1 -f mpegts %out";
            int i = 2;
            foreach (KeyValuePair<string, string> config in configurations) {
                Transcoders.Add(new ExtendedTranscoderProfile() {
                    Name = config.Key,
                    UseTranscoding = true,
                    Transcoder = Path.Combine(BasePath, @"ffmpeg\bin\ffmpeg.exe"),
                    Parameters = config.Value.Replace("%threads", Environment.ProcessorCount.ToString()).Replace("%in", "\"{0}\"").Replace("%out", "\"{1}\""),
                    InputMethod = TransportMethod.NamedPipe,
                    OutputMethod = TransportMethod.NamedPipe,
                    Id = i,
                    MIME = "video/MP2T",
                    Type = TranscoderProfileType.System
                });
                i++;
            }

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
            if (doc.SelectSingleNode("/mpwebstream/logfile") != null)
                LogFile = doc.SelectSingleNode("/mpwebstream/logfile").InnerText;
            if (doc.SelectSingleNode("/mpwebstream/transcoderlog") != null)
                TranscoderLog = doc.SelectSingleNode("/mpwebstream/transcoderlog").InnerText == "true";
            if (doc.SelectSingleNode("/mpwebstream/transcoderProfiles") != null) {
                Transcoders = new List<ExtendedTranscoderProfile>();
                XmlNodeList nodes = doc.SelectNodes("/mpwebstream/transcoderProfiles/transcoder");
                foreach (XmlNode node in nodes) {
                    // TODO: this can be done easier
                    ExtendedTranscoderProfile transcoder = new ExtendedTranscoderProfile();
                    transcoder.Type = TranscoderProfileType.User;
                    foreach (XmlNode child in node.ChildNodes) {
                        if (child.Name == "name") transcoder.Name = child.InnerText;
                        if (child.Name == "useTranscoding") transcoder.UseTranscoding = child.InnerText == "true";
                        if (child.Name == "inputMethod") transcoder.InputMethod = (TransportMethod)Enum.Parse(typeof(TransportMethod), child.InnerText, true);
                        if (child.Name == "outputMethod") transcoder.OutputMethod = (TransportMethod)Enum.Parse(typeof(TransportMethod), child.InnerText, true);
                        if (child.Name == "transcoder") transcoder.Transcoder = child.InnerText;
                        if (child.Name == "parameters") transcoder.Parameters = child.InnerText;
                        if (child.Name == "mime") transcoder.MIME = child.InnerText;
                        if (child.Name == "id") transcoder.Id = Int32.Parse(child.InnerText);
                        if (child.Name == "type") transcoder.Type = (TranscoderProfileType)Enum.Parse(typeof(TranscoderProfileType), child.InnerText, true);
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
            AddChild(doc, root, "logfile", LogFile);
            AddChild(doc, root, "transcoderlog", TranscoderLog);

            XmlNode transcoders = doc.CreateElement("transcoderProfiles");
            foreach (ExtendedTranscoderProfile profile in Transcoders) {
                XmlNode thisTranscoder = doc.CreateElement("transcoder");
                AddChild(doc, thisTranscoder, "name", profile.Name);
                AddChild(doc, thisTranscoder, "useTranscoding", profile.UseTranscoding);
                AddChild(doc, thisTranscoder, "inputMethod", Enum.GetName(typeof(TransportMethod), profile.InputMethod));
                AddChild(doc, thisTranscoder, "outputMethod", Enum.GetName(typeof(TransportMethod), profile.OutputMethod));
                AddChild(doc, thisTranscoder, "transcoder", profile.Transcoder);
                AddChild(doc, thisTranscoder, "parameters", profile.Parameters);
                AddChild(doc, thisTranscoder, "mime", profile.MIME);
                AddChild(doc, thisTranscoder, "id", profile.Id);
                AddChild(doc, thisTranscoder, "type", Enum.GetName(typeof(TranscoderProfileType), profile.Type));
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
        public ExtendedTranscoderProfile GetTranscoder(int transcoderId) {
            foreach (ExtendedTranscoderProfile profile in Transcoders) {
                if (profile.Id == transcoderId)
                    return profile;
            }
            return null;
        }
        #endregion
    }
}
