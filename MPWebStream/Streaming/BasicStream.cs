using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Pipes;
using System.IO;

namespace MPWebStream.Streaming
{
  public class BasicStream : TransportStream
  {
    private Boolean isReady;

    private Stream _stream;

    public override String Url
    {
      get { return ""; }
    }

    public override bool IsReady
    {
      get { return isReady; }
    }

    public override bool CanRead
    {
      get { return _stream.CanRead; }
    }

    public override bool CanSeek
    {
      get { return _stream.CanSeek; }
    }

    public override bool CanWrite
    {
      get { return _stream.CanWrite; }
    }

    public override long Length
    {
      get { return _stream.Length; }
    }

    public override long Position
    {
      get
      {
        return _stream.Position;
      }
      set
      {
        _stream.Position = value;
      }
    }

    public BasicStream()
    {
      _stream = null;
    }

    public override void Flush()
    {
      if (_stream != null)
        _stream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      int read = 0;
      try
      {
        read = _stream.Read(buffer, offset, count);
      }
      catch (Exception)
      {
      }
      return read;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      return _stream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
      _stream.SetLength(value); 
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      try
      {
        _stream.Write(buffer, offset, count);
      }
      catch (Exception)
      {
        //throw new Exception("Can't write to pipe");
      }
    }

    public override void Start(Boolean isClient)
    {
      isReady = (_stream != null);
    }
    public override Stream UnderlyingStreamObject
    {
      get
      {
        return _stream;
      }
      set
      {
        _stream = value;
      }
    }
  }
}
