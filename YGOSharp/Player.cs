using YGOSharp.Network;
using YGOSharp.Network.Enums;

namespace YGOSharp
{
    public class Player
    {
        public Game Game { get; private set; }
        public string Name { get; private set; }
        public bool IsAuthentified { get; private set; }
        public int Type { get; set; }
        public int TurnSkip { get; set; }
        public Deck Deck { get; private set; }
        public PlayerState State { get; set; }
        private CoreClient _client;
        private bool _isError;

        public Player(Game game, CoreClient client)
        {
            Game = game;
            Type = (int)PlayerType.Undefined;
            State = PlayerState.None;
            _client = client;
            TurnSkip = 0;
        }

        public void Send(GamePacketWriter packet)
        {
            _client.Send(packet);
        }

        public void Disconnect()
        {
            _client.Close();
        }

        public void OnDisconnected()
        {
            if (IsAuthentified)
                Game.RemovePlayer(this);
        }

        public void SendTypeChange()
        {
            GamePacketWriter packet = new GamePacketWriter(StocMessage.TypeChange);
            packet.Write((byte)(Type + (Game.HostPlayer.Equals(this) ? (int)PlayerType.Host : 0)));
            Send(packet);
        }

        public void SendErrorMessage(string message)
        {
            _isError = true;

            GamePacketWriter join = new GamePacketWriter(StocMessage.JoinGame);
            join.Write(0U);
            join.Write((byte)0);
            join.Write((byte)0);
            join.Write(false);
            join.Write(false);
            join.Write(false);
            // C++ padding: 5 bytes + 3 bytes = 8 bytes
            for (int i = 0; i < 3; i++)
                join.Write((byte)0);
            join.Write(8000);
            join.Write(5);
            join.Write(1);
            join.Write(0);
            Send(join);

            GamePacketWriter packet = new GamePacketWriter(StocMessage.TypeChange);
            packet.Write((byte)(0));
            Send(packet);

            GamePacketWriter enter = new GamePacketWriter(StocMessage.HsPlayerEnter);
            enter.Write("[Error occurred]:", 20);
            enter.Write((byte)0);
            Send(enter);

            enter = new GamePacketWriter(StocMessage.HsPlayerEnter);
            enter.Write(message, 20);
            enter.Write((byte)1);
            Send(enter);
        }

        public bool Equals(Player player)
        {
            return ReferenceEquals(this, player);
        }

        public void Parse(GamePacketReader packet)
        {
            if (_isError)
                return;

            CtosMessage msg = packet.ReadCtos();
            switch (msg)
            {
                case CtosMessage.PlayerInfo:
                    OnPlayerInfo(packet);
                    break;
                case CtosMessage.JoinGame:
                    OnJoinGame(packet);
                    break;
            }
            if (!IsAuthentified)
                return;
            switch (msg)
            {
                case CtosMessage.Chat:
                    OnChat(packet);
                    break;
                case CtosMessage.HsToDuelist:
                    Game.MoveToDuelist(this);
                    break;
                case CtosMessage.HsToObserver:
                    Game.MoveToObserver(this);
                    break;
                case CtosMessage.LeaveGame:
                    Game.RemovePlayer(this);
                    break;
                case CtosMessage.HsReady:
                    Game.SetReady(this, true);
                    break;
                case CtosMessage.HsNotReady:
                    Game.SetReady(this, false);
                    break;
                case CtosMessage.HsKick:
                    OnKick(packet);
                    break;
                case CtosMessage.HsStart:
                    Game.StartDuel(this);
                    break;
                case CtosMessage.HandResult:
                    OnHandResult(packet);
                    break;
                case CtosMessage.TpResult:
                    OnTpResult(packet);
                    break;
                case CtosMessage.UpdateDeck:
                    OnUpdateDeck(packet);
                    break;
                case CtosMessage.Response:
                    OnResponse(packet);
                    break;
                case CtosMessage.Surrender:
                    Game.Surrender(this, 0);
                    break;
            }
        }

        private void OnPlayerInfo(GamePacketReader packet)
        {
            if (Name != null)
                return;
            Name = packet.ReadUnicode(20);
        }

        private void OnJoinGame(GamePacketReader packet)
        {
            if (Name == null || Type != (int)PlayerType.Undefined)
                return;

            int version = packet.ReadInt16();
            if (version != Program.ClientVersion)
                return;

            packet.ReadInt32();//gameid
            packet.ReadInt16();

            Game.AddPlayer(this);
            IsAuthentified = true;
        }

        private void OnChat(GamePacketReader packet)
        {
            string msg = packet.ReadUnicode(256);
            Game.Chat(this, msg);
        }

        private void OnKick(GamePacketReader packet)
        {
            int pos = packet.ReadByte();
            Game.KickPlayer(this, pos);
        }

        private void OnHandResult(GamePacketReader packet)
        {
            int res = packet.ReadByte();
            Game.HandResult(this, res);
        }

        private void OnTpResult(GamePacketReader packet)
        {
            bool tp = packet.ReadByte() != 0;
            Game.TpResult(this, tp);
        }

        private void OnUpdateDeck(GamePacketReader packet)
        {
            if (Type == (int)PlayerType.Observer)
                return;
            Deck deck = new Deck();
            int main = packet.ReadInt32();
            int side = packet.ReadInt32();

            for (int i = 0; i < main; i++)
                deck.AddMain(packet.ReadInt32());
            for (int i = 0; i < side; i++)
                deck.AddSide(packet.ReadInt32());
            if (Game.State == GameState.Lobby)
            {
                Deck = deck;
                Game.IsReady[Type] = false;
            }
            else if (Game.State == GameState.Side)
            {
                if (Game.IsReady[Type])
                    return;
                if (!Deck.Check(deck))
                {
                    GamePacketWriter error = new GamePacketWriter(StocMessage.ErrorMsg);
                    error.Write((byte)3);
                    error.Write(0);
                    Send(error);
                    return;
                }
                Deck = deck;
                Game.IsReady[Type] = true;
                Send(new GamePacketWriter(StocMessage.DuelStart));
                Game.MatchSide();
            }
        }

        private void OnResponse(GamePacketReader packet)
        {
            if (Game.State != GameState.Duel)
                return;
            if (State != PlayerState.Response)
                return;
            byte[] resp = packet.ReadToEnd();
            if (resp.Length > 64)
                return;
            State = PlayerState.None;
            Game.SetResponse(resp);
        }
    }
}