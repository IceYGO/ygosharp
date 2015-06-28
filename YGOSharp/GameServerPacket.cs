using System.IO;
using OCGWrapper.Enums;
using YGOSharp.Enums;

namespace YGOSharp
{
    public class GameServerPacket
    {
        private BinaryWriter _writer;
        private MemoryStream _stream;

        public GameServerPacket(StocMessage message)
        {
            _stream = new MemoryStream();
            _writer = new BinaryWriter(_stream);
            _writer.Write((byte)message);
        }

        public GameServerPacket(GameMessage message)
        {
            _stream = new MemoryStream();
            _writer = new BinaryWriter(_stream);
            _writer.Write((byte)(StocMessage.GameMsg));
            _writer.Write((byte)message);
        }

        public byte[] GetContent()
        {
            return _stream.ToArray();
        }

        public void Write(byte[] array)
        {
            _writer.Write(array);
        }

        public void Write(bool value)
        {
            _writer.Write((byte)(value ? 1 : 0));
        }

        public void Write(sbyte value)
        {
            _writer.Write(value);
        }

        public void Write(byte value)
        {
            _writer.Write(value);
        }

        public void Write(short value)
        {
            _writer.Write(value);
        }

        public void Write(int value)
        {
            _writer.Write(value);
        }

        public void Write(uint value)
        {
            _writer.Write(value);
        }

        public void Write(string text, int len)
        {
            _writer.WriteUnicode(text, len);
        }

        public long GetPosition()
        {
            return _stream.Position;
        }

        public void SetPosition(long pos)
        {
            _stream.Position = pos;
        }
    }
}