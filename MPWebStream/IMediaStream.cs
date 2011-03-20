using System;
using System.ServiceModel;
using System.Collections.Generic;
using System.ServiceModel.Web;

namespace MPWebStream.Site {
    [ServiceContract]
    public interface IMediaStream {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, 
            UriTemplate = "GetChannel?idChannel={idChannel}")]
        Channel GetChannel(int idChannel);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<Channel> GetChannels();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<Recording> GetRecordings();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, 
            UriTemplate = "GetRecordingStreamUrl?idRecording={idRecording}&username={username}&password={password}")]
        string GetRecordingStreamUrl(int idRecording, string username, string password);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, 
            UriTemplate = "GetTranscodedRecordingStreamUrl?idRecording={idRecording}&username={username}&password={password}&idTranscoder={idTranscoder}")]
        string GetTranscodedRecordingStreamUrl(int idRecording, string username, string password, int idTranscoder);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, 
            UriTemplate = "GetTranscodedTvStreamUrl?idChannel={idChannel}&username={username}&password={password}&idTranscoder={idTranscoder}")]
        string GetTranscodedTvStreamUrl(int idChannel, string username, string password, int idTranscoder);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, 
            UriTemplate = "GetTranscoderById?id={id}")]
        Transcoder GetTranscoderById(int id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<Transcoder> GetTranscoders();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, 
            UriTemplate = "GetTvStreamUrl?idChannel={idChannel}&username={username}&password={password}")]
        string GetTvStreamUrl(int idChannel, string username, string password);
    }
}
