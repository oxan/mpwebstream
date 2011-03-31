#region Copyright
/* 
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
                } while (read == 0 && original.CanRead && destination.CanWrite);

                if(destination.CanWrite)
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