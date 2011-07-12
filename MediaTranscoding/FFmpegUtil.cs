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

        public static void ParseOutputStream(Stream outputStream, out FFmpegEncodingInfo saveData) {
            ParseOutputStream(outputStream, out saveData, true, false);
        }

        public static void ParseOutputStream(Stream outputStream, out FFmpegEncodingInfo saveData, bool logMessages, bool logProgress) {
            StreamReader reader = new StreamReader(outputStream);
            saveData = new FFmpegEncodingInfo();

            bool aborted = false;
            string line;
            while ((line = reader.ReadLine()) != null) {
                try {
                    if (line.StartsWith("frame=")) {
                        // format of an output line (yes, we're doomed as soon as ffmpeg changes it output):
                        // frame=  923 fps=256 q=31.0 size=    2712kB time=00:05:22.56 bitrate= 601.8kbits/s
                        Match match = Regex.Match(line, @"frame=([ 0-9]*) fps=([ 0-9]*) q=[^ ]* L?size=([ 0-9]*)kB time=([0-9]{2}):([0-9]{2}):([0-9]{2})\.[0-9]{2} bitrate=([ .0-9]*)kbits/s", RegexOptions.IgnoreCase);
                        if (match.Success) {
                            lock (saveData) {
                                saveData.CurrentBitrate = Decimal.Parse(match.Groups[7].Value, System.Globalization.CultureInfo.InvariantCulture);
                                saveData.CurrentTime = Int32.Parse(match.Groups[4].Value) * 3600 + Int32.Parse(match.Groups[5].Value) * 60 + Int32.Parse(match.Groups[6].Value);
                                saveData.EncodedFrames = Int32.Parse(match.Groups[1].Value);
                                saveData.EncodingFPS = Int32.Parse(match.Groups[2].Value);
                                saveData.EncodedKb = Int32.Parse(match.Groups[3].Value);
                            }

                            if (!logProgress) {
                                //we don't want to log progress reports
                                continue;
                            }
                        }
                    } else if (line.StartsWith("video:")) {
                        //process the result line (e.g. video:5608kB audio:781kB global headers:0kB muxing overhead 13.235302%)
                        //line to see if it completed successfully
                        Match resultMatch = Regex.Match(line, @"video:([0-9]*)kB audio:([0-9]*)kB global headers:([0-9]*)kB muxing overhead[^%]*%", RegexOptions.IgnoreCase);
                        bool success = false;
                        if (resultMatch.Success) {
                            success = true;
                        }

                        lock (saveData) {
                            saveData.FinishedSuccessfully = success;
                        }
                    } else if (line.Contains("Application provided invalid, non monotonically increasing dts to muxer")) {
                        lock (saveData) {
                            saveData.EncodingErrors = saveData.EncodingErrors | FFMpegEncodingErrors.NonMonotonicallyIncreasingDts;
                        }
                    } else if (line.Contains("start time is not set in av_estimate_timings_from_pts")) {
                        lock (saveData) {
                            saveData.EncodingErrors = saveData.EncodingErrors | FFMpegEncodingErrors.StartTimeNotSetInEstimateTimingsFromPts;
                        }
                    } else if (line.Contains("use -vbsf h264_mp4toannexb")) {
                        lock (saveData) {
                            saveData.EncodingErrors = saveData.EncodingErrors | FFMpegEncodingErrors.UseVbsfH264Mp4ToAnnexb;
                        }
                    }

                    if (logMessages) {
                        Log.Trace(line);
                    }
                } catch (ThreadAbortException) {
                    // allow it to be used in a thread
                    aborted = true; ;
                    reader.Close();
                    break;
                } catch (Exception e) {
                    aborted = true;
                    Log.Error("Failure during parsing of ffmpeg output", e);
                }
            }

            if (aborted) {

            }
            reader.Close();
            return;
        }
    }
}