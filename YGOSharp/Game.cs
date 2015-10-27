using System;
using System.Collections.Generic;
using System.IO;
using OCGWrapper;
using OCGWrapper.Enums;
using YGOSharp.Enums;

namespace YGOSharp
{
    public class Game
    {
        public CoreConfig Config { get; private set; }
        public Banlist Banlist { get; private set; }
        public bool IsMatch { get; private set; }
        public bool IsTag { get; private set; }
        public bool IsTpSelect { get; private set; }

        public GameState State { get; private set; }
        public DateTime SideTimer { get; private set; }
        public DateTime TpTimer { get; private set; }
        public DateTime RpsTimer { get; private set; }
        public int TurnCount { get; set; }
        public int CurrentPlayer { get; set; }
        public int[] LifePoints { get; set; }

        public Player[] Players { get; private set; }
        public Player[] CurPlayers { get; private set; }
        public bool[] IsReady { get; private set; }
        public List<Player> Observers { get; private set; }
        public Player HostPlayer { get; private set; }

        public Replay Replay { get; private set; }

        private CoreServer _server;
        private Duel _duel;
        private GameAnalyser _analyser;
        private int[] _handResult;
        private int _startplayer;
        private int _lastresponse;

        private int[] _timelimit;
        private DateTime? _time;

        private int[] _matchResult;
        private int _duelCount;
        private bool _matchKill;
        private bool _swapped;

        public Game(CoreServer server, CoreConfig config)
        {
            Config = config;
            State = GameState.Lobby;
            IsMatch = config.Mode == 1;
            IsTag = config.Mode == 2;
            CurrentPlayer = 0;
            LifePoints = new int[2];
            Players = new Player[IsTag ? 4 : 2];
            CurPlayers = new Player[2];
            IsReady = new bool[IsTag ? 4 : 2];
            _handResult = new int[2];
            _timelimit = new int[2];
            _matchResult = new int[3];
            Observers = new List<Player>();
            if (config.LfList >= 0 && config.LfList < BanlistManager.Banlists.Count)
                Banlist = BanlistManager.Banlists[config.LfList];
            _server = server;
            _analyser = new GameAnalyser(this);
            ReloadGameConfig();
        }

        public void ReloadGameConfig()
        {
            IsMatch = Config.Mode == 1;
            IsTag = Config.Mode == 2;
            Players = new Player[IsTag ? 4 : 2];
            IsReady = new bool[IsTag ? 4 : 2];
            if (Config.LfList >= 0 && Config.LfList < BanlistManager.Banlists.Count)
                Banlist = BanlistManager.Banlists[Config.LfList];
            LifePoints[0] = Config.StartLp;
            LifePoints[1] = Config.StartLp;
        }

        public void SendToAll(GameServerPacket packet)
        {
            SendToPlayers(packet);
            SendToObservers(packet);
        }

        public void SendToAllBut(GameServerPacket packet, Player except)
        {
            foreach (Player player in Players)
                if (player != null && !player.Equals(except))
                    player.Send(packet);
            foreach (Player player in Observers)
                if (!player.Equals(except))
                    player.Send(packet);
        }

        public void SendToAllBut(GameServerPacket packet, int except)
        {
            if(except < CurPlayers.Length)
                SendToAllBut(packet, CurPlayers[except]);
            else
                SendToAll(packet);
        }

        public void SendToPlayers(GameServerPacket packet)
        {
            foreach (Player player in Players)
                if (player != null)
                    player.Send(packet);
        }

        public void SendToObservers(GameServerPacket packet)
        {
            foreach (Player player in Observers)
                player.Send(packet);
        }

        public void SendToTeam(GameServerPacket packet, int team)
        {
            if (!IsTag)
                Players[team].Send(packet);
            else if (team == 0)
            {
                Players[0].Send(packet);
                Players[1].Send(packet);
            }
            else
            {
                Players[2].Send(packet);
                Players[3].Send(packet);
            }
        }

        public void AddPlayer(Player player)
        {
            if (State != GameState.Lobby)
            {
                player.Type = (int)PlayerType.Observer;
                if (State == GameState.End)
                    return;
                SendJoinGame(player);
                player.SendTypeChange();
                player.Send(new GameServerPacket(StocMessage.DuelStart));
                Observers.Add(player);
                if (State == GameState.Duel)
                    InitNewSpectator(player);
                return;
            }

            if (HostPlayer == null)
                HostPlayer = player;

            int pos = GetAvailablePlayerPos();
            if (pos != -1)
            {
                GameServerPacket enter = new GameServerPacket(StocMessage.HsPlayerEnter);
                enter.Write(player.Name, 20);
                enter.Write((byte)pos);
                //padding
                enter.Write((byte)0);
                SendToAll(enter);

                Players[pos] = player;
                IsReady[pos] = false;
                player.Type = pos;
            }
            else
            {
                GameServerPacket watch = new GameServerPacket(StocMessage.HsWatchChange);
                watch.Write((short)(Observers.Count + 1));
                SendToAll(watch);

                player.Type = (int)PlayerType.Observer;
                Observers.Add(player);
            }

            SendJoinGame(player);
            player.SendTypeChange();

            for (int i = 0; i < Players.Length; i++)
            {
                if (Players[i] != null)
                {
                    GameServerPacket enter = new GameServerPacket(StocMessage.HsPlayerEnter);
                    enter.Write(Players[i].Name, 20);
                    enter.Write((byte)i);
                    //padding
                    enter.Write((byte)0);
                    player.Send(enter);

                    if (IsReady[i])
                    {
                        GameServerPacket change = new GameServerPacket(StocMessage.HsPlayerChange);
                        change.Write((byte)((i << 4) + (int)PlayerChange.Ready));
                        player.Send(change);
                    }
                }
            }

            if (Observers.Count > 0)
            {
                GameServerPacket nwatch = new GameServerPacket(StocMessage.HsWatchChange);
                nwatch.Write((short)Observers.Count);
                player.Send(nwatch);
            }
        }

        public void RemovePlayer(Player player)
        {
            if (player.Equals(HostPlayer) && State == GameState.Lobby)
                _server.Stop();
            else if (player.Type == (int)PlayerType.Observer)
            {
                Observers.Remove(player);
                if (State == GameState.Lobby)
                {
                    GameServerPacket nwatch = new GameServerPacket(StocMessage.HsWatchChange);
                    nwatch.Write((short) Observers.Count);
                    SendToAll(nwatch);
                }
                player.Disconnect();
            }
            else if (State == GameState.Lobby)
            {
                Players[player.Type] = null;
                IsReady[player.Type] = false;
                GameServerPacket change = new GameServerPacket(StocMessage.HsPlayerChange);
                change.Write((byte)((player.Type << 4) + (int) PlayerChange.Leave));
                SendToAll(change);
                player.Disconnect();
            }
            else
                Surrender(player, 4, true);
        }

        public void MoveToDuelist(Player player)
        {
            if (State != GameState.Lobby)
                return;
            int pos = GetAvailablePlayerPos();
            if (pos == -1)
                return;
            if (player.Type != (int)PlayerType.Observer)
            {
                if (!IsTag || IsReady[player.Type])
                    return;

                pos = (player.Type + 1) % 4;
                while (Players[pos] != null)
                    pos = (pos + 1) % 4;

                GameServerPacket change = new GameServerPacket(StocMessage.HsPlayerChange);
                change.Write((byte)((player.Type << 4) + pos));
                SendToAll(change);

                Players[player.Type] = null;
                Players[pos] = player;
                player.Type = pos;
                player.SendTypeChange();
            }
            else
            {
                Observers.Remove(player);
                Players[pos] = player;
                player.Type = pos;

                GameServerPacket enter = new GameServerPacket(StocMessage.HsPlayerEnter);
                enter.Write(player.Name, 20);
                enter.Write((byte)pos);
                //padding
                enter.Write((byte)0);
                SendToAll(enter);

                GameServerPacket nwatch = new GameServerPacket(StocMessage.HsWatchChange);
                nwatch.Write((short)Observers.Count);
                SendToAll(nwatch);

                player.SendTypeChange();
            }
        }

        public void MoveToObserver(Player player)
        {
            if (State != GameState.Lobby)
                return;
            if (player.Type == (int)PlayerType.Observer)
                return;
            if (IsReady[player.Type])
                return;
            Players[player.Type] = null;
            IsReady[player.Type] = false;
            Observers.Add(player);

            GameServerPacket change = new GameServerPacket(StocMessage.HsPlayerChange);
            change.Write((byte)((player.Type << 4) + (int)PlayerChange.Observe));
            SendToAll(change);

            player.Type = (int)PlayerType.Observer;
            player.SendTypeChange();
        }

        public void Chat(Player player, string msg)
        {
            GameServerPacket packet = new GameServerPacket(StocMessage.Chat);
            packet.Write((short)player.Type);
            if (player.Type == (int)PlayerType.Observer)
            {
                msg = "[" + player.Name + "]: " + msg;
                CustomMessage(player, msg);
            }
            else
            {
                packet.Write(msg, msg.Length + 1);
                SendToAllBut(packet, player);
            }
        }

        public void CustomMessage(Player player, string msg)
        {
            string finalmsg = msg;
            GameServerPacket packet = new GameServerPacket(StocMessage.Chat);
            packet.Write((short)PlayerType.Yellow);
            packet.Write(finalmsg, finalmsg.Length + 1);
            SendToAllBut(packet, player);
        }

        public void SetReady(Player player, bool ready)
        {
            if (State != GameState.Lobby)
                return;
            if (player.Type == (int)PlayerType.Observer)
                return;
            if (IsReady[player.Type] == ready)
                return;

            if (ready)
            {
                bool ocg = Config.Rule == 0 || Config.Rule == 2;
                bool tcg = Config.Rule == 1 || Config.Rule == 2;
                int result = 1;
                if (player.Deck != null)
                {
                    result = Config.NoCheckDeck ? 0 : player.Deck.Check(Banlist, ocg, tcg);
                }
                if (result != 0)
                {
                    GameServerPacket rechange = new GameServerPacket(StocMessage.HsPlayerChange);
                    rechange.Write((byte)((player.Type << 4) + (int)(PlayerChange.NotReady)));
                    player.Send(rechange);
                    GameServerPacket error = new GameServerPacket(StocMessage.ErrorMsg);
                    error.Write((byte)2); // ErrorMsg.DeckError
                    // C++ padding: 1 byte + 3 bytes = 4 bytes
                    for (int i = 0; i < 3; i++)
                        error.Write((byte)0);
                    error.Write(result);
                    player.Send(error);
                    return;
                }
            }

            IsReady[player.Type] = ready;

            GameServerPacket change = new GameServerPacket(StocMessage.HsPlayerChange);
            change.Write((byte)((player.Type << 4) + (int)(ready ? PlayerChange.Ready : PlayerChange.NotReady)));
            SendToAll(change);
        }

        public void KickPlayer(Player player, int pos)
        {
            if (State != GameState.Lobby)
                return;
            if (pos >= Players.Length || !player.Equals(HostPlayer) || player.Equals(Players[pos]) || Players[pos] == null)
                return;
            RemovePlayer(Players[pos]);
        }

        public void StartDuel(Player player)
        {
            if (State != GameState.Lobby)
                return;
            if (!player.Equals(HostPlayer))
                return;
            for (int i = 0; i < Players.Length; i++)
            {
                if (!IsReady[i])
                    return;
                if (Players[i] == null)
                    return;
            }

            State = GameState.Hand;
            SendToAll(new GameServerPacket(StocMessage.DuelStart));

            SendHand();
        }

        public void HandResult(Player player, int result)
        {
            if (State != GameState.Hand)
                return;
            if (player.Type == (int)PlayerType.Observer)
                return;
            if (result < 1 || result > 3)
                return;
            if (IsTag && player.Type != 0 && player.Type != 2)
                return;
            int type = player.Type;
            if (IsTag && player.Type == 2)
                type = 1;
            if (_handResult[type] != 0)
                return;
            _handResult[type] = result;
            if (_handResult[0] != 0 && _handResult[1] != 0)
            {
                GameServerPacket packet = new GameServerPacket(StocMessage.HandResult);
                packet.Write((byte)_handResult[0]);
                packet.Write((byte)_handResult[1]);
                SendToTeam(packet, 0);
                SendToObservers(packet);

                packet = new GameServerPacket(StocMessage.HandResult);
                packet.Write((byte)_handResult[1]);
                packet.Write((byte)_handResult[0]);
                SendToTeam(packet, 1);

                if (_handResult[0] == _handResult[1])
                {
                    _handResult[0] = 0;
                    _handResult[1] = 0;
                    SendHand();
                    return;
                }
                if ((_handResult[0] == 1 && _handResult[1] == 2) ||
                    (_handResult[0] == 2 && _handResult[1] == 3) ||
                    (_handResult[0] == 3 && _handResult[1] == 1))
                    _startplayer = IsTag ? 2 : 1;
                else
                    _startplayer = 0;
                State = GameState.Starting;
                Players[_startplayer].Send(new GameServerPacket(StocMessage.SelectTp));
                TpTimer = DateTime.UtcNow;
            }
        }

        public void TpResult(Player player, bool result)
        {
            if (State != GameState.Starting)
                return;
            if (player.Type != _startplayer)
                return;

            _swapped = false;
            if (result && player.Type == (IsTag ? 2 : 1) || !result && player.Type == 0)
            {
                _swapped = true;
                if (IsTag)
                {
                    Player temp = Players[0];
                    Players[0] = Players[2];
                    Players[2] = temp;

                    temp = Players[1];
                    Players[1] = Players[3];
                    Players[3] = temp;

                    Players[0].Type = 0;
                    Players[1].Type = 1;
                    Players[2].Type = 2;
                    Players[3].Type = 3;
                }
                else
                {
                    Player temp = Players[0];
                    Players[0] = Players[1];
                    Players[1] = temp;
                    Players[0].Type = 0;
                    Players[1].Type = 1;
                }
            }
            CurPlayers[0] = Players[0];
            CurPlayers[1] = Players[IsTag ? 3 : 1];

            State = GameState.Duel;
            int seed = Environment.TickCount;
            _duel = Duel.Create((uint)seed);
            Random rand = new Random(seed);

            _duel.SetAnalyzer(_analyser.Analyse);
            _duel.SetErrorHandler(HandleError);

            _duel.InitPlayers(Config.StartLp, Config.StartHand, Config.DrawCount);

            int opt = 0;
            if (Config.EnablePriority)
                opt += 0x08;
            if (Config.NoShuffleDeck)
                opt += 0x10;
            if (IsTag)
                opt += 0x20;

            Replay = new Replay((uint)seed, IsTag);
            Replay.Writer.WriteUnicode(Players[0].Name, 20);
            Replay.Writer.WriteUnicode(Players[1].Name, 20);
            if (IsTag)
            {
                Replay.Writer.WriteUnicode(Players[2].Name, 20);
                Replay.Writer.WriteUnicode(Players[3].Name, 20);
            }
            Replay.Writer.Write(Config.StartLp);
            Replay.Writer.Write(Config.StartHand);
            Replay.Writer.Write(Config.DrawCount);
            Replay.Writer.Write(opt);

            for (int i = 0; i < Players.Length; i++)
            {
                Player dplayer = Players[i == 2 ? 3 : (i == 3 ? 2 : i)];
                int pid = i;
                if (IsTag)
                    pid = i >= 2 ? 1 : 0;
                if (!Config.NoShuffleDeck)
                {
                    List<int> cards = ShuffleCards(rand, dplayer.Deck.Main);
                    Replay.Writer.Write(cards.Count);
                    foreach (int id in cards)
                    {
                        if (IsTag && (i == 1 || i == 3))
                            _duel.AddTagCard(id, pid, CardLocation.Deck);
                        else
                            _duel.AddCard(id, pid, CardLocation.Deck);
                        Replay.Writer.Write(id);
                    }
                }
                else
                {
                    Replay.Writer.Write(dplayer.Deck.Main.Count);
                    for (int j = dplayer.Deck.Main.Count - 1; j >= 0; j--)
                    {
                        int id = dplayer.Deck.Main[j];
                        if (IsTag && (i == 1 || i == 3))
                            _duel.AddTagCard(id, pid, CardLocation.Deck);
                        else
                            _duel.AddCard(id, pid, CardLocation.Deck);
                        Replay.Writer.Write(id);
                    }
                }
                Replay.Writer.Write(dplayer.Deck.Extra.Count);
                foreach (int id in dplayer.Deck.Extra)
                {
                    if (IsTag && (i == 1 || i == 3))
                        _duel.AddTagCard(id, pid, CardLocation.Extra);
                    else
                        _duel.AddCard(id, pid, CardLocation.Extra);
                    Replay.Writer.Write(id);
                }
            }

            GameServerPacket packet = new GameServerPacket(GameMessage.Start);
            packet.Write((byte)0);
            packet.Write(Config.StartLp);
            packet.Write(Config.StartLp);
            packet.Write((short)_duel.QueryFieldCount(0, CardLocation.Deck));
            packet.Write((short)_duel.QueryFieldCount(0, CardLocation.Extra));
            packet.Write((short)_duel.QueryFieldCount(1, CardLocation.Deck));
            packet.Write((short)_duel.QueryFieldCount(1, CardLocation.Extra));
            SendToTeam(packet, 0);

            packet.SetPosition(2);
            packet.Write((byte)1);
            SendToTeam(packet, 1);

            packet.SetPosition(2);
            if (_swapped)
                packet.Write((byte)0x11);
            else
                packet.Write((byte)0x10);
            SendToObservers(packet);

            RefreshExtra(0);
            RefreshExtra(1);

            _duel.Start(opt);

            TurnCount = 0;
            LifePoints[0] = Config.StartLp;
            LifePoints[1] = Config.StartLp;
            TimeReset();

            Process();
        }

        public void Surrender(Player player, int reason, bool force = false)
        {
            if(!force)
                if (State != GameState.Duel)
                    return;
            if (player.Type == (int)PlayerType.Observer)
                return;
            GameServerPacket win = new GameServerPacket(GameMessage.Win);
            int team = player.Type;
            if (IsTag)
                team = player.Type >= 2 ? 1 : 0;
            else if (State == GameState.Hand)
                team = 1 - team;
            win.Write((byte)(1 - team));
            win.Write((byte)reason);
            SendToAll(win);

            MatchSaveResult(1 - team);

            EndDuel(reason == 4);
        }

        public void RefreshAll()
        {
            RefreshMonsters(0);
            RefreshMonsters(1);
            RefreshSpells(0);
            RefreshSpells(1);
            RefreshHand(0);
            RefreshHand(1);
        }

        public void RefreshAllObserver(Player observer)
        {
            RefreshMonsters(0, useCache: false, observer: observer);
            RefreshMonsters(1, useCache: false, observer: observer);
            RefreshSpells(0, useCache: false, observer: observer);
            RefreshSpells(1, useCache: false, observer: observer);
            RefreshHand(0, useCache: false, observer: observer);
            RefreshHand(1, useCache: false, observer: observer);
        }

        public void RefreshMonsters(int player, int flag = 0x81fff, bool useCache = true, Player observer = null)
        {
            byte[] result = _duel.QueryFieldCard(player, CardLocation.MonsterZone, flag, useCache);
            GameServerPacket update = new GameServerPacket(GameMessage.UpdateData);
            update.Write((byte)player);
            update.Write((byte)CardLocation.MonsterZone);
            update.Write(result);
            if (observer == null)
                SendToTeam(update, player);

            update = new GameServerPacket(GameMessage.UpdateData);
            update.Write((byte)player);
            update.Write((byte)CardLocation.MonsterZone);

            MemoryStream ms = new MemoryStream(result);
            BinaryReader reader = new BinaryReader(ms);
            BinaryWriter writer = new BinaryWriter(ms);
            for (int i = 0; i < 5; i++)
            {
                int len = reader.ReadInt32();
                if (len == 4)
                    continue;
                long pos = ms.Position;
                byte[] raw = reader.ReadBytes(len - 4);
                if ((raw[11] & (int)CardPosition.FaceDown) != 0)
                {
                    ms.Position = pos;
                    writer.Write(new byte[len - 4]);
                }
            }
            update.Write(result);

            if (observer == null)
            {
                SendToTeam(update, 1 - player);
                SendToObservers(update);
            }
            else
            {
                observer.Send(update);
            }
        }

        public void RefreshSpells(int player, int flag = 0x681fff, bool useCache = true, Player observer = null)
        {
            byte[] result = _duel.QueryFieldCard(player, CardLocation.SpellZone, flag, useCache);
            GameServerPacket update = new GameServerPacket(GameMessage.UpdateData);
            update.Write((byte)player);
            update.Write((byte)CardLocation.SpellZone);
            update.Write(result);
            if (observer == null)
                SendToTeam(update, player);

            update = new GameServerPacket(GameMessage.UpdateData);
            update.Write((byte)player);
            update.Write((byte)CardLocation.SpellZone);

            MemoryStream ms = new MemoryStream(result);
            BinaryReader reader = new BinaryReader(ms);
            BinaryWriter writer = new BinaryWriter(ms);
            for (int i = 0; i < 8; i++)
            {
                int len = reader.ReadInt32();
                if (len == 4)
                    continue;
                long pos = ms.Position;
                byte[] raw = reader.ReadBytes(len - 4);
                if ((raw[11] & (int)CardPosition.FaceDown) != 0)
                {
                    ms.Position = pos;
                    writer.Write(new byte[len - 4]);
                }
            }
            update.Write(result);

            if (observer == null)
            {
                SendToTeam(update, 1 - player);
                SendToObservers(update);
            }
            else
            {
                observer.Send(update);
            }
        }

        public void RefreshHand(int player, int flag = 0x181fff, bool useCache = true, Player observer = null)
        {
            byte[] result = _duel.QueryFieldCard(player, CardLocation.Hand, flag | 0x100000, useCache);
            GameServerPacket update = new GameServerPacket(GameMessage.UpdateData);
            update.Write((byte)player);
            update.Write((byte)CardLocation.Hand);
            update.Write(result);
            if (observer == null)
                CurPlayers[player].Send(update);

            update = new GameServerPacket(GameMessage.UpdateData);
            update.Write((byte)player);
            update.Write((byte)CardLocation.Hand);

            MemoryStream ms = new MemoryStream(result);
            BinaryReader reader = new BinaryReader(ms);
            BinaryWriter writer = new BinaryWriter(ms);
            while (ms.Position < ms.Length)
            {
                int len = reader.ReadInt32();
                if (len == 4)
                    continue;
                long pos = ms.Position;
                byte[] raw = reader.ReadBytes(len - 4);
                if (raw[len - 8] == 0)
                {
                    ms.Position = pos;
                    writer.Write(new byte[len - 4]);
                }
            }
            update.Write(result);

            if (observer == null)
                SendToAllBut(update, player);
            else
                observer.Send(update);
        }

        public void RefreshGrave(int player, int flag = 0x81fff, bool useCache = true, Player observer = null)
        {
            byte[] result = _duel.QueryFieldCard(player, CardLocation.Grave, flag, useCache);
            GameServerPacket update = new GameServerPacket(GameMessage.UpdateData);
            update.Write((byte)player);
            update.Write((byte)CardLocation.Grave);
            update.Write(result);
            if (observer == null)
                SendToAll(update);
            else
                observer.Send(update);
        }

        public void RefreshExtra(int player, int flag = 0x81fff, bool useCache = true)
        {
            byte[] result = _duel.QueryFieldCard(player, CardLocation.Extra, flag, useCache);
            GameServerPacket update = new GameServerPacket(GameMessage.UpdateData);
            update.Write((byte)player);
            update.Write((byte)CardLocation.Extra);
            update.Write(result);
            CurPlayers[player].Send(update);
        }

        public void RefreshSingle(int player, int location, int sequence, int flag = 0x781fff)
        {
            byte[] result = _duel.QueryCard(player, location, sequence, flag);

            if (location == (int)CardLocation.Removed && (result[15] & (int)CardPosition.FaceDown) != 0)
                return;

            GameServerPacket update = new GameServerPacket(GameMessage.UpdateCard);
            update.Write((byte)player);
            update.Write((byte)location);
            update.Write((byte)sequence);
            update.Write(result);
            CurPlayers[player].Send(update);

            if (IsTag)
            {
                if ((location & (int)CardLocation.Onfield) != 0)
                {
                    SendToTeam(update, player);
                    if ((result[15] & (int)CardPosition.FaceUp) != 0)
                        SendToTeam(update, 1 - player);
                }
                else
                {
                    CurPlayers[player].Send(update);
                    if ((location & 0x90) != 0)
                        SendToAllBut(update, player);
                }
            }
            else
            {
                if ((location & 0x90) != 0 || ((location & 0x2c) != 0 && (result[15] & (int)CardPosition.FaceUp) != 0))
                    SendToAllBut(update, player);
            }
        }

        public int WaitForResponse()
        {
            WaitForResponse(_lastresponse);
            return _lastresponse;
        }

        public void WaitForResponse(int player)
        {
            _lastresponse = player;
            CurPlayers[player].State = PlayerState.Response;
            SendToAllBut(new GameServerPacket(GameMessage.Waiting), player);
            TimeStart();
            GameServerPacket packet = new GameServerPacket(StocMessage.TimeLimit);
            packet.Write((byte)player);
            packet.Write((byte)0); // C++ padding
            packet.Write((short)_timelimit[player]);
            SendToPlayers(packet);
        }

        public void SetResponse(int resp)
        {
            if (!Replay.Disabled)
            {
                Replay.Writer.Write((byte)4);
                Replay.Writer.Write(BitConverter.GetBytes(resp));
                Replay.Check();
            }

            TimeStop();
            _duel.SetResponse(resp);
        }

        public void SetResponse(byte[] resp)
        {
            if (!Replay.Disabled)
            {
                Replay.Writer.Write((byte)resp.Length);
                Replay.Writer.Write(resp);
                Replay.Check();
            }

            TimeStop();
            _duel.SetResponse(resp);
            Process();
        }

        public void EndDuel(bool force)
        {
            if (State == GameState.Duel)
            {
                if (!Replay.Disabled)
                {
                    Replay.End();
                    byte[] replayData = Replay.GetContent();
                    GameServerPacket packet = new GameServerPacket(StocMessage.Replay);
                    packet.Write(replayData);
                    SendToAll(packet);
                }

                State = GameState.End;
                _duel.End();
            }

            if (_swapped)
            {
                _swapped = false;
                if (IsTag)
                {
                    Player temp = Players[0];
                    Players[0] = Players[2];
                    Players[2] = temp;

                    temp = Players[1];
                    Players[1] = Players[3];
                    Players[3] = temp;

                    Players[0].Type = 0;
                    Players[1].Type = 1;
                    Players[2].Type = 2;
                    Players[3].Type = 3;
                }
                else
                {
                    Player temp = Players[0];
                    Players[0] = Players[1];
                    Players[1] = temp;
                    Players[0].Type = 0;
                    Players[1].Type = 1;
                }
            }

            if (IsMatch && !force && !MatchIsEnd())
            {
                IsReady[0] = false;
                IsReady[1] = false;
                State = GameState.Side;
                SideTimer = DateTime.UtcNow;
                SendToPlayers(new GameServerPacket(StocMessage.ChangeSide));
                SendToObservers(new GameServerPacket(StocMessage.WaitingSide));
            }
            else
            {
                if (State == GameState.Hand)
                {
                    State = GameState.End;
                    End();
                }
                else
                {
                    ApplyRanking();
                    End();
                }
            }
        }

        public void End()
        {
            SendToAll(new GameServerPacket(StocMessage.DuelEnd));
            _server.StopDelayed();
        }

        public void TimeReset()
        {
            _timelimit[0] = Config.GameTimer;
            _timelimit[1] = Config.GameTimer;
        }

        public void TimeStart()
        {
            _time = DateTime.UtcNow;
        }

        public void TimeStop()
        {
            if (_time != null)
            {
                TimeSpan elapsed = DateTime.UtcNow - _time.Value;
                _timelimit[_lastresponse] -= (int)elapsed.TotalSeconds;
                if (_timelimit[_lastresponse] < 0)
                    _timelimit[_lastresponse] = 0;
                _time = null;
            }
        }

        public void TimeTick()
        {
            if (State == GameState.Duel)
            {
                if (_time != null)
                {
                    TimeSpan elapsed = DateTime.UtcNow - _time.Value;
                    if ((int)elapsed.TotalSeconds > _timelimit[_lastresponse])
                    {
                        Surrender(Players[_lastresponse], 3);
                    }
                }
            }

            if (State == GameState.Side)
            {
                TimeSpan elapsed = DateTime.UtcNow - SideTimer;

                if (elapsed.TotalMilliseconds >= 120000)
                {
                    if (!IsReady[0] && !IsReady[1])
                    {
                        State = GameState.End;
                        End();
                        return;
                    }

                    Surrender(!IsReady[0] ? Players[0] : Players[1], 3, true);
                    State = GameState.End;
                    End();
                }
            }

            if (State == GameState.Starting)
            {
                if (IsTpSelect)
                {
                    TimeSpan elapsed = DateTime.UtcNow - TpTimer;

                    if (elapsed.TotalMilliseconds >= 30000)
                    {
                        Surrender(Players[_startplayer], 3, true);
                        State = GameState.End;
                        End();
                    }

                }
            }
            if (State == GameState.Hand)
            {
                TimeSpan elapsed = DateTime.UtcNow - RpsTimer;

                if ((int)elapsed.TotalMilliseconds >= 60000)
                {
                    if (_handResult[0] != 0)
                        Surrender(Players[1], 3, true);
                    else if (_handResult[1] != 0)
                        Surrender(Players[0], 3, true);
                    else
                    {
                        State = GameState.End;
                        End();
                    }
                }
            }
        }

        public void MatchSaveResult(int player)
        {
            if (player < 2 && _swapped)
                player = 1 - player;
            if (player < 2)
                _startplayer = 1 - player;
            else
                _startplayer = 1 - _startplayer;
            _matchResult[_duelCount++] = player;
        }

        public void MatchKill()
        {
            _matchKill = true;
        }

        public bool MatchIsEnd()
        {
            if (_matchKill)
                return true;
            int[] wins = new int[3];
            for (int i = 0; i < _duelCount; i++)
                wins[_matchResult[i]]++;
            return wins[0] == 2 || wins[1] == 2 || wins[0] + wins[1] + wins[2] == 3;
        }

        public void MatchSide()
        {
            if (IsReady[0] && IsReady[1])
            {
                State = GameState.Starting;
                IsTpSelect = true;
                TpTimer = DateTime.UtcNow;
                TimeReset();
                Players[_startplayer].Send(new GameServerPacket(StocMessage.SelectTp));
            }
        }

        private int GetAvailablePlayerPos()
        {
            for (int i = 0; i < Players.Length; i++)
            {
                if (Players[i] == null)
                    return i;
            }
            return -1;
        }

        private void SendHand()
        {
            RpsTimer = DateTime.UtcNow;
            GameServerPacket hand = new GameServerPacket(StocMessage.SelectHand);
            if (IsTag)
            {
                Players[0].Send(hand);
                Players[2].Send(hand);
            }
            else
                SendToPlayers(hand);
        }

        private void Process()
        {
            int result = _duel.Process();
            switch (result)
            {
                case -1:
                    _server.Stop();
                    break;
                case 2: // Game finished
                    EndDuel(false);
                    break;
            }
        }

        private void SendJoinGame(Player player)
        {
            GameServerPacket join = new GameServerPacket(StocMessage.JoinGame);
            join.Write(Banlist == null ? 0U : Banlist.Hash);
            join.Write((byte)Config.Rule);
            join.Write((byte)Config.Mode);
            join.Write(Config.EnablePriority);
            join.Write(Config.NoCheckDeck);
            join.Write(Config.NoShuffleDeck);
            // C++ padding: 5 bytes + 3 bytes = 8 bytes
            for (int i = 0; i < 3; i++)
                join.Write((byte)0);
            join.Write(Config.StartLp);
            join.Write((byte)Config.StartHand);
            join.Write((byte)Config.DrawCount);
            join.Write((short)Config.GameTimer);
            player.Send(join);

            if (State != GameState.Lobby)
                SendDuelingPlayers(player);
        }

        private void SendDuelingPlayers(Player player)
        {
            for (int i = 0; i < Players.Length; i++)
            {
                GameServerPacket enter = new GameServerPacket(StocMessage.HsPlayerEnter);
                int id = i;
                if (_swapped)
                {
                    if (IsTag)
                    {
                        if (i == 0 || id == 1)
                            id = i + 2;
                        else
                            id = i - 2;
                    }
                    else
                        id = 1 - i;
                }
                enter.Write(Players[id].Name, 20);
                enter.Write((byte)i);
                //padding
                enter.Write((byte)0);
                player.Send(enter);
            }
        }

        private void InitNewSpectator(Player player)
        {
            GameServerPacket packet = new GameServerPacket(GameMessage.Start);
            packet.Write((byte)(_swapped ? 0x11 : 0x10));
            packet.Write(LifePoints[0]);
            packet.Write(LifePoints[1]);
            packet.Write((short)0); // deck
            packet.Write((short)0); // extra
            packet.Write((short)0); // deck
            packet.Write((short)0);  // extra
            player.Send(packet);
            
            GameServerPacket turn = new GameServerPacket(GameMessage.NewTurn);
            turn.Write((byte)0);
            player.Send(turn);
            if (CurrentPlayer == 1)
            {
                turn = new GameServerPacket(GameMessage.NewTurn);
                turn.Write((byte)0);
                player.Send(turn);
            }

            GameServerPacket reload = new GameServerPacket(GameMessage.ReloadField);
            byte[] fieldInfo = _duel.QueryFieldInfo();
            reload.Write(fieldInfo, 1, fieldInfo.Length - 1);
            player.Send(reload);

            RefreshAllObserver(player);
        }

        private void InitSpectatorLocation(Player player, CardLocation loc)
        {
            for (int index = 0; index < 2; index++)
            {
                int flag = loc == CardLocation.MonsterZone ? 0x91fff : 0x81fff;
                byte[] result = _duel.QueryFieldCard(index, loc, flag, false);

                MemoryStream ms = new MemoryStream(result);
                BinaryReader reader = new BinaryReader(ms);
                BinaryWriter writer = new BinaryWriter(ms);
                while (ms.Position < ms.Length)
                {
                    int len = reader.ReadInt32();
                    if (len == 4)
                        continue;
                    long pos = ms.Position;
                    reader.ReadBytes(len - 4);
                    long endPos = ms.Position;

                    ms.Position = pos;
                    ClientCard card = new ClientCard();
                    card.Update(reader);
                    ms.Position = endPos;

                    bool facedown = ((card.Position & (int)CardPosition.FaceDown) != 0);

                    GameServerPacket move = new GameServerPacket(GameMessage.Move);
                    move.Write(facedown ? 0 : card.Code);
                    move.Write(0);
                    move.Write((byte)card.Controler);
                    move.Write((byte)card.Location);
                    move.Write((byte)card.Sequence);
                    move.Write((byte)card.Position);
                    move.Write(0);
                    player.Send(move);

                    foreach (ClientCard material in card.Overlay)
                    {
                        GameServerPacket xyzcreate = new GameServerPacket(GameMessage.Move);
                        xyzcreate.Write(material.Code);
                        xyzcreate.Write(0);
                        xyzcreate.Write((byte)index);
                        xyzcreate.Write((byte)CardLocation.Grave);
                        xyzcreate.Write((byte)0);
                        xyzcreate.Write((byte)0);
                        xyzcreate.Write(0);
                        player.Send(xyzcreate);

                        GameServerPacket xyzmove = new GameServerPacket(GameMessage.Move);
                        xyzmove.Write(material.Code);
                        xyzmove.Write((byte)index);
                        xyzmove.Write((byte)CardLocation.Grave);
                        xyzmove.Write((byte)0);
                        xyzmove.Write((byte)0);
                        xyzmove.Write((byte)material.Controler);
                        xyzmove.Write((byte)material.Location);
                        xyzmove.Write((byte)material.Sequence);
                        xyzmove.Write((byte)material.Position);
                        xyzmove.Write(0);
                        player.Send(xyzmove);
                    }

                    if (facedown)
                    {
                        ms.Position = pos;
                        writer.Write(new byte[len - 4]);
                    }
                }

                if (loc == CardLocation.MonsterZone)
                {
                    result = _duel.QueryFieldCard(index, loc, 0x81fff, false);
                    ms = new MemoryStream(result);
                    reader = new BinaryReader(ms);
                    writer = new BinaryWriter(ms);
                    while (ms.Position < ms.Length)
                    {
                        int len = reader.ReadInt32();
                        if (len == 4)
                            continue;
                        long pos = ms.Position;
                        byte[] raw = reader.ReadBytes(len - 4);

                        bool facedown = ((raw[11] & (int)CardPosition.FaceDown) != 0);
                        if (facedown)
                        {
                            ms.Position = pos;
                            writer.Write(new byte[len - 4]);
                        }
                    }
                }

                GameServerPacket update = new GameServerPacket(GameMessage.UpdateData);
                update.Write((byte)index);
                update.Write((byte)loc);
                update.Write(result);
                player.Send(update);
            }
        }

        private void HandleError(string error)
        {
            GameServerPacket packet = new GameServerPacket(StocMessage.Chat);
            packet.Write((short)PlayerType.Observer);
            packet.Write(error, error.Length + 1);
            SendToAll(packet);

            File.WriteAllText("lua_" + DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt", error);
        }

        private static List<int> ShuffleCards(Random rand, IEnumerable<int> cards)
        {
            List<int> shuffled = new List<int>(cards);
            for (int i = shuffled.Count-1 ; i > 0; --i)
            {
                int pos = rand.Next(i+1);
                int tmp = shuffled[i];
                shuffled[i] = shuffled[pos];
                shuffled[pos] = tmp;
            }
            return shuffled;
        }

        private void ApplyRanking()
        {
            int winner = -1;
            if (_duelCount > 0)
            {
                if (!_matchKill && _duelCount != 1)
                {
                    int[] wins = new int[3];
                    for (int i = 0; i < _duelCount; i++)
                        wins[_matchResult[i]]++;
                    if (wins[0] > wins[1])
                        winner = 0;
                    else if (wins[1] > wins[0])
                        winner = 1;
                    else
                        winner = 2;
                }
                else
                    winner = _matchResult[_duelCount - 1];
            }

            // TODO save the ranking
        }
    }
}