using System;
using System.Runtime.Serialization;
using TV4Home.Server.TVEInteractionLibrary.Interfaces;

namespace MPWebStream.Site.Service {
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
}