using System.IO;
using YGOSharp.Network.Enums;
using YGOSharp.Network.Utils;

namespace YGOSharp.Network
{
    public class GamePacketWriter
    {
        private BinaryWriter _writer;
        private MemoryStream _stream;

        public GamePacketWriter(CtosMessage message)
        {
            InitializeStream();
            _writer.Write((byte)message);
        }

        public GamePacketWriter(StocMessage message)
        {
            InitializeStream();
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

        public void Write(byte[] array, int index, int count)
        {
            _writer.Write(array, index, count);
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

        private void InitializeStream()
        {
            _stream = new MemoryStream();
            _writer = new BinaryWriter(_stream);
        }
    }
}
