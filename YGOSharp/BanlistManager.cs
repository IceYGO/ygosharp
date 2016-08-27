using System.Collections.Generic;
using System.IO;
using YGOSharp.OCGWrapper;
using YGOSharp.OCGWrapper.Enums;

namespace YGOSharp
{
    public static class BanlistManager
    {
        public static List<Banlist> Banlists { get; private set; }

        public static void Init(string fileName)
        {
            Banlists = new List<Banlist>();
            Banlist current = null;
            StreamReader reader = new StreamReader(fileName);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (line == null)
                    continue;
                if (line.StartsWith("#"))
                    continue;
                if (line.StartsWith("!"))
                {
                    current = new Banlist();
                    Banlists.Add(current);
                    continue;
                }
                if (!line.Contains(" "))
                    continue;
                if (current == null)
                    continue;
                string[] data = line.Split(' ');
                int id = 0;
                int.TryParse(data[0], out id);
                int count = int.Parse(data[1]);
                if (id == 0)
                {
                    if (data[0] == "TYPE_NORMAL")
                        BanType(current, CardType.Normal, count);
                    if (data[0] == "TYPE_XYZ")
                        BanType(current,CardType.Xyz, count);
                    if (data[0] == "TYPE_SYNCHRO")
                        BanType(current, CardType.Synchro, count);
                    if (data[0] == "TYPE_FUSION")
                        BanType(current, CardType.Fusion, count);
                    if (data[0] == "TYPE_PENDULUM")
                        BanType(current, CardType.Pendulum, count);
                    if (data[0] == "TYPE_SPELL")
                        BanType(current, CardType.Spell, count);
                    if (data[0] == "TYPE_TRAP")
                        BanType(current, CardType.Trap, count);
                    if (data[0] == "TYPE_RITUAL")
                        BanType(current, CardType.Ritual, count);
                    if (data[0] == "TYPE_EFFECT")
                        BanType(current, CardType.Effect, count);
                }
                else
                    current.Add(id, count);
            }
        }

        static void BanType(Banlist current,CardType type, int count)
        {
            Card[] cards = Api.GetCardList();
            foreach (Card card in cards)
            {
                if (card.HasType(type))
                    current.Add(card.Id, count);
            }
        }

        public static int GetIndex(uint hash)
        {
            for (int i = 0; i < Banlists.Count; i++)
                if (Banlists[i].Hash == hash)
                    return i;

            if (hash < Banlists.Count)
                return (int)hash;
            return 0;
        }
    }
}