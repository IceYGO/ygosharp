using System;
using System.IO;
using YGOSharp.OCGWrapper.Enums;

namespace YGOSharp
{
    public class GameAnalyser
    {
        public Game Game { get; private set; }

        public GameAnalyser(Game game)
        {
            Game = game;
        }

        public int Analyse(GameMessage msg, BinaryReader reader, byte[] raw)
        {
            CoreMessage cmsg = new CoreMessage(msg, reader, raw);
            switch (msg)
            {
                case GameMessage.Retry:
                    OnRetry();
                    return 1;
                case GameMessage.Hint:
                    OnHint(cmsg);
                    break;
                case GameMessage.Win:
                    OnWin(cmsg);
                    return 2;
                case GameMessage.SelectBattleCmd:
                    OnSelectBattleCmd(cmsg);
                    return 1;
                case GameMessage.SelectIdleCmd:
                    OnSelectIdleCmd(cmsg);
                    return 1;
                case GameMessage.SelectEffectYn:
                    OnSelectEffectYn(cmsg);
                    return 1;
                case GameMessage.SelectYesNo:
                    OnSelectYesNo(cmsg);
                    return 1;
                case GameMessage.SelectOption:
                    OnSelectOption(cmsg);
                    return 1;
                case GameMessage.SelectCard:
                case GameMessage.SelectTribute:
                    OnSelectCard(cmsg);
                    return 1;
                case GameMessage.SelectChain:
                    return OnSelectChain(cmsg);
                case GameMessage.SelectPlace:
                case GameMessage.SelectDisfield:
                case GameMessage.SelectPosition:
                    OnSelectPlace(cmsg);
                    return 1;
                case GameMessage.SelectCounter:
                    OnSelectCounter(cmsg);
                    return 1;
                case GameMessage.SelectSum:
                    OnSelectSum(cmsg);
                    return 1;
                case GameMessage.SortCard:
                case GameMessage.SortChain:
                    OnSortCard(cmsg);
                    return 1;
                case GameMessage.ConfirmDecktop:
                    OnConfirmDecktop(cmsg);
                    break;
                case GameMessage.ConfirmCards:
                    OnConfirmCards(cmsg);
                    break;
                case GameMessage.ShuffleDeck:
                case GameMessage.RefreshDeck:
                    SendToAll(cmsg, 1);
                    break;
                case GameMessage.ShuffleHand:
                    OnShuffleHand(cmsg);
                    break;
                case GameMessage.SwapGraveDeck:
                    OnSwapGraveDeck(cmsg);
                    break;
                case GameMessage.ReverseDeck:
                    SendToAll(cmsg, 0);
                    break;
                case GameMessage.DeckTop:
                    SendToAll(cmsg, 6);
                    break;
                case GameMessage.ShuffleSetCard:
                    OnShuffleSetCard(cmsg);
                    break;
                case GameMessage.NewTurn:
                    OnNewTurn(cmsg);
                    break;
                case GameMessage.NewPhase:
                    OnNewPhase(cmsg);
                    break;
                case GameMessage.Move:
                    OnMove(cmsg);
                    break;
                case GameMessage.PosChange:
                    OnPosChange(cmsg);
                    break;
                case GameMessage.Set:
                    OnSet(cmsg);
                    break;
                case GameMessage.Swap:
                    SendToAll(cmsg, 16);
                    break;
                case GameMessage.FieldDisabled:
                    SendToAll(cmsg, 4);
                    break;
                case GameMessage.Summoned:
                case GameMessage.SpSummoned:
                case GameMessage.FlipSummoned:
                    SendToAll(cmsg, 0);
                    Game.RefreshMonsters(0);
                    Game.RefreshMonsters(1);
                    Game.RefreshSpells(0);
                    Game.RefreshSpells(1);
                    break;
                case GameMessage.Summoning:
                case GameMessage.SpSummoning:
                    SendToAll(cmsg, 8);
                    break;
                case GameMessage.FlipSummoning:
                    OnFlipSummoning(cmsg);
                    break;
                case GameMessage.Chaining:
                    SendToAll(cmsg, 16);
                    break;
                case GameMessage.Chained:
                    SendToAll(cmsg, 1);
                    Game.RefreshAll();
                    break;
                case GameMessage.ChainSolving:
                    SendToAll(cmsg, 1);
                    break;
                case GameMessage.ChainSolved:
                    SendToAll(cmsg, 1);
                    Game.RefreshAll();
                    break;
                case GameMessage.ChainEnd:
                    SendToAll(cmsg, 0);
                    Game.RefreshAll();
                    break;
                case GameMessage.ChainNegated:
                case GameMessage.ChainDisabled:
                    SendToAll(cmsg, 1);
                    break;
                case GameMessage.CardSelected:
                    OnCardSelected(cmsg);
                    break;
                case GameMessage.RandomSelected:
                    OnRandomSelected(cmsg);
                    break;
                case GameMessage.BecomeTarget:
                    OnBecomeTarget(cmsg);
                    break;
                case GameMessage.Draw:
                    OnDraw(cmsg);
                    break;
                case GameMessage.Damage:
                case GameMessage.Recover:
                case GameMessage.LpUpdate:
                case GameMessage.PayLpCost:
                    OnLpUpdate(cmsg);
                    break;
                case GameMessage.Equip:
                    SendToAll(cmsg, 8);
                    break;
                case GameMessage.Unequip:
                    SendToAll(cmsg, 4);
                    break;
                case GameMessage.CardTarget:
                case GameMessage.CancelTarget:
                    SendToAll(cmsg, 8);
                    break;
                case GameMessage.AddCounter:
                case GameMessage.RemoveCounter:
                    SendToAll(cmsg, 7);
                    break;
                case GameMessage.Attack:
                    SendToAll(cmsg, 8);
                    break;
                case GameMessage.Battle:
                    SendToAll(cmsg, 26);
                    break;
                case GameMessage.AttackDisabled:
                    SendToAll(cmsg, 0);
                    break;
                case GameMessage.DamageStepStart:
                case GameMessage.DamageStepEnd:
                    SendToAll(cmsg, 0);
                    Game.RefreshMonsters(0);
                    Game.RefreshMonsters(1);
                    break;
                case GameMessage.MissedEffect:
                    OnMissedEffect(cmsg);
                    break;
                case GameMessage.TossCoin:
                case GameMessage.TossDice:
                    OnTossCoin(cmsg);
                    break;
                case GameMessage.RockPaperScissors:
                    OnRockPaperScissors(cmsg);
                    return 1;
                case GameMessage.HandResult:
                    SendToAll(cmsg, 1);
                    break;
                case GameMessage.AnnounceRace:
                    OnAnnounceRace(cmsg);
                    return 1;
                case GameMessage.AnnounceAttrib:
                    OnAnnounceAttrib(cmsg);
                    return 1;
                case GameMessage.AnnounceCard:
                    OnAnnounceCard(cmsg);
                    return 1;
                case GameMessage.AnnounceNumber:
                    OnAnnounceNumber(cmsg);
                    return 1;
                case GameMessage.AnnounceCardFilter:
                    OnAnnounceCardFilter(cmsg);
                    return 1;
                case GameMessage.CardHint:
                    SendToAll(cmsg, 9);
                    break;
                case GameMessage.PlayerHint:
                    SendToAll(cmsg, 6);
                    break;
                case GameMessage.MatchKill:
                    OnMatchKill(cmsg);
                    break;
                case GameMessage.TagSwap:
                    OnTagSwap(cmsg);
                    break;
                default:
                    throw new Exception("[GameAnalyser] Unhandled packet id: " + msg);
            }
            return 0;
        }

        private void OnRetry()
        {
            int player = Game.WaitForResponse();
            Game.CurPlayers[player].Send(GamePacketFactory.Create(GameMessage.Retry));

            Game.Replay.End();
        }

        private void OnHint(CoreMessage msg)
        {
            int type = msg.Reader.ReadByte();
            int player = msg.Reader.ReadByte();
            msg.Reader.ReadInt32();

            byte[] buffer = msg.CreateBuffer();
            BinaryWriter packet = GamePacketFactory.Create(msg.Message);
            packet.Write(buffer);

            switch (type)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                    Game.CurPlayers[player].Send(packet);
                    break;
                case 6:
                case 7:
                case 8:
                case 9:
                    Game.SendToAllBut(packet, player);
                    break;
                case 10:
                    if (Game.IsTag)
                        Game.CurPlayers[player].Send(packet);
                    else
                        Game.SendToAll(packet);
                    break;
            }
        }

        private void OnWin(CoreMessage msg)
        {
            int player = msg.Reader.ReadByte();
            int reason = msg.Reader.ReadByte();
            Game.MatchSaveResult(player, reason);
            SendToAll(msg);
        }

        private void OnSelectBattleCmd(CoreMessage msg)
        {
            int player = msg.Reader.ReadByte();
            int count = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(count * 11);
            count = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(count * 8 + 2);
            Game.RefreshAll();
            Game.WaitForResponse(player);
            SendToPlayer(msg, player);
        }

        private void OnSelectIdleCmd(CoreMessage msg)
        {
            int player = msg.Reader.ReadByte();
			int count = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(count * 7);
            count = msg.Reader.ReadByte();
			msg.Reader.ReadBytes(count * 7);
            count = msg.Reader.ReadByte();
			msg.Reader.ReadBytes(count * 7);
            count = msg.Reader.ReadByte();
			msg.Reader.ReadBytes(count * 7);
            count = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(count * 7);
            count = msg.Reader.ReadByte();
			msg.Reader.ReadBytes(count * 11 + 3);

            Game.RefreshAll();
            Game.WaitForResponse(player);
            SendToPlayer(msg, player);
        }

        private void OnSelectEffectYn(CoreMessage msg)
        {
            int player = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(8);
            Game.WaitForResponse(player);
            SendToPlayer(msg, player);
        }

        private void OnSelectYesNo(CoreMessage msg)
        {
            int player = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(4);
            Game.WaitForResponse(player);
            SendToPlayer(msg, player);
        }

        private void OnSelectOption(CoreMessage msg)
        {
            int player = msg.Reader.ReadByte();
            int count = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(count * 4);
            Game.WaitForResponse(player);
            SendToPlayer(msg, player);
        }

        private void OnSelectCard(CoreMessage msg)
        {
            BinaryWriter packet = GamePacketFactory.Create(msg.Message);

            int player = msg.Reader.ReadByte();
            packet.Write((byte)player);
            packet.Write(msg.Reader.ReadBytes(3));

            int count = msg.Reader.ReadByte();
            packet.Write((byte)count);

            for (int i = 0; i < count; i++)
            {
                int code = msg.Reader.ReadInt32();
                int pl = msg.Reader.ReadByte();
                int loc = msg.Reader.ReadByte();
                int seq = msg.Reader.ReadByte();
                int pos = msg.Reader.ReadByte();
                packet.Write(pl == player ? code : 0);
                packet.Write((byte)pl);
                packet.Write((byte)loc);
                packet.Write((byte)seq);
                packet.Write((byte)pos);
            }

            Game.WaitForResponse(player);
            Game.CurPlayers[player].Send(packet);
        }

        private int OnSelectChain(CoreMessage msg)
        {
            int player = msg.Reader.ReadByte();
            int count = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(10 + count * 13);

            if (count > 0)
            {
                Game.WaitForResponse(player);
                SendToPlayer(msg, player);
                return 1;
            }

            Game.SetResponse(-1);
            return 0;
        }

        private void OnSelectPlace(CoreMessage msg)
        {
            int player = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(5);
            Game.WaitForResponse(player);
            SendToPlayer(msg, player);
        }

        private void OnSelectCounter(CoreMessage msg)
        {
            int player = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(4);
            int count = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(count * 9);
            Game.WaitForResponse(player);
            SendToPlayer(msg, player);
        }

        private void OnSelectSum(CoreMessage msg)
        {
            msg.Reader.ReadByte();
            int player = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(6);
            int count = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(count * 11);
            count = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(count * 11);
            Game.WaitForResponse(player);
            SendToPlayer(msg, player);
        }

        private void OnSortCard(CoreMessage msg)
        {
            int player = msg.Reader.ReadByte();
            int count = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(count * 7);
            Game.WaitForResponse(player);
            SendToPlayer(msg, player);
        }

        private void OnConfirmDecktop(CoreMessage msg)
        {
            msg.Reader.ReadByte();
            int count = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(count * 7);
            SendToAll(msg);
        }

        private void OnConfirmCards(CoreMessage msg)
        {
            int player = msg.Reader.ReadByte();
            int count = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(count * 7);

            byte[] buffer = msg.CreateBuffer();
            BinaryWriter packet = GamePacketFactory.Create(msg.Message);
            packet.Write(buffer);
            if ((CardLocation)buffer[7] == CardLocation.Hand)
                Game.SendToAll(packet);
            else
                Game.CurPlayers[player].Send(packet);
        }

        private void OnShuffleHand(CoreMessage msg)
        {
            BinaryWriter packet = GamePacketFactory.Create(msg.Message);
            int player = msg.Reader.ReadByte();
            int count = msg.Reader.ReadByte();
            packet.Write((byte)player);
            packet.Write((byte)count);

            msg.Reader.ReadBytes(count * 4);
            for (int i = 0; i < count; i++)
                packet.Write(0);

            SendToPlayer(msg, player);
            Game.SendToAllBut(packet, player);
            Game.RefreshHand(player);
        }

        private void OnSwapGraveDeck(CoreMessage msg)
        {
            int player = msg.Reader.ReadByte();
            SendToAll(msg);
            Game.RefreshGrave(player);
        }

        private void OnShuffleSetCard(CoreMessage msg)
        {
            int count = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(count * 8);
            SendToAll(msg);
            Game.RefreshMonsters(0);
            Game.RefreshMonsters(1);
        }

        private void OnNewTurn(CoreMessage msg)
        {
            Game.TimeReset();
            if (!Game.IsTag)
                Game.RefreshAll();
            Game.CurrentPlayer = msg.Reader.ReadByte();
            SendToAll(msg);

            Game.TurnCount++;
        }

        private void OnNewPhase(CoreMessage msg)
        {
            msg.Reader.ReadInt16();
            SendToAll(msg);
            Game.RefreshAll();
        }

        private void OnMove(CoreMessage msg)
        {
            byte[] raw = msg.Reader.ReadBytes(16);
            int pc = raw[4];
            int pl = raw[5];
            int cc = raw[8];
            int cl = raw[9];
            int cs = raw[10];
            int cp = raw[11];

            SendToPlayer(msg, cc);
            BinaryWriter packet = GamePacketFactory.Create(msg.Message);
            packet.Write(raw);
            if (!Convert.ToBoolean((cl & ((int)CardLocation.Grave + (int)CardLocation.Overlay))) && Convert.ToBoolean((cl & ((int)CardLocation.Deck + (int)CardLocation.Hand)))
                || Convert.ToBoolean((cp & (int)CardPosition.FaceDown)))
            {
                packet.BaseStream.Position = 2;
                packet.Write(0);
            }
            Game.SendToAllBut(packet, cc);

            if (cl != 0 && (cl & 0x80) == 0 && (cl != pl || pc != cc))
                Game.RefreshSingle(cc, cl, cs);
        }

        private void OnPosChange(CoreMessage msg)
        {
            byte[] raw = msg.Reader.ReadBytes(9);
            SendToAll(msg);

            int cc = raw[4];
            int cl = raw[5];
            int cs = raw[6];
            int pp = raw[7];
            int cp = raw[8];
            if ((pp & (int)CardPosition.FaceDown) != 0 && (cp & (int)CardPosition.FaceUp) != 0)
                Game.RefreshSingle(cc, cl, cs);
        }

        private void OnSet(CoreMessage msg)
        {
            msg.Reader.ReadBytes(4);
            byte[] raw = msg.Reader.ReadBytes(4);
            BinaryWriter packet = GamePacketFactory.Create(GameMessage.Set);
            packet.Write(0);
            packet.Write(raw);
            Game.SendToAll(packet);
        }

        private void OnFlipSummoning(CoreMessage msg)
        {
            byte[] raw = msg.Reader.ReadBytes(8);
            Game.RefreshSingle(raw[4], raw[5], raw[6]);
            SendToAll(msg);
        }

        private void OnCardSelected(CoreMessage msg)
        {
            msg.Reader.ReadByte();
            int count = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(count * 4);
        }

        private void OnRandomSelected(CoreMessage msg)
        {
            msg.Reader.ReadByte();
            int count = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(count * 4);
            SendToAll(msg);
        }

        private void OnBecomeTarget(CoreMessage msg)
        {
            int count = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(count * 4);
            SendToAll(msg);
        }

        private void OnDraw(CoreMessage msg)
        {
            BinaryWriter packet = GamePacketFactory.Create(msg.Message);
            int player = msg.Reader.ReadByte();
            int count = msg.Reader.ReadByte();
            packet.Write((byte)player);
            packet.Write((byte)count);

            for (int i = 0; i < count; i++)
            {
                uint code = msg.Reader.ReadUInt32();
                if ((code & 0x80000000) != 0)
                    packet.Write(code);
                else
                    packet.Write(0);
            }

            SendToTeam(msg, player);
            SendToOpponentTeam(packet, player);
            Game.SendToObservers(packet);
        }

        private void OnLpUpdate(CoreMessage msg)
        {
            int player = msg.Reader.ReadByte();
            int value = msg.Reader.ReadInt32();

            switch (msg.Message)
            {
                case GameMessage.LpUpdate:
                    Game.LifePoints[player] = value;
                    break;
                case GameMessage.PayLpCost:
                case GameMessage.Damage:
                    Game.LifePoints[player] -= value;
                    if (Game.LifePoints[player] < 0)
                        Game.LifePoints[player] = 0;
                    break;
                case GameMessage.Recover:
                    Game.LifePoints[player] += value;
                    break;
            }

            SendToAll(msg);
        }

        private void OnMissedEffect(CoreMessage msg)
        {
            int player = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(7);
            SendToPlayer(msg, player);
        }

        private void OnTossCoin(CoreMessage msg)
        {
            msg.Reader.ReadByte();
            int count = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(count);
            SendToAll(msg);
        }

        private void OnRockPaperScissors(CoreMessage msg)
        {
            int player = msg.Reader.ReadByte();
            Game.WaitForResponse(player);
            SendToPlayer(msg, player);
        }

        private void OnAnnounceRace(CoreMessage msg)
        {
            int player = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(5);
            Game.WaitForResponse(player);
            SendToPlayer(msg, player);
        }

        private void OnAnnounceAttrib(CoreMessage msg)
        {
            int player = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(5);
            Game.WaitForResponse(player);
            SendToPlayer(msg, player);
        }

        private void OnAnnounceCard(CoreMessage msg)
        {
            int player = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(4);
            Game.WaitForResponse(player);
            SendToPlayer(msg, player);
        }

        private void OnAnnounceNumber(CoreMessage msg)
        {
            int player = msg.Reader.ReadByte();
            int count = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(count * 4);
            Game.WaitForResponse(player);
            SendToPlayer(msg, player);
        }

        private void OnAnnounceCardFilter(CoreMessage msg)
        {
            int player = msg.Reader.ReadByte();
            int count = msg.Reader.ReadByte();
            msg.Reader.ReadBytes(count * 4);
            Game.WaitForResponse(player);
            SendToPlayer(msg, player);
        }

        private void OnMatchKill(CoreMessage msg)
        {
            msg.Reader.ReadInt32();
            if (Game.IsMatch)
            {
                Game.MatchKill();
                SendToAll(msg);
            }
        }

        private void OnTagSwap(CoreMessage msg)
        {
            BinaryWriter packet = GamePacketFactory.Create(GameMessage.TagSwap);

            int player = msg.Reader.ReadByte();
            packet.Write((byte)player);

            packet.Write(msg.Reader.ReadByte()); // mcount
            int ecount = msg.Reader.ReadByte();
            packet.Write((byte)ecount);
            packet.Write(msg.Reader.ReadByte()); // pcount
            int hcount = msg.Reader.ReadByte();
            packet.Write((byte)hcount);

            packet.Write(msg.Reader.ReadBytes(4)); // topcode

            for (int i = 0; i < hcount + ecount; i++)
            {
                uint code = msg.Reader.ReadUInt32();
                if ((code & 0x80000000) != 0)
                    packet.Write(code);
                else
                    packet.Write(0);
            }

            if (Game.CurPlayers[player].Equals(Game.Players[player * 2]))
                Game.CurPlayers[player] = Game.Players[player * 2 + 1];
            else
                Game.CurPlayers[player] = Game.Players[player * 2];

            SendToPlayer(msg, player);
            Game.SendToAllBut(packet, player);

            Game.RefreshExtra(player);
            Game.RefreshMonsters(0);
            Game.RefreshMonsters(1);
            Game.RefreshSpells(0);
            Game.RefreshSpells(1);
            Game.RefreshHand(0);
            Game.RefreshHand(1);
        }

        private void SendToAll(CoreMessage msg)
        {
            byte[] buffer = msg.CreateBuffer();
            BinaryWriter packet = GamePacketFactory.Create(msg.Message);
            packet.Write(buffer);
            Game.SendToAll(packet);
        }

        private void SendToAll(CoreMessage msg, int length)
        {
            if (length == 0)
            {
                Game.SendToAll(GamePacketFactory.Create(msg.Message));
                return;
            }
            msg.Reader.ReadBytes(length);
            SendToAll(msg);
        }

        private void SendToPlayer(CoreMessage msg, int player)
        {
            if (player != 0 && player != 1)
                return;
            byte[] buffer = msg.CreateBuffer();
            BinaryWriter packet = GamePacketFactory.Create(msg.Message);
            packet.Write(buffer);
            Game.CurPlayers[player].Send(packet);
        }

        private void SendToTeam(CoreMessage msg, int player)
        {
            if (player != 0 && player != 1)
                return;
            byte[] buffer = msg.CreateBuffer();
            BinaryWriter packet = GamePacketFactory.Create(msg.Message);
            packet.Write(buffer);
            Game.SendToTeam(packet, player);
        }

        private void SendToOpponentTeam(BinaryWriter packet, int player)
        {
            if (player != 0 && player != 1)
                return;
            Game.SendToTeam(packet, 1 - player);
        }
    }
}
