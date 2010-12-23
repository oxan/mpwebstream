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
            this.IsTv = isTv;
            this.IsRadio = isRadio;
        }

        public Channel(WebChannel ch) : this(ch.IdChannel, ch.DisplayName, ch.Name, ch.IsTv, ch.IsRadio) {
        }

    }

    public class TvStream : ITvStream {
        private ITVEInteraction client;

        public TvStream() {
            client = ChannelFactory<ITVEInteraction>.CreateChannel(new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 },
                new EndpointAddress("net.pipe://localhost/TV4Home.Server.CoreService/TVEInteractionService"));
        }

        public List<Channel> GetChannels() {
            // FIXME: use LINQ for this? 
            List<Channel> result = new List<Channel>();
            List<WebChannelGroup> groups = client.GetGroups();
            foreach (WebChannelGroup group in groups) {
                List<WebChannel> channels = client.GetChannels(group.IdGroup);
                foreach (WebChannel channel in channels) {
                    result.Add(new Channel(channel));
                }
            }
            return result;
        }

        public string GetStreamUrl(int idChannel, string username, string password) {
            Uri uri = OperationContext.Current.IncomingMessageHeaders.To;
            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["channelId"] = idChannel.ToString();
            queryString["login"] = Authentication.createLoginArgument(username, password);
            Uri stream = new Uri(uri, "/Stream.ashx?" + queryString.ToString());
            return stream.ToString();
        }
    }
}
