﻿#region Copyright
/* 
 *  Copyright (C) 2008, 2009 StreamTv, http://code.google.com/p/mpstreamtv/
 *  Copyright (C) 2009, 2010 Gemx
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
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.IO.Pipes;

namespace MPWebStream.Streaming
{
  public class EncoderWrapper : Stream
  {
    // Transport Pipes
    private TransportStream encoderInput;
    private TransportStream encoderOutput;

    private Process applicationThread;
    private ProcessStartInfo applicationDetails;
    private EncoderConfig encCfg;

    // Media
    private string filename;
    private Stream mediaStream = null;

    public bool IsTranscodingDone {
      get {
          if (!encCfg.useTranscoding)
              return false;
          return applicationThread == null || applicationThread.HasExited;
      }
    }

    public EncoderWrapper(string filename, EncoderConfig encCfg)
    {
      this.filename = filename;
      this.mediaStream = null;
      this.encCfg = encCfg;
      if (!encCfg.useTranscoding)
        return;
      SetupPipes();
      Start();
    }

    public EncoderWrapper(Stream mediaStream, EncoderConfig encCfg)
    {
      this.filename = "";
      this.mediaStream = mediaStream;
      this.encCfg = encCfg;
      if (!encCfg.useTranscoding)
        return;
      SetupPipes();
      Start();
    }

    private void SetupPipes()
    {
      switch (encCfg.inputMethod)
      {
        case TransportMethod.Filename:
          encoderInput = null;
          break;
        case TransportMethod.NamedPipe:
          encoderInput = new NamedPipe();
          break;
        case TransportMethod.StandardIn:
          encoderInput = new BasicStream();
          break;
        default:
          throw new ArgumentException("Invalid option.");
      }

      switch (encCfg.outputMethod)
      {
        case TransportMethod.NamedPipe:
          encoderOutput = new NamedPipe();
          break;
        case TransportMethod.StandardOut:
          encoderOutput = new BasicStream();
          break;
        default:
          throw new ArgumentException("Invalid option.");
      }
    }

    protected void Start()
    {
      StartPipe();
    }

    private void StartPipe()
    {
      // Start the transcoder.
      if (mediaStream != null)
      {
        encoderInput.Start(false);
        StartProcess(encoderInput.Url, encoderOutput.Url);
        encoderInput.CopyStream(mediaStream);
      }
      else
        StartProcess(filename, encoderOutput.Url);

      encoderOutput.Start(false);

      // Wait for the output encoder to connect.
      int tries = 10000;

      do
      {
        if (encoderOutput.IsReady)
          break;

        System.Threading.Thread.Sleep(1);
      } while (--tries != 0);
    }

    public override bool CanRead
    {
      get
      {
        return encoderOutput.CanRead;
      }
    }
    public override bool CanSeek
    {
      get
      {
        return encoderOutput.CanSeek;
      }
    }
    public override bool CanWrite
    {
      get { throw new NotSupportedException(); }
    }
    public override void Flush()
    {
      throw new NotSupportedException();
    }
    public override long Length
    {
      get
      {
        return encoderOutput.Length;
      }
    }
    public override long Position
    {
      get
      {
        return encoderOutput.Position;
      }
      set
      {
        encoderOutput.Position = value;
      }
    }
    public override int Read(byte[] buffer, int offset, int count)
    {
      return encoderOutput.Read(buffer, offset, count);
    }
    public override long Seek(long offset, SeekOrigin origin)
    {
      return encoderOutput.Seek(offset, origin);
    }
    public override void SetLength(long value)
    {
      encoderOutput.SetLength(value);
    }
    public override void Write(byte[] buffer, int offset, int count)
    {
      throw new NotSupportedException();
    }

    public void StartProcess(String input, String output)
    {
      if (applicationThread != null)
        applicationThread.Kill();
      string args = encCfg.args.Replace("{0}",input);
      args=args.Replace("{1}",output);
      System.Console.Write("Starting process:\n");
      System.Console.Write(encCfg.fileName);
      System.Console.Write(args);
      System.Console.Write("\n\n");
      
      applicationDetails = new ProcessStartInfo(encCfg.fileName, args);
      applicationDetails.UseShellExecute = false;
      applicationDetails.RedirectStandardInput = (encCfg.inputMethod == TransportMethod.StandardIn);
      applicationDetails.RedirectStandardOutput = (encCfg.outputMethod == TransportMethod.StandardOut);

      applicationThread = new Process();
      applicationThread.StartInfo = applicationDetails;
      if (applicationThread.Start())
      {
        if (encCfg.inputMethod == TransportMethod.StandardIn)
          encoderInput.UnderlyingStreamObject = applicationThread.StandardInput.BaseStream;
        if (encCfg.outputMethod == TransportMethod.StandardOut)
          encoderOutput.UnderlyingStreamObject = applicationThread.StandardOutput.BaseStream;
      }
    }

    public void StopProcess()
    {
      if (applicationThread != null)
        applicationThread.Kill();
    }
  }
}
