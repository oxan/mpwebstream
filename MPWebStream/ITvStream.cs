using System.Collections.Generic;
using System.ServiceModel;

namespace MPWebStream.Site {
    [ServiceContract]
    public interface ITvStream {
        [OperationContract]
        List<Channel> GetChannels();
        [OperationContract]
        string GetStreamUrl(int idChannel, string username, string password);
    }
}
