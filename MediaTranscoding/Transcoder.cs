#region Copyright
/* 
 *  Copyright (C) 2010, 2011 Oxan
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
using System.Linq;
using System.IO;
using System.Web;
using System.Collections.Generic;

namespace MPWebStream.MediaTranscoding {
    internal class Transcoder {
        public TranscoderProfile Profile { get; set; }
        public string Source { get; set; }
        public bool WantLogStream { get; set; }

        public Stream LogStream {
            get {
                return logStream;
            }
        }

        public bool IsRunning {
            get {
                return Profile.UseTranscoding ? encoder.TranscoderRunning : true;
            }
        }

        private Stream logStream;
        private Encoder encoder;

        ~Transcoder() {
            StopTranscoding();
        }

        public void StartTranscoding() {
            // encoder configuration
            Log.Write("Starting transcoding of {0} with transcoder named {1}", Source, Profile.Name);

            // create encoder
            encoder = new Encoder(Profile, Source);
            encoder.WantTranscoderInfo = Profile.UseTranscoding && WantLogStream;
            encoder.StartTranscode();
            if (encoder.WantTranscoderInfo)
                logStream = encoder.TranscoderInfoOutputStream;
            Log.Write("Transcoding started!");
        }

        public void StreamToStream(Stream output) {
            encoder.StartStreaming();

            // do the actually copy to the output
            if (encoder.TranscoderOutputStream != null && output != null) {
                Log.Write("Copy transcoder output stream of type {0} into output stream of type {1}", encoder.TranscoderOutputStream.ToString(), output.ToString());
                if (encoder.TranscoderOutputStream is NamedPipe)
                    ((NamedPipe)encoder.TranscoderOutputStream).WaitTillReady();
                StreamCopy.AsyncStreamCopy(encoder.TranscoderOutputStream, output, "transoutput");
            }

            Log.Write("Streaming started!");
        }

        public Stream GetVideoStream() {
            encoder.StartStreaming();
            return encoder.TranscoderOutputStream;
        }

        public void StopTranscoding() {
            Log.Write("Finishing transcoding");
            try {
                if (encoder != null) 
                    encoder.StopTranscode();
            } catch (Exception e) {
                Log.Error("Stopping transcoder failed", e);
            }
        }
    }
}