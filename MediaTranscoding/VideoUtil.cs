using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MPWebStream.MediaTranscoding
{
    public static class VideoUtil
    {
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