using System;
using System.Threading;
using System.IO;

namespace MPWebStream.Streaming {
    public class StreamCopy {
        private const int _defaultBufferSize = 8192;

        delegate void BlockingCopyDelegate(Stream original, Stream destination, int bufferSize);

        private static void BlockingCopy(Stream original, Stream destination, int bufferSize) {
            byte[] buffer = new byte[bufferSize];
            int read;
            while (true) {
                do {
                    read = original.Read(buffer, 0, buffer.Length);
                } while (read == 0 && original.CanRead);

                destination.Write(buffer, 0, read);
            }
        }

        private static void DoneCallback(IAsyncResult result) {
            // do nothing with it
        }

        public static void AsyncStreamCopy(Stream original, Stream destination) {
            BlockingCopyDelegate d = new BlockingCopyDelegate(StreamCopy.BlockingCopy);
            d.BeginInvoke(original, destination, _defaultBufferSize, new AsyncCallback(DoneCallback), new object());
        }
    }
}