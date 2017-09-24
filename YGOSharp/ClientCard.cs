using System.Collections.Generic;
using System.IO;
using YGOSharp.OCGWrapper.Enums;

namespace YGOSharp
{
    public class ClientCard
    {
        public int Code { get; private set; }

        public int Controler { get; private set; }
        public int Location { get; private set; }
        public int Sequence { get; private set; }
        public int Position { get; private set; }

        public IList<ClientCard> Overlay { get; private set; }

        public ClientCard()
        {
            Overlay = new List<ClientCard>();
        }

        public ClientCard(int code)
        {
            Overlay = new List<ClientCard>();
            Code = code;
        }

        public void Update(BinaryReader reader)
        {
            int flag = reader.ReadInt32();
            if ((flag & (int)Query.Code) != 0)
                Code = reader.ReadInt32();
            if ((flag & (int)Query.Position) != 0)
            {
                Controler = reader.ReadByte();
                Location = reader.ReadByte();
                Sequence = reader.ReadByte();
                Position = reader.ReadByte();
            }
            if ((flag & (int)Query.Alias) != 0)
                reader.ReadInt32();
            if ((flag & (int)Query.Type) != 0)
                reader.ReadInt32();
            if ((flag & (int)Query.Level) != 0)
                reader.ReadInt32();
            if ((flag & (int)Query.Rank) != 0)
                reader.ReadInt32();
            if ((flag & (int)Query.Attribute) != 0)
                reader.ReadInt32();
            if ((flag & (int)Query.Race) != 0)
                reader.ReadInt32();
            if ((flag & (int)Query.Attack) != 0)
                reader.ReadInt32();
            if ((flag & (int)Query.Defence) != 0)
                reader.ReadInt32();
            if ((flag & (int)Query.BaseAttack) != 0)
                reader.ReadInt32();
            if ((flag & (int)Query.BaseDefence) != 0)
                reader.ReadInt32();
            if ((flag & (int)Query.Reason) != 0)
                reader.ReadInt32();
            if ((flag & (int)Query.ReasonCard) != 0)
                reader.ReadInt32(); // Int8 * 4
            if ((flag & (int)Query.EquipCard) != 0)
                reader.ReadInt32(); // Int8 * 4
            if ((flag & (int)Query.TargetCard) != 0)
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; ++i)
                    reader.ReadInt32(); // Int8 * 4
            }
            if ((flag & (int)Query.OverlayCard) != 0)
            {
                int count = reader.ReadInt32();
                Overlay.Clear();
                for (int i = 0; i < count; ++i)
                {
                    ClientCard xyz = new ClientCard(reader.ReadInt32());
                    Overlay.Add(xyz);
                    xyz.Controler = Controler;
                    xyz.Location = Location | (int)CardLocation.Overlay;
                    xyz.Sequence = Sequence;
                    xyz.Position = 0;
                }
            }
            if ((flag & (int)Query.Counters) != 0)
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; ++i)
                    reader.ReadInt32(); // Int16 * 2
            }
            if ((flag & (int)Query.Owner) != 0)
                reader.ReadInt32();
            if ((flag & (int)Query.IsDisabled) != 0)
                reader.ReadInt32();
            if ((flag & (int)Query.IsPublic) != 0)
                reader.ReadInt32();
            if ((flag & (int)Query.LScale) != 0)
                reader.ReadInt32();
            if ((flag & (int)Query.RScale) != 0)
                reader.ReadInt32();
            if ((flag & (int)Query.Link) != 0)
            {
                reader.ReadInt32();
                reader.ReadInt32();
            }
        }
    }
}