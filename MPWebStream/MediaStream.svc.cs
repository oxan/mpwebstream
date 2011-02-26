using System.Collections.Specialized;
using System.Collections.Generic;
using System.ServiceModel;
using System.Linq;
using System.Runtime.Serialization;
using System;
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

        public Transcoder(string name, bool usesTranscoding) {
            this.Id = 5;
            this.Name = name;
            this.UsesTranscoding = usesTranscoding;
        }

        public Transcoder(TranscoderProfile transcoder) :
            this(transcoder.Name, transcoder.UseTranscoding) {
        }
    }

    public class MediaStream : IMediaStream, MPWebStream.Site.IMediaStream {
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
            Uri baseUri = new Uri(config.SiteRoot);
            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);

            // parameters
            queryString[idKey] = idValue.ToString();
            queryString["transcoder"] = transcoder.Name;
            if(username != string.Empty)
                queryString["login"] = Authentication.createLoginArgument(username, password);

            // build
            Uri stream = new Uri(baseUri, "Stream.ashx?" + queryString.ToString());
            return stream.ToString();
        }
    }
}
