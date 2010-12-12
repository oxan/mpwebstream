using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Pipes;
using System.IO;

namespace MPWebStream.Streaming
{
    public class NamedPipe : TransportStream
    {
        private String _pipeName;
        private Boolean isReady;

        private PipeStream pipe;

        public override String Url
        {
            get { return String.Format("\\\\.\\pipe\\{0}", _pipeName); }
        }

        public override bool IsReady
        {
            get { return isReady; }
        }

        public override bool CanRead
        {
            get { return pipe.CanRead; }
        }

        public override bool CanSeek
        {
            get { return pipe.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return pipe.CanWrite; }
        }

        public override long Length
        {
            get { return pipe.Length; }
        }

        public override long Position
        {
            get
            {
                return pipe.Position;
            }
            set
            {
                pipe.Position = value;
            }
        }

        public NamedPipe()
        {
            _pipeName = Guid.NewGuid().ToString();
        }

        public NamedPipe(String pipeName)
        {
            _pipeName = pipeName;
        }

        public override void Flush()
        {
            pipe.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
          int read=0;
          try
          {
            read = pipe.Read(buffer, offset, count);
          }
          catch (Exception)
          {
          }
          return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return pipe.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            pipe.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
          try
          {
            pipe.Write(buffer, offset, count);
          }
          catch (Exception)
          {
            //throw new Exception("Can't write to pipe");
          }
        }

        public override void Start(Boolean isClient)
        {
            if (isClient)
            {
                NamedPipeClientStream client = new NamedPipeClientStream (".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                client.Connect(10000); // 10 second timeout.

                pipe = client;
                isReady = true;
            }
            else
            {
                NamedPipeServerStream server = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                server.BeginWaitForConnection(new AsyncCallback(WaitForConnection), server);
            }
        }
        public override Stream UnderlyingStreamObject
        {
          get
          {
            return pipe;
          }
          set
          {
            
          }
        }

        private void WaitForConnection(IAsyncResult ar)
        {
            NamedPipeServerStream server = ar.AsyncState as NamedPipeServerStream;
            server.EndWaitForConnection(ar);
            
            pipe = server;
            isReady = true;
        }
    }
}
