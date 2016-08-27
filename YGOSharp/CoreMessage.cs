using System;
using System.IO;
using YGOSharp.OCGWrapper.Enums;

namespace YGOSharp
{
    public class CoreMessage
    {
        public GameMessage Message { get; private set; }
        public BinaryReader Reader { get; private set; }

        private readonly byte[] _raw;
        private readonly MemoryStream _stream;

        private readonly long _startPosition;
        private long _endPosition;
        private long _length;

        public CoreMessage(GameMessage msg, BinaryReader reader, byte[] raw)
        {
            Message = msg;
            Reader = reader;
            _raw = raw;
            _stream = (MemoryStream)reader.BaseStream;
            _startPosition = _stream.Position;
        }

        public byte[] CreateBuffer()
        {
            SetEndPosition();
            byte[] buffer = new byte[_length];
            Array.Copy(_raw, _startPosition, buffer, 0L, _length);
            return buffer;
        }

        private void SetEndPosition()
        {
            _endPosition = _stream.Position;
            _length = _endPosition - _startPosition;
        }
    }
}