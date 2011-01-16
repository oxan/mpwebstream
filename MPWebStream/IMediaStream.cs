using System.Collections.Generic;
using System.ServiceModel;

namespace MPWebStream.Site {
    [ServiceContract]
    public interface IMediaStream {
        [OperationContract]
        List<Channel> GetChannels();
        [OperationContract]
        string GetTvStreamUrl(int idChannel, string username, string password);
        [OperationContract]
        List<Recording> GetRecordings();
        [OperationContract]
        string GetRecordingStreamUrl(int idRecording, string username, string password);
    }
}
