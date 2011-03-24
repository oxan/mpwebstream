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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Web;
using TV4Home.Server.TVEInteractionLibrary.Interfaces;

namespace MPWebStream.Site {
    // FIXME: this should be possible without the DataContract attributes
    [DataContract]
    public class Channel {
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public int IdChannel { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public bool IsTv { get; set; }
        [DataMember]
        public bool IsRadio { get; set; }

        public Channel(int id, string displayName, string name, bool isTv, bool isRadio) {
            this.IdChannel = id;
            this.DisplayName = displayName;
            this.Name = name;
            this.IsTv = isTv;
            this.IsRadio = isRadio;
        }

        public Channel(WebChannelDetailed ch) : 
            this(ch.IdChannel, ch.DisplayName, ch.Name, ch.IsTv, ch.IsRadio) {
        }
    }

    [DataContract]
    public class Recording {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public int IdChannel { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public DateTime StartTime { get; set; }
        [DataMember]
        public DateTime EndTime { get; set; }

        public Recording(int id, int idChannel, string title, DateTime startTime, DateTime endTime) {
            this.Id = id;
            this.Title = title;
            this.IdChannel = idChannel;
            this.StartTime = startTime;
            this.EndTime = endTime;
        }

        public Recording(WebRecording rec)
            : this(rec.IdRecording, rec.IdChannel, rec.Title, rec.StartTime, rec.EndTime) {
        }
    }

    [DataContract]
    public class Transcoder {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public bool UsesTranscoding { get; set; }

        public Transcoder(int id, string name, bool usesTranscoding) {
            this.Id = id;
            this.Name = name;
            this.UsesTranscoding = usesTranscoding;
        }

        public Transcoder(TranscoderProfile transcoder) :
            this(transcoder.Id, transcoder.Name, transcoder.UseTranscoding) {
        }
    }

    public class MediaStream : IMediaStream {
        private ITVEInteraction client;
        private Configuration config;

        public MediaStream() {
            client = ChannelFactory<ITVEInteraction>.CreateChannel(new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 },
                new EndpointAddress("net.pipe://localhost/TV4Home.Server.CoreService/TVEInteractionService"));
            config = new Configuration();
        }

        public List<Transcoder> GetTranscoders() {
            List<Transcoder> result = new List<Transcoder>();
            foreach (TranscoderProfile transcoder in config.Transcoders)
                result.Add(new Transcoder(transcoder));
            return result;
        }

        public Transcoder GetTranscoderById(int id) {
            foreach (Transcoder transcoder in GetTranscoders()) {
                if (transcoder.Id == id) 
                    return transcoder;
            }
            return null;
        }

        public List<Channel> GetChannels() {
            List<Channel> result = new List<Channel>();
            List<WebChannelGroup> groups = client.GetGroups();
            foreach (WebChannelGroup group in groups)
                result.AddRange(client.GetChannelsDetailed(group.IdGroup).Select(ch => new Channel(ch)).ToList());
            return result;
        }

        public Channel GetChannel(int idChannel) {
            WebChannelDetailed ch = client.GetChannelDetailedById(idChannel);
            return new Channel(ch);
        }

        public string GetTvStreamUrl(int idChannel, string username, string password) {
            return CreateStreamUrl("channelId", idChannel, username, password);
        }

        public string GetTranscodedTvStreamUrl(int idChannel, string username, string password, int idTranscoder) {
            return CreateStreamUrl("channelId", idChannel, username, password, GetTranscoderById(idTranscoder));
        }

        public List<Recording> GetRecordings() {
            return client.GetRecordings().Select(rec => new Recording(rec)).ToList();
        }

        public string GetRecordingStreamUrl(int idRecording, string username, string password) {
            return CreateStreamUrl("recordingId", idRecording, username, password);
        }

        public string GetTranscodedRecordingStreamUrl(int idRecording, string username, string password, int idTranscoder) {
            return CreateStreamUrl("recordingId", idRecording, username, password, GetTranscoderById(idTranscoder));
        }

        private string CreateStreamUrl(string idKey, int idValue, string username, string password) {
            foreach (Transcoder transcoder in GetTranscoders()) {
                if (!transcoder.UsesTranscoding)
                    return CreateStreamUrl(idKey, idValue, username, password, transcoder);
            }
            return "";
        }

        private string CreateStreamUrl(string idKey, int idValue, string username, string password, Transcoder transcoder) {
            UriBuilder uri = new UriBuilder(config.SiteRoot);
            
            // add port if needed
            if (config.UseWebserver && config.Port != 80)
                uri.Port = config.Port;

            // parameters
            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString[idKey] = idValue.ToString();
            queryString["transcoder"] = transcoder.Id.ToString();
            if(username != string.Empty && config.EnableAuthentication)
                queryString["login"] = Authentication.createLoginArgument(username, password);

            // create
            if (uri.Path.Substring(uri.Path.Length - 1) != "/") 
                uri.Path += "/";
            uri.Path += "Stream.ashx";
            uri.Query = queryString.ToString();
            return uri.ToString();
        }
    }
}
