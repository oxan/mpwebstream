using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MPWebStream.MediaTranscoding
{
    public static class VideoUtil
    {
        public class ResolutionInfo
        {
            // The original resolution of the stream
            public int OriginalWidth { get; set; }
            public int OriginalHeight { get; set; }

            // The Pixel Aspect Ratio (PAR). Is 1:1 for square pixels, most new HD material
            public int PixelAspectX { get; set; }
            public int PixelAspectY { get; set; }
            public string PixelAspectRatio {
                get {
                    return PixelAspectX + ":" + PixelAspectY;
                }
            }

            // The Display Aspect Ratio (DAR). Is 16:9 for HD material. 
            // This should be the same as the original size corrected with the PAR, the rendered image (see below)
            public int DisplayAspectX { get; set; }
            public int DisplayAspectY { get; set; }
            public string DisplayAspectRatio {
                get {
                    return DisplayAspectX + ":" + DisplayAspectY;
                }
            }

            // the size of the actually rendered image. 
            public int RenderWidth { get; set; }
            public int RenderHeight { get; set; }

            // check if the DAR is the same as the aspect ratio of the rendered image. if this returns false that's a bug.
            public bool Validate() {
                double DAR = DisplayAspectX / DisplayAspectY;
                double renderAspect = RenderWidth / RenderHeight;
                return Math.Abs(DAR - renderAspect) <= 0.01; // allow a slight deviation as you don't get exactly the DAR with small frame sizes
            }
        }

        public static ResolutionInfo GetResolutionInfo(string ffmpegPath, string video)
        {
            ProcessStartInfo start = new ProcessStartInfo(ffmpegPath, String.Format("-i \"{0}\"", video));
            start.UseShellExecute = false;
            start.RedirectStandardError = true;

            Process process = new Process();
            process.StartInfo = start;
            process.Start();

            ResolutionInfo info = null;
            while(true) {
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
    }
}