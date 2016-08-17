using System;
using System.IO;
using YGOSharp.Network.Utils;
using YGOSharp.OCGWrapper.Enums;
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
        private BinaryWriter Writer;

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

        public void Write(BinaryWriter packet)
        {
            byte[] data = ((MemoryStream)packet.BaseStream).ToArray();
            byte[] replayData = new byte[data.Length - 1];
            Array.Copy(data, 1, replayData, 0, replayData.Length);

            Write((short)replayData.Length);
            Write(replayData);
        }

        public void Write(int packet)
        {
            Writer.Write(packet);
        }

        public void Write(short packet)
        {
            Writer.Write(packet);
        }

        public void Write(byte packet)
        {
            Writer.Write(packet);
        }

        public void Write(byte[] packet)
        {
            Writer.Write(packet);
        }

        public void WriteUnicode(string packet, int len)
        {
            Writer.WriteUnicode(packet, len);
           
        }

        public void Write(byte[] packet, int index, int len)
        {
            Writer.Write(packet, index, len);
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
            if (Config.GetBool("YRP2", false))
            {
                Write((short)1);
                Write((byte)GameMessage.Win);
            }

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