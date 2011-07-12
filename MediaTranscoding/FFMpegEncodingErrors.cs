using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPWebStream.MediaTranscoding
{
    public enum FFMpegEncodingErrors
    {
        /// <summary>
        /// 
        /// </summary>
        NonMonotonicallyIncreasingDts = 1,
        /// <summary>
        /// start time is not set in av_estimate_timings_from_pts
        /// 
        /// leads to transcoding failing with: [buffer @ 01A4FD80] Invalid pixel format string '-1' 
        /// </summary>
        StartTimeNotSetInEstimateTimingsFromPts = 2,

        /// <summary>
        /// [mpegts @ 036973C0] h264 bitstream malformated, no startcode found, use -vbsf h264_mp4toannexb
        /// 
        /// leads to av_interleaved_write_frame(): Operation not permitted 
        /// </summary>
        UseVbsfH264Mp4ToAnnexb = 4
         
    }
}
