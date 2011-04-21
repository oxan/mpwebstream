#region Copyright
/* 
 *  Copyright (C) 2011 Oxan
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
using System.Threading;

namespace MPWebStream.Site {
    class Log {
        private static TextWriter writer;
        private static object lockObj = new Object();

        static Log() {
            Configuration config = new Configuration();
            try {
                writer = new StreamWriter(config.LogFile, true);
            } catch (IOException) {
                // probably not a good path, just use some place to at least have a log and don't crash. 
                writer = new StreamWriter(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "mpwebstream.log"), true);
            }

            // register us for logs from MPWebStream.MediaTranscoding
            MPWebStream.MediaTranscoding.Log.RegisterWriter(new MPWebStream.MediaTranscoding.Log.LogWrite(Log.Write));
        }

        public static void Write(string message) {
            PerformWrite(message);
        }

        public static void Write(string message, params object[] arg) {
            PerformWrite(message, arg);
        }

        public static void Error(string message, params object[] arg) {
            PerformWrite(String.Format("ERROR: {0}", message), arg);
        }

        public static void Error(string message, Exception ex) {
            Error(message);
            PerformWrite(String.Format("Exception: {0}", ex));
        }

        private static void PerformWrite(string format, params object[] arg) {
            lock (lockObj) { // avoid multiple log entries on the same line and other weird stuff like that
                string text = string.Format(format, arg);
                bool first = true;
                foreach (string line in text.Split('\n')) {
                    writer.WriteLine("{0:yyyy-MM-dd HH:mm:ss.ffffff}: {1,-2}: {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, (!first ? "  " : "") + line.Trim());
                    first = false;
                }
                writer.Flush();
            }
        }
    }
}