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
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace MPWebStream.MediaTranscoding {
    public class FFmpegEncodingInfo {
        public decimal CurrentBitrate { get; set; }
        public int CurrentTime { get; set; }
        public int EncodedFrames { get; set; }
        public int EncodingFPS { get; set; }
        public int EncodedKb { get; set; }
        public bool FinishedSuccessfully { get; set; }
        public FFMpegEncodingErrors EncodingErrors { get; set; }
    }

    [Flags]
    public enum FFMpegEncodingErrors {
        NonMonotonicallyIncreasingDts = 1,

        /// <summary>
        /// start time is not set in av_estimate_timings_from_pts
        /// leads to transcoding failing with: [buffer @ 01A4FD80] Invalid pixel format string '-1' 
        /// </summary>
        StartTimeNotSetInEstimateTimingsFromPts = 2,

        /// <summary>
        /// [mpegts @ 036973C0] h264 bitstream malformated, no startcode found, use -vbsf h264_mp4toannexb
        /// leads to av_interleaved_write_frame(): Operation not permitted 
        /// </summary>
        UseVbsfH264Mp4ToAnnexb = 4
    }

    public static class FFmpegUtil {
        public static ResolutionInfo GetResolutionInfo(string ffmpegPath, string video) {
            ProcessStartInfo start = new ProcessStartInfo(ffmpegPath, String.Format("-i \"{0}\"", video));
            start.UseShellExecute = false;
            start.RedirectStandardError = true;

            Process process = new Process();
            process.StartInfo = start;
            process.Start();

            ResolutionInfo info = null;
            while (true) {
                string line = process.StandardError.ReadLine();
                if (line == null)
                    break;

                // skip other lines
                if (!line.TrimStart().StartsWith("Stream #") ||
                    !line.Contains("Video:"))
                    continue;

                // find data
                Match match = Regex.Match(line, @"Video: [^,]+, [a-z0-9]+, ([0-9]+)x([0-9]+)[,\[ ]+PAR ([0-9]+):([0-9]+) DAR ([0-9]+):([0-9]+)", RegexOptions.IgnoreCase);
                info = new ResolutionInfo();
                info.OriginalWidth = Int32.Parse(match.Groups[1].Value);
                info.OriginalHeight = Int32.Parse(match.Groups[2].Value);
                info.PixelAspectX = Int32.Parse(match.Groups[3].Value);
                info.PixelAspectY = Int32.Parse(match.Groups[4].Value);
                info.DisplayAspectX = Int32.Parse(match.Groups[5].Value);
                info.DisplayAspectY = Int32.Parse(match.Groups[6].Value);

                // always scale up instead of down
                if (Math.Min(info.PixelAspectX, info.PixelAspectY) == info.PixelAspectX) {
                    info.RenderWidth = info.OriginalWidth;
                    info.RenderHeight = (int)(info.OriginalHeight * ((float)info.PixelAspectY / info.PixelAspectX));
                } else {
                    info.RenderWidth = (int)(info.OriginalWidth * ((float)info.PixelAspectX / info.PixelAspectY));
                    info.RenderHeight = info.OriginalHeight;
                }
                break;
            }

            // end process
            process.StandardError.ReadToEnd();
            try {
                if (!process.HasExited)
                    process.Kill();
            } catch (Exception) {
                // if we can't kill, wait for it to voluntary die
                process.WaitForExit();
            }
            return info;
        }

        public static void ParseOutputStream(Stream outputStream, Reference<FFmpegEncodingInfo> saveData) {
            ParseOutputStream(outputStream, saveData, true, false);
        }

        public static void ParseOutputStream(Stream outputStream, Reference<FFmpegEncodingInfo> saveData, bool logMessages, bool logProgress) {
            StreamReader reader = new StreamReader(outputStream);

            bool aborted = false;
            string line;
            while ((line = reader.ReadLine()) != null) {
                try {
                    bool canBeErrorLine = true;

                    if (line.StartsWith("frame=")) {
                        // format of an output line (yes, we're doomed as soon as ffmpeg changes it output):
                        // frame=  923 fps=256 q=31.0 size=    2712kB time=00:05:22.56 bitrate= 601.8kbits/s
                        Match match = Regex.Match(line, @"frame=([ 0-9]*) fps=([ 0-9]*) q=[^ ]* L?size=([ 0-9]*)kB time=([0-9]{2}):([0-9]{2}):([0-9]{2})\.[0-9]{2} bitrate=([ .0-9]*)kbits/s", RegexOptions.IgnoreCase);
                        if (match.Success) {
                            canBeErrorLine = false;
                            lock (saveData) {
                                saveData.Value.CurrentBitrate = Decimal.Parse(match.Groups[7].Value, System.Globalization.CultureInfo.InvariantCulture);
                                saveData.Value.CurrentTime = Int32.Parse(match.Groups[4].Value) * 3600 + Int32.Parse(match.Groups[5].Value) * 60 + Int32.Parse(match.Groups[6].Value);
                                saveData.Value.EncodedFrames = Int32.Parse(match.Groups[1].Value);
                                saveData.Value.EncodingFPS = Int32.Parse(match.Groups[2].Value);
                                saveData.Value.EncodedKb = Int32.Parse(match.Groups[3].Value);
                            }

                            if(!logProgress) // we don't log output
                                continue;
                        }
                    }

                    if (line.StartsWith("video:")) {
                        // process the result line to see if it completed successfully (example):
                        // video:5608kB audio:781kB global headers:0kB muxing overhead 13.235302%
                        Match resultMatch = Regex.Match(line, @"video:([0-9]*)kB audio:([0-9]*)kB global headers:([0-9]*)kB muxing overhead[^%]*%", RegexOptions.IgnoreCase);
                        saveData.Value.FinishedSuccessfully = resultMatch.Success;
                        canBeErrorLine = false;
                    }

                    // parse and log errors if requested
                    if(!canBeErrorLine)
                        continue;
                    saveData.Value.EncodingErrors = ParseErrorLine(line, saveData.Value.EncodingErrors);
                    if (logMessages)
                        Log.Trace(line);
                } catch (ThreadAbortException) {
                    // allow it to be used in a thread
                    aborted = true;
                    reader.Close();
                    break;
                } catch (Exception e) {
                    aborted = true;
                    Log.Error("Failure during parsing of ffmpeg output", e);
                }
            }

            saveData.Value.FinishedSuccessfully = !aborted;
            reader.Close();
            return;
        }

        private static FFMpegEncodingErrors ParseErrorLine(string line, FFMpegEncodingErrors errors = 0) {
            if (line.Contains("Application provided invalid, non monotonically increasing dts to muxer")) {
                errors |= FFMpegEncodingErrors.NonMonotonicallyIncreasingDts;
            } else if (line.Contains("start time is not set in av_estimate_timings_from_pts")) {
                errors |= FFMpegEncodingErrors.StartTimeNotSetInEstimateTimingsFromPts;
            } else if (line.Contains("use -vbsf h264_mp4toannexb")) {
                errors |= FFMpegEncodingErrors.UseVbsfH264Mp4ToAnnexb;
            }

            return errors;
        }
    }
}
