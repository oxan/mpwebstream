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

    public class MediaStream : IMediaStream {
        private ITVEInteraction client;

        public MediaStream() {
            client = ChannelFactory<ITVEInteraction>.CreateChannel(new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 },
                new EndpointAddress("net.pipe://localhost/TV4Home.Server.CoreService/TVEInteractionService"));
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

        public List<Recording> GetRecordings() {
            return client.GetRecordings().Select(rec => new Recording(rec)).ToList();
        }

        public string GetRecordingStreamUrl(int idRecording, string username, string password) {
            return CreateStreamUrl("recordingId", idRecording, username, password);
        }

        private string CreateStreamUrl(string idKey, int idValue, string username, string password) {
            Configuration config = new Configuration();
            Uri baseUri = new Uri(config.SiteRoot);
            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString[idKey] = idValue.ToString();
            if(username != string.Empty)
                queryString["login"] = Authentication.createLoginArgument(username, password);
            Uri stream = new Uri(baseUri, "Stream.ashx?" + queryString.ToString());
            return stream.ToString();
        }
    }
}
