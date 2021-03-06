﻿#region Copyright
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
using System.Web;
using System.Threading;

namespace MPWebStream.MediaTranscoding {
    class StreamCopy {
        private const int _defaultBufferSize = 0x10000;
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

        private void CopyStream(bool retry) {
            // do a parallel read
            buffer = new byte[bufferSize];
            try {
                source.BeginRead(buffer, 0, buffer.Length, MediaReadAsyncCallback, new object());
            } catch (NotSupportedException e) {
                // we only do a workaround for TsBuffer here, nothing for other errors
                if (!(source is TsBuffer))
                    throw e;

                TsBuffer stream = (TsBuffer)source;
                Log.Error(string.Format("StreamCopy {0}: NotSupportedException when trying to read from TsBuffer", log), e);
                Log.Write("StreamCopy {0}: TsBuffer dump: CanRead {1}, CanWrite {2}", log, stream.CanRead, stream.CanWrite);
                Log.Write("StreamCopy {0}:\r\n{1}", log, stream.DumpStatus());
                if (retry) {
                    Thread.Sleep(500);
                    Log.Write("StreamCopy {0}: Trying to recover", log);
                    CopyStream(false);
                }
            }
        }

        private void CopyStream() {
            CopyStream(true);
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
                        HandleException(e, "inner");
                    }
                }, null);
            } catch (Exception e) {
                HandleException(e, "outer");
            }
        }

        private void HandleException(Exception e, string type) {
            if (e is IOException) {
                // end of pipe etc
                Log.Write("StreamCopy {0}: IOException in {1} stream copy, is usually ok: {2}", log, type, e.Message);
            } else if (e is HttpException) {
                // client disconnected, picked up by TranscodingStreamer.TranscodeToClient
                Log.Write("StreamCopy {0}: HttpException in {1} stream copy, is usually ok: {2}", log, type, e.Message);
            } else {
                Log.Error(string.Format("StreamCopy {0}: Failure in {1} stream copy", log, type), e);
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