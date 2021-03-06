﻿#region Copyright
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

namespace MPWebStream.MediaTranscoding
{
    public class ResolutionInfo
    {
        // The original resolution of the stream
        public int OriginalWidth { get; set; }
        public int OriginalHeight { get; set; }

        // The Pixel Aspect Ratio (PAR). Is 1:1 for square pixels, most new HD material
        public int PixelAspectX { get; set; }
        public int PixelAspectY { get; set; }
        public string PixelAspectRatio
        {
            get
            {
                return PixelAspectX + ":" + PixelAspectY;
            }
        }

        // The Display Aspect Ratio (DAR). Is 16:9 for HD material. 
        // This should be the same as the original size corrected with the PAR, the rendered image (see below)
        public int DisplayAspectX { get; set; }
        public int DisplayAspectY { get; set; }
        public string DisplayAspectRatio
        {
            get
            {
                return DisplayAspectX + ":" + DisplayAspectY;
            }
        }

        // the size of the actually rendered image. 
        public int RenderWidth { get; set; }
        public int RenderHeight { get; set; }

        // check if the DAR is the same as the aspect ratio of the rendered image. if this returns false that's a bug.
        public bool Validate()
        {
            double DAR = DisplayAspectX / DisplayAspectY;
            double renderAspect = RenderWidth / RenderHeight;
            return Math.Abs(DAR - renderAspect) <= 0.01; // allow a slight deviation as you don't get exactly the DAR with small frame sizes
        }
    }
}
