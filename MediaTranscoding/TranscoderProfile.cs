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

namespace MPWebStream.MediaTranscoding {
    public enum TransportMethod {
        Filename,
        NamedPipe,
        StandardIn,
        StandardOut
    }

    public class TranscoderProfile {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool UseTranscoding { get; set; }
        public string Transcoder { get; set; }
        public string Parameters { get; set; }
        public TransportMethod InputMethod { get; set; }
        public TransportMethod OutputMethod { get; set; }
    }
}
