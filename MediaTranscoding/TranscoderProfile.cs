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
using System.Xml;

namespace MPWebStream.MediaTranscoding {
    public enum TransportMethod {
        Filename,       // always a full path to a file
        NamedPipe,      // always a named pipe (all data passes through MPWebStream)
        Path,           // dynamically dispatch between Filename and NamedPipe (recommended)
        StandardIn,     // only input: is written to standard input of transcoder
        StandardOut,    // only output: is read from standard output of transcoder
        External        // don't touch input or output (i.e. hardcoded in transcoder config)
    }

    public class TranscoderProfile {
        public string Name { get; set; }
        public bool UseTranscoding { get; set; }
        public string Transcoder { get; set; }
        public string Parameters { get; set; }
        public TransportMethod InputMethod { get; set; }
        public TransportMethod OutputMethod { get; set; }
        public string MIME { get; set; }

        public static TranscoderProfile CreateFromXmlNode(XmlNode node) {
            TranscoderProfile transcoder = new TranscoderProfile();
            foreach (XmlNode child in node.ChildNodes) {
                if (child.Name == "name") transcoder.Name = child.InnerText;
                if (child.Name == "useTranscoding") transcoder.UseTranscoding = child.InnerText == "true";
                if (child.Name == "inputMethod") transcoder.InputMethod = (TransportMethod)Enum.Parse(typeof(TransportMethod), child.InnerText, true);
                if (child.Name == "outputMethod") transcoder.OutputMethod = (TransportMethod)Enum.Parse(typeof(TransportMethod), child.InnerText, true);
                if (child.Name == "transcoder") transcoder.Transcoder = child.InnerText;
                if (child.Name == "parameters") transcoder.Parameters = child.InnerText;
                if (child.Name == "mime") transcoder.MIME = child.InnerText;
            }
            return transcoder;
        }
    }
}
