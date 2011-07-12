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
using System.Collections.Generic;
using System.Threading;

namespace MPWebStream.MediaTranscoding {
    public class Log {
        public enum LogLevel
        {
            Trace = 0,
            Debug = 1,
            Info = 2,
            Warn = 3,
            Error = 4,
            Fatal = 5
        }
        public delegate void LogWrite(int logLevel, string message);

        private static LogWrite callback = null;
        private static object lockObj = new Object();

        public static void RegisterWriter(LogWrite writer) {
            callback = writer;
        }

        internal static void Trace(string message, params object[] arg)
        {
            PerformWrite(LogLevel.Trace, message, arg);
        }

        internal static void Debug(string message, params object[] arg)
        {
            PerformWrite(LogLevel.Debug, message, arg);
        }

        internal static void Info(string message, params object[] arg)
        {
            PerformWrite(LogLevel.Info, message, arg);
        }

        internal static void Warn(string message, params object[] arg)
        {
            PerformWrite(LogLevel.Warn, message, arg);
        }

        internal static void Error(string message, params object[] arg)
        {
            PerformWrite(LogLevel.Error, message, arg);
        }

        internal static void Fatal(string message, params object[] arg)
        {
            PerformWrite(LogLevel.Fatal, message, arg);
        }

        internal static void Write(string message, params object[] arg) {
            PerformWrite(LogLevel.Info, message, arg);
        }

        internal static void Error(string message, Exception ex) {
            Error(message);
            PerformWrite(LogLevel.Error, String.Format("Exception: {0}", ex)); 
        }

        private static void PerformWrite(LogLevel logLevel, string format, params object[] arg)
        {
            string text = string.Format(format, arg);
            if (callback != null) {
                lock (lockObj)  // avoid calling it in concurrent
                    callback.Invoke((int)logLevel, text);
            }
        }
    }
}