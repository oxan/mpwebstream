/*
 * CopyStreamToStream taken from: http://msdn.microsoft.com/en-us/magazine/cc337900.aspx
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;


namespace MPWebStream.Streaming
{
    public abstract class TransportStream : Stream
    {
        private byte[] buffer;

        public abstract String Url { get; }
        public abstract Boolean IsReady { get; }
        public abstract void Start(Boolean isClient);
        public abstract Stream UnderlyingStreamObject { get; set; }

        public void CopyStream(Stream media)
        {
            // Make sure the stream is ready first.
            int tries = 10000; // 10 Seconds
            do
            {
                if (this.IsReady)
                    break;

                System.Threading.Thread.Sleep(1);
            } while (--tries != 0);

            if (IsReady)
            {
                buffer = new byte[0x1000];

                media.BeginRead(buffer, 0, buffer.Length, MediaReadAsyncCallback, media);
            }
            else
            {
                throw new Exception("Pipe Stream isn't ready.");
            }
        }

        private void MediaReadAsyncCallback(IAsyncResult ar)
        {
            try
            {
                Stream media = ar.AsyncState as Stream;

                int read = media.EndRead(ar);
                if (read > 0)
                {
                    this.BeginWrite(buffer, 0, read, writeResult =>
                    {
                        try
                        {
                            this.EndWrite(writeResult);
                            media.BeginRead(
                                buffer, 0, buffer.Length, MediaReadAsyncCallback, media);
                        }
                        catch (Exception exc)
                        {
                            System.Diagnostics.Debug.WriteLine("Stream copy complete.");
                            System.Diagnostics.Debug.WriteLine(String.Format("Exception: {0}", exc.Message));
                        }
                    }, null);
                }
                else
                {
                    return;
                }
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine("Stream copy complete.");
                System.Diagnostics.Debug.WriteLine(String.Format("Exception: {0}", exc.Message));
            }
        }

    }
}
