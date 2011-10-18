using System.Runtime.Serialization;
using TV4Home.Server.TVEInteractionLibrary.Interfaces;

namespace MPWebStream.Site.Service {
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
            this(ch.IdChannel, ch.DisplayName, ch.DisplayName, ch.IsTv, ch.IsRadio) {
        }
    }
}