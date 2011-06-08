using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
