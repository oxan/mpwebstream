#region Copyright
/* 
 *  Copyright (C) 2008, 2009 StreamTv, http://code.google.com/p/mpstreamtv/
 *  Copyright (C) 2011, Oxan
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

using System;
using System.IO;

namespace MPWebStream.MediaTranscoding {
    class StreamCopy {
        private const int _defaultBufferSize = 4096;

        private byte[] buffer;
        private Stream source;
        private Stream destination;
        private int bufferSize;
        private string log;

        private StreamCopy(Stream source, Stream destination, int bufferSize, string log) {
            this.source = source;
            this.destination = destination;
            this.bufferSize = bufferSize;
            this.log = log;
        }

        private void CopyStream() {
            // do a parallel read
            buffer = new byte[bufferSize];
            source.BeginRead(buffer, 0, buffer.Length, MediaReadAsyncCallback, new object());
        }

        private void MediaReadAsyncCallback(IAsyncResult ar) {
            try {
                int read = source.EndRead(ar);
                if (read == 0) // we're done
                    return;

                // write it to the destination
                destination.BeginWrite(buffer, 0, read, writeResult => {
                    try {
                        destination.EndWrite(writeResult);

                        // and read again...
                        source.BeginRead(buffer, 0, buffer.Length, MediaReadAsyncCallback, new object());
                    } catch (Exception e) {
                        Log.Error("StreamCopy {0}: Failure in inner stream copy", log);
                        Log.Error("Exception", e);
                    }
                }, null);
            } catch (Exception e) {
                Log.Error("StreamCopy {0}: Failure in outer stream copy", log);
                Log.Error("Exception", e);
            }
        }

        public static void AsyncStreamCopy(Stream original, Stream destination, string logIdentifier, int bufferSize) {
            StreamCopy copy = new StreamCopy(original, destination, bufferSize, logIdentifier);
            copy.CopyStream();
        }

        public static void AsyncStreamCopy(Stream original, Stream destination, string logIdentifier) {
            AsyncStreamCopy(original, destination, logIdentifier, _defaultBufferSize);
        }

        public static void AsyncStreamCopy(Stream original, Stream destination) {
            AsyncStreamCopy(original, destination, "", _defaultBufferSize);
        }
    }
}