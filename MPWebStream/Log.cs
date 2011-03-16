﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace MPWebStream.Site {
    public class Log {
        static TextWriter writer;

        static Log() {
            Configuration config = new Configuration();
            try {
                writer = new StreamWriter(config.LogFile);
            } catch (IOException) {
                // probably not a valid path, just use some place to at least have a log and don't crash. 
                writer = new StreamWriter(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "mpwebstream.log"));
            }
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
            string text = string.Format(format, arg);
            writer.WriteLine("{0:yyyy-MM-dd HH:mm:ss.ffffff}: {1}", DateTime.Now, text);
            writer.Flush();
        }
    }
}