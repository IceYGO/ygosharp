using System.Collections.Generic;
using YGOSharp.OCGWrapper;
using YGOSharp.OCGWrapper.Enums;

namespace YGOSharp
{
    public class Deck
    {
        public IList<int> Main { get; private set; }
        public IList<int> Extra { get; private set; }
        public IList<int> Side { get; private set; }

        public Deck()
        {
            Main = new List<int>();
            Extra = new List<int>();
            Side = new List<int>();
        }

        public void AddMain(int cardId)
        {
            Card card = Card.Get(cardId);
            if (card == null)
                return;
            if ((card.Type & (int)CardType.Token) != 0)
                return;
            if (card.IsExtraCard())
            {
                if (Extra.Count < 15)
                    Extra.Add(cardId);
            }
            else
            {
                if (Main.Count < 60)
                    Main.Add(cardId);
            }
        }

        public void AddSide(int cardId)
        {
            Card card = Card.Get(cardId);
            if (card == null)
                return;
            if ((card.Type & (int)CardType.Token) != 0)
                return;
            if (Side.Count < 15)
                Side.Add(cardId);
        }

        public int Check(Banlist ban, bool ocg, bool tcg)
        {
            if (Main.Count < Config.GetInt("MainDeckMinSize", 40) ||
                Main.Count > Config.GetInt("MainDeckMaxSize", 60) ||
                Extra.Count > Config.GetInt("ExtraDeckMaxSize", 15) ||
                Side.Count > Config.GetInt("SideDeckMaxSize", 15))
                return 1;

            IDictionary<int, int> cards = new Dictionary<int, int>();

            IList<int>[] stacks = { Main, Extra, Side };
            foreach (IList<int> stack in stacks)
            {
                foreach (int id in stack)
                {
                    Card card = Card.Get(id);
                    AddToCards(cards, card);
                    if (!ocg && card.Ot == 1 || !tcg && card.Ot == 2)
                        return id;
                }
            }

            if (ban == null)
                return 0;

            foreach (var pair in cards)
            {
                int max = ban.GetQuantity(pair.Key);
                if (pair.Value > max)
                    return pair.Key;
            }

            return 0;
        }

        public bool Check(Deck deck)
        {
            if (deck.Main.Count != Main.Count || deck.Extra.Count != Extra.Count)
                return false;

            IDictionary<int, int> cards = new Dictionary<int, int>();
            IDictionary<int, int> ncards = new Dictionary<int, int>();
            IList<int>[] stacks = { Main, Extra, Side };
            foreach (IList<int> stack in stacks)
            {
                foreach (int id in stack)
                {
                    if (!cards.ContainsKey(id))
                        cards.Add(id, 1);
                    else
                        cards[id]++;
                }
            }
            stacks = new[] { deck.Main, deck.Extra, deck.Side };
            foreach (var stack in stacks)
            {
                foreach (int id in stack)
                {
                    if (!ncards.ContainsKey(id))
                        ncards.Add(id, 1);
                    else
                        ncards[id]++;
                }
            }
            foreach (var pair in cards)
            {
                if (!ncards.ContainsKey(pair.Key))
                    return false;
                if (ncards[pair.Key] != pair.Value)
                    return false;
            }
            return true;
        }

        private static void AddToCards(IDictionary<int, int> cards, Card card)
        {
            int id = card.Id;
            if (card.Alias != 0)
                id = card.Alias;
            if (cards.ContainsKey(id))
                cards[id]++;
            else
                cards.Add(id, 1);
        }
    }
}