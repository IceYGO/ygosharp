using System.Collections.Generic;
using System.IO;

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
                int id = int.Parse(data[0]);
                int count = int.Parse(data[1]);
                current.Add(id, count);
            }
        }

        public static int GetIndex(uint hash)
        {
            for (int i = 0; i < Banlists.Count; i++)
                if (Banlists[i].Hash == hash)
                    return i;
            return 0;
        }
    }
}