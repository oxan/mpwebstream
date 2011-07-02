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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;

namespace MPWebStream.MediaTranscoding {
    public class Transcoder {
        public TranscoderProfile Profile { get; set; }
        public string Source { get; set; }
        public Pipeline Pipeline { get; private set; }
        public Reference<FFmpegEncodingInfo> EncodingInfo { get; set; }

        public void BuildPipeline() {
            Pipeline = new Pipeline();

            // dispatch the .Path method
            bool isTsBuffer = Source.IndexOf(".ts.tsbuffer") != -1;
            if(Profile.InputMethod == TransportMethod.Path)
                Profile.InputMethod = isTsBuffer ? TransportMethod.NamedPipe : TransportMethod.Filename;
            if(Profile.OutputMethod == TransportMethod.Path)
                Profile.OutputMethod = TransportMethod.NamedPipe;

            // add input processing unit to pipeline if needed
            bool needInputRead =
                Profile.InputMethod == TransportMethod.StandardIn ||
                Profile.InputMethod == TransportMethod.NamedPipe ||
                !Profile.UseTranscoding;
            if(needInputRead)
                Pipeline.AddDataProcessingUnit(new InputProcessingUnit(Source), 1);

            // add processing unit to pipeline if wanted
            if (Profile.UseTranscoding) {
                EncodingProcessingUnit encoder;
                if (needInputRead) {
                    encoder = new EncodingProcessingUnit(Profile.Transcoder, Profile.Parameters, Profile.InputMethod, Profile.OutputMethod);
                } else {
                    encoder = new EncodingProcessingUnit(Profile.Transcoder, Profile.Parameters, Profile.InputMethod, Profile.OutputMethod, Source);
                }
                Pipeline.AddDataProcessingUnit(encoder, 2);
            }

            // add ffmpeg output parsing
            if (EncodingInfo != null)
                Pipeline.AddLogProcessingUnit(new FFmpegOutputParsingProcessingUnit(EncodingInfo), 3);
        }

        public void TranscodeToClient(HttpResponse response) {
            TranscodeToClientImplementation(response);
        }

        public void TranscodeToClient(HttpResponseBase response) {
            TranscodeToClientImplementation(response);
        }

        private void TranscodeToClientImplementation(dynamic response) {
            if(Pipeline == null)
                BuildPipeline();
            Pipeline.AddDataProcessingUnit(new HttpOutputProcessingUnit(Pipeline, Profile.MIME, response), 3);
            Pipeline.RunBlocking();
            Pipeline.Stop();
        }

        public void StartTranscodeToStream(Stream outputStream) {
            if (Pipeline == null)
                BuildPipeline();
            Pipeline.AddDataProcessingUnit(new StreamCopyProcessingUnit(outputStream, "transoutput"), 5);
            Pipeline.Start();
        }

        public Stream StartStream() {
            if (Pipeline == null)
                BuildPipeline();
            PassthroughProcessingUnit unit = new PassthroughProcessingUnit();
            Pipeline.AddDataProcessingUnit(unit, 5);
            Pipeline.Start();
            return unit.DataOutputStream;
        }

        public void StopTranscoding() {
            Pipeline.Stop();
        }

        /// <summary>
        /// Get all the transcoders that the user has configured in the TvServerPlugin of MPWebStream.
        ///
        /// This list can be empty (for example when you integrate this in your own app and don't provide the MPWebStream plugins. It is intended for users to have single
        /// place to configure the transcoders for multiple pieces of software that use this library.
        /// </summary>
        public static List<TranscoderProfile> GetConfiguredTranscoders() {
            Configuration config = new Configuration();
            return (List<TranscoderProfile>)config.Transcoders.Cast<TranscoderProfile>();
        }
    }
}
