using MPWebStream.MediaTranscoding;
using System.Runtime.Serialization;

namespace MPWebStream.Site.Service {
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

        public Transcoder(ExtendedTranscoderProfile transcoder) :
            this(transcoder.Id, transcoder.Name, transcoder.UseTranscoding) {
        }
    }
}