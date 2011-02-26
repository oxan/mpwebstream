using System;
using System.ServiceModel;

namespace MPWebStream.Site {
    [ServiceContract]
    interface IMediaStream {
        [OperationContract]
        Channel GetChannel(int idChannel);

        [OperationContract]
        System.Collections.Generic.List<Channel> GetChannels();

        [OperationContract]
        System.Collections.Generic.List<Recording> GetRecordings();

        [OperationContract]
        string GetRecordingStreamUrl(int idRecording, string username, string password);

        [OperationContract]
        string GetTranscodedRecordingStreamUrl(int idRecording, string username, string password, int idTranscoder);

        [OperationContract]
        string GetTranscodedTvStreamUrl(int idChannel, string username, string password, int idTranscoder);

        [OperationContract]
        Transcoder GetTranscoderById(int id);

        [OperationContract]
        System.Collections.Generic.List<Transcoder> GetTranscoders();

        [OperationContract]
        string GetTvStreamUrl(int idChannel, string username, string password);
    }
}
