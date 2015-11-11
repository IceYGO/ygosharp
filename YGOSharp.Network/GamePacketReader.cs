using System.IO;
using YGOSharp.Network.Enums;
using YGOSharp.Network.Utils;

namespace YGOSharp.Network
{
    public class GamePacketReader
    {
        public byte[] Content { get; private set; }

        private BinaryReader _reader;

        public GamePacketReader(byte[] content)
        {
            Content = content;
            _reader = new BinaryReader(new MemoryStream(Content));
        }

        public long Position
        {
            get
            {
                return _reader.BaseStream.Position;
            }
            set
            {
                _reader.BaseStream.Position = value;
            }
        }

        public long Length
        {
            get
            {
                return _reader.BaseStream.Length;
            }
        }

        public CtosMessage ReadCtos()
        {
            return (CtosMessage)_reader.ReadByte();
        }

        public StocMessage ReadStoc()
        {
            return (StocMessage)_reader.ReadByte();
        }

        public byte ReadByte()
        {
            return _reader.ReadByte();
        }

        public byte[] ReadBytes(int count)
        {
            return _reader.ReadBytes(count);
        }

        public byte[] ReadToEnd()
        {
            return _reader.ReadBytes((int)(Length - Position));
        }

        public sbyte ReadSByte()
        {
            return _reader.ReadSByte();
        }

        public bool ReadBool()
        {
            return _reader.ReadByte() != 0;
        }

        public short ReadInt16()
        {
            return _reader.ReadInt16();
        }

        public int ReadInt32()
        {
            return _reader.ReadInt32();
        }

        public string ReadUnicode(int len)
        {
            return _reader.ReadUnicode(len);
        }
    }
}
