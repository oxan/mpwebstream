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

namespace MPWebStream.MediaTranscoding {
    public class Resolution {
        public int Width { get; set; }
        public int Height { get; set; }

        public Resolution(int width, int height) {
            Width = width;
            Height = height;
        }

        public Resolution CalculateResize(Resolution maxOutputSize, int framesizeMultipleOff = 1) {
            return CalculateResize(Width / Height, maxOutputSize, framesizeMultipleOff);
        }

        public Resolution CalculateResize(decimal destinationAspectRatio, Resolution maxOutput, int framesizeMultipleOff = 1) {
            // get the aspect ratio for the height / width calculation, defaulting to 16:9
            decimal displayAspect = destinationAspectRatio == 0 ? 16 / 9 : destinationAspectRatio;

            // calculate new width
            int width = maxOutput.Width;
            int height = (int)(width * (1 / displayAspect));
            if (height > maxOutput.Height) {
                height = maxOutput.Height;
                width = (int)(height * displayAspect);
            }

            // round
            int newWidth = ((int)Math.Round(width * 1.0 / framesizeMultipleOff)) * framesizeMultipleOff;
            int newHeight = ((int)Math.Round(height * 1.0 / framesizeMultipleOff)) * framesizeMultipleOff;

            return new Resolution(newWidth, newHeight);
        }

        public override string ToString() {
            return Width.ToString() + "x" + Height.ToString();
        }

        public static Resolution Calculate(decimal destinationAspectRatio, Resolution maxOutput, int framesizeMultipleOff = 1) {
            Resolution res = new Resolution(0, 0);
            return res.CalculateResize(destinationAspectRatio, maxOutput, framesizeMultipleOff);
        }
    }
}
