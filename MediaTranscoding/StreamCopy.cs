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
        private byte[] buffer;
        private Stream src;
        private Stream dest;

        public StreamCopy(Stream A, Stream B) {
            src = A;
            dest = B;
        }

        private void CopyStream() {
            buffer = new byte[0x1000];
            src.BeginRead(buffer, 0, buffer.Length, MediaReadAsyncCallback, src);
        }

        private void MediaReadAsyncCallback(IAsyncResult ar) {
            try {
                Stream media = ar.AsyncState as Stream;

                int read = media.EndRead(ar);
                if (read > 0) {
                    dest.BeginWrite(buffer, 0, read, writeResult => {
                        try {
                            dest.EndWrite(writeResult);
                            media.BeginRead(
                                buffer, 0, buffer.Length, MediaReadAsyncCallback, media);
                        } catch (Exception exc) {
                            System.Diagnostics.Debug.WriteLine("Stream copy complete.");
                            System.Diagnostics.Debug.WriteLine(String.Format("Exception: {0}", exc.Message));
                        }
                    }, null);
                } else {
                    return;
                }
            } catch (Exception exc) {
                System.Diagnostics.Debug.WriteLine("Stream copy complete.");
                System.Diagnostics.Debug.WriteLine(String.Format("Exception: {0}", exc.Message));
            }
        }

        public static void AsyncStreamCopy(Stream original, Stream destination, string logIdentifier) {
            StreamCopy copy = new StreamCopy(original, destination);
            copy.CopyStream();
        }

        public static void AsyncStreamCopy(Stream original, Stream destination) {
            AsyncStreamCopy(original, destination, "");
        }
    }
}