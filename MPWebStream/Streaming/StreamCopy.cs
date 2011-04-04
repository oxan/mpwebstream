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

using MPWebStream.Site;
using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;

namespace MPWebStream.Streaming {
    public class StreamCopy {
        private const int _defaultBufferSize = 8192;
        private static List<Thread> currentThreads = new List<Thread>();

        // non-static implementation class
        private string identifier;
        private Stream original;
        private Stream destination;
        private int bufferSize;
        private StreamCopy(string identifier, Stream original, Stream destination, int bufferSize) {
            this.identifier = identifier;
            this.original = original;
            this.destination = destination;
            this.bufferSize = bufferSize;
        }

        private void BlockingCopy() {
            Log.Write(" IN BC {0}", identifier);
            byte[] buffer = new byte[bufferSize];
            int read;
            while (true) {
                read = 0;
                do {
                    Log.Write("Reading from {0}, canRead {1}", original.ToString(), original.CanRead);
                    read = original.Read(buffer, 0, buffer.Length);
                    Log.Write("BC {0}: read {1}, canRead: {2}, canWrite: {3}", identifier, read, original.CanRead, destination.CanWrite);
                } while (read == 0 && original.CanRead && destination.CanWrite);
                Log.Write("BX {0}: read {1}, canRead: {2}, canWrite: {3}", identifier, read, original.CanRead, destination.CanWrite);
                if(destination.CanWrite)
                    destination.Write(buffer, 0, read);
            }
        }

        public static void AsyncStreamCopy(Stream original, Stream destination, string logIdentifier) {
            Log.Write("A {0}", logIdentifier);
            StreamCopy copy = new StreamCopy(logIdentifier, original, destination, _defaultBufferSize);
            Thread thread = new Thread(new ThreadStart(copy.BlockingCopy));
            thread.Start();
            currentThreads.Add(thread);
        }

        public static void AsyncStreamCopy(Stream original, Stream destination) {
            AsyncStreamCopy(original, destination, "");
        }
    }
}