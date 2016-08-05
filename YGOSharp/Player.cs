using System.IO;
using YGOSharp.Network;
using YGOSharp.Network.Enums;
using YGOSharp.Network.Utils;

namespace YGOSharp
{
    public class Player
    {
        public Game Game { get; private set; }
        public string Name { get; private set; }
        public bool IsAuthentified { get; private set; }
        public int Type { get; set; }
        public Deck Deck { get; private set; }
        public PlayerState State { get; set; }
        private YGOClient _client;

        public Player(Game game, YGOClient client)
        {
            Game = game;
            Type = (int)PlayerType.Undefined;
            State = PlayerState.None;
            _client = client;
        }

        public void Send(BinaryWriter packet)
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
            BinaryWriter packet = GamePacketFactory.Create(StocMessage.TypeChange);
            packet.Write((byte)(Type + (Game.HostPlayer.Equals(this) ? (int)PlayerType.Host : 0)));
            Send(packet);
        }

        public bool Equals(Player player)
        {
            return ReferenceEquals(this, player);
        }

        public void Parse(BinaryReader packet)
        {
            CtosMessage msg = (CtosMessage)packet.ReadByte();
            switch (msg)
            {
                case CtosMessage.PlayerInfo:
                    OnPlayerInfo(packet);
                    break;
                case CtosMessage.JoinGame:
                    OnJoinGame(packet);
                    break;
                case CtosMessage.CreateGame:
                    OnCreateGame(packet);
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

        private void OnPlayerInfo(BinaryReader packet)
        {
            if (Name != null)
                return;
            Name = packet.ReadUnicode(20);
        }

        private void OnCreateGame(BinaryReader packet)
        {
            Game.SetRules(packet);
            packet.ReadUnicode(20);//hostname
            packet.ReadUnicode(30); //password

            Game.AddPlayer(this);
            IsAuthentified = true;
        }

        private void OnJoinGame(BinaryReader packet)
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

        private void OnChat(BinaryReader packet)
        {
            string msg = packet.ReadUnicode(256);
            Game.Chat(this, msg);
        }

        private void OnKick(BinaryReader packet)
        {
            int pos = packet.ReadByte();
            Game.KickPlayer(this, pos);
        }

        private void OnHandResult(BinaryReader packet)
        {
            int res = packet.ReadByte();
            Game.HandResult(this, res);
        }

        private void OnTpResult(BinaryReader packet)
        {
            bool tp = packet.ReadByte() != 0;
            Game.TpResult(this, tp);
        }

        private void OnUpdateDeck(BinaryReader packet)
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
                    BinaryWriter error = GamePacketFactory.Create(StocMessage.ErrorMsg);
                    error.Write((byte)3);
                    error.Write(0);
                    Send(error);
                    return;
                }
                Deck = deck;
                Game.IsReady[Type] = true;
                Send(GamePacketFactory.Create(StocMessage.DuelStart));
                Game.MatchSide();
            }
        }

        private void OnResponse(BinaryReader packet)
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