#region Copyright
/* 
 *  Copyright (C) 2010, 2011 Oxan
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#endregion

using System.Collections.Generic;
using System.ServiceModel;
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
