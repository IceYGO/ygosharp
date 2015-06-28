using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;

namespace OCGWrapper
{
    internal static class CardsManager
    {
        private static IDictionary<int, Card> _cards;

        internal static void Init(string databaseFullPath)
        {
            _cards = new Dictionary<int, Card>();

            using (SqliteConnection connection = new SqliteConnection("Data Source=" + databaseFullPath))
            {
                connection.Open();

                using (IDbCommand command = new SqliteCommand("SELECT id, ot, alias, setcode, type, level, race, attribute, atk, def FROM datas", connection))
                {
                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            LoadCard(reader);
                        }
                    }
                }
            }
        }

        internal static Card GetCard(int id)
        {
            if (_cards.ContainsKey(id))
                return _cards[id];
            return null;
        }

        private static void LoadCard(IDataRecord reader)
        {
            int id = reader.GetInt32(0);
            int ot = reader.GetInt32(1);
            int levelinfo = reader.GetInt32(5);
            int level = levelinfo & 0xff;
            int lscale = (levelinfo >> 24) & 0xff;
            int rscale = (levelinfo >> 16) & 0xff;
            Card.CardData data = new Card.CardData
            {
                Code = id,
                Alias = reader.GetInt32(2),
                Setcode = reader.GetInt64(3),
                Type = reader.GetInt32(4),
                Level = level,
                LScale = lscale,
                RScale = rscale,
                Race = reader.GetInt32(6),
                Attribute = reader.GetInt32(7),
                Attack = reader.GetInt32(8),
                Defense = reader.GetInt32(9)
            };
            _cards.Add(id, new Card(data, ot));
        }
    }
}