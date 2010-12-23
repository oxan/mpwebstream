using System.Collections.Generic;
using System.ServiceModel;
using System.Linq;
using TV4Home.Server.TVEInteractionLibrary.Interfaces;

namespace MPWebStream.Site {
    public class Channel {
        public string DisplayName { get; set; }
        public int IdChannel { get; set; }
        public string Name { get; set; }
        public bool IsTv { get; set; }
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

        public string GetStreamUrl(int idChannel) {
            return "";
        }
    }
}
