namespace YGOSharp
{
    public class CoreConfig
    {
        public bool Ranked { get; private set; }
        public bool Private { get; private set; }
        public int Rule { get; private set; }
        public int Mode { get; private set; }
        public int LfList { get; private set; }
        public bool EnablePriority { get; private set; }
        public bool NoCheckDeck { get; private set; }
        public bool NoShuffleDeck { get; private set; }
        public int StartLp { get; private set; }
        public int StartHand { get; private set; }
        public int DrawCount { get; private set; }
        public int GameTimer { get; private set; }
        public int Port { get; private set; }

        public bool Load(string name, string portText)
        {
            if (name.Length < 3)
                return false;

            bool result = name.Contains(",")
                ? ParseAdvanced(name)
                : ParseSimple(name);

            if (!result)
                return false;

            if (Ranked&& LfList != 0 && LfList != 1)
                return false;

            if (Ranked && LfList == 1)
                Rule = 0; // TCG Only
            if (Ranked && LfList == 0)
                Rule = 1; // OCG Only

            if (LfList < 0 || LfList > 3)
                return false;
            if (Mode < 0 || Mode > 2)
                return false;
            if (StartHand < 0 || DrawCount <= 0)
                return false;

            int port;
            if (!int.TryParse(portText, out port))
                return false;
            if (port < 0 || port > 65535)
                return false;
            Port = port;

            return true;
        }

        private bool ParseAdvanced(string name)
        {
            const string hex = "0123456789ABCDEF";

            string[] data = name.Split(',');
            if (data.Length != 3 || data[0].Length != 6)
                return false;

            ParseCommon(name);

            if (Ranked)
                return false;

            int cheats = name[3] - '0';
            EnablePriority = false;
            NoCheckDeck = (cheats & 1) != 0;
            NoShuffleDeck = (cheats & 2) != 0;

            int startLp;
            if (!int.TryParse(data[1], out startLp))
                return false;
            StartLp = startLp;

            StartHand = hex.IndexOf(char.ToUpper(name[4]));
            DrawCount = hex.IndexOf(char.ToUpper(name[5]));

            GameTimer = 240;

            return true;
        }

        private bool ParseSimple(string name)
        {
            ParseCommon(name);

            if (name.Length > 3 && name[3] == 'P')
                Private = true;

            EnablePriority = false;
            NoCheckDeck = false;
            NoShuffleDeck = false;

            StartLp = Mode == 2 ? 16000 : 8000;
            StartHand = 5;
            DrawCount = 1;

            GameTimer = 240;

            return true;
        }

        private void ParseCommon(string name)
        {
            Ranked = name[0] == '1';
            LfList = name[1] - '0';
            Mode = name[2] - '0';

            Rule = 2; // TCG/OCG
        }
    }
}