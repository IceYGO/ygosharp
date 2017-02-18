using System.IO;
using YGOSharp.SevenZip.Compress.LZMA;

namespace YGOSharp
{
    public class Replay
    {
        public struct ReplayHeader
        {
            public uint Id;
            public uint Version;
            public uint Flag;
            public uint Seed;
            public uint DataSize;
            public uint Hash;
            public byte[] Props;
        }

        public const uint FlagCompressed = 0x1;
        public const uint FlagTag = 0x2;

        public const int MaxReplaySize = 0x20000;

        public bool Disabled { get; private set; }
        public ReplayHeader Header;
        public BinaryWriter Writer { get; private set; }

        private MemoryStream _stream;
        private byte[] _data;

        public Replay(uint seed, bool tag)
        {
            Header.Id = 0x31707279;
            Header.Version = Program.ClientVersion;
            Header.Flag = tag ? FlagTag : 0;
            Header.Seed = seed;

            _stream = new MemoryStream();
            Writer = new BinaryWriter(_stream);
        }

        public void Check()
        {
            if (_stream.Position >= MaxReplaySize)
            {
                Writer.Close();
                _stream.Dispose();
                Disabled = true;
            }
        }

        public void End()
        {
            if (Disabled)
                return;
            Disabled = true;

            byte[] raw = _stream.ToArray();

            Header.DataSize = (uint)raw.Length;
            Header.Flag |= FlagCompressed;
            Header.Props = new byte[8];

            Encoder lzma = new Encoder();
            using (MemoryStream props = new MemoryStream(Header.Props))
                lzma.WriteCoderProperties(props);

            MemoryStream compressed = new MemoryStream();
            lzma.Code(new MemoryStream(raw), compressed, raw.LongLength, -1, null);

            raw = compressed.ToArray();

            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);

            writer.Write(Header.Id);
            writer.Write(Header.Version);
            writer.Write(Header.Flag);
            writer.Write(Header.Seed);
            writer.Write(Header.DataSize);
            writer.Write(Header.Hash);
            writer.Write(Header.Props);

            writer.Write(raw);

            _data = ms.ToArray();
        }

        public byte[] GetContent()
        {
            return _data;
        }
    }
}