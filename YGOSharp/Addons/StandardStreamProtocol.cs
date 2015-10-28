using System;
using YGOSharp.Enums;

namespace YGOSharp.Addons
{
    public class StandardStreamProtocol : AddonBase
    {
        public StandardStreamProtocol(Game game)
            : base(game)
        {
            Game.OnNetworkReady += Game_OnNetworkReady;
            Game.OnNetworkEnd += Game_OnNetworkEnd;
            Game.OnPlayerChat += Game_OnPlayerChat;
            Game.OnPlayerJoin += Game_OnPlayerJoin;
            Game.OnPlayerLeave += Game_OnPlayerLeave;
            Game.OnPlayerMove += Game_OnPlayerMove;
            Game.OnPlayerReady += Game_OnPlayerReady;
            Game.OnGameStart += Game_OnGameStart;
            Game.OnGameEnd += Game_OnGameEnd;
            Game.OnDuelEnd += Game_OnDuelEnd;
        }

        private void Game_OnNetworkReady(object sender, EventArgs e)
        {
            Console.WriteLine("::::network-ready");
        }

        private void Game_OnNetworkEnd(object sender, EventArgs e)
        {
            Console.WriteLine("::::network-end");
        }

        private void Game_OnPlayerChat(object sender, PlayerChatEventArgs e)
        {
            Console.WriteLine("::::chat|" + e.Player.Name + "|" + e.Message);
        }

        private void Game_OnPlayerJoin(object sender, PlayerEventArgs e)
        {
            if (Game.State != GameState.Lobby)
                return;

            if (e.Player.Type != (int)PlayerType.Observer)
            {
                Console.WriteLine("::::join-slot|" + e.Player.Type + "|" + e.Player.Name);
            }
            else
            {
                Console.WriteLine("::::spectator|" + Game.Observers.Count);
            }
        }

        private void Game_OnPlayerLeave(object sender, PlayerEventArgs e)
        {
            if (Game.State != GameState.Lobby)
                return;

            if (e.Player.Type != (int)PlayerType.Observer)
            {
                Console.WriteLine("::::left-slot|" + e.Player.Type + "|" + e.Player.Name);
            }
            else
            {
                Console.WriteLine("::::spectator|" + Game.Observers.Count);
            }
        }

        private void Game_OnPlayerMove(object sender, PlayerMoveEventArgs e)
        {
            if (Game.State != GameState.Lobby)
                return;

            if (e.FromType != (int)PlayerType.Observer)
            {
                Console.WriteLine("::::left-slot|" + e.FromType + "|" + e.Player.Name);
            }
            if (e.Player.Type != (int)PlayerType.Observer)
            {
                Console.WriteLine("::::join-slot|" + e.Player.Type + "|" + e.Player.Name);
            }
            if (e.FromType == (int)PlayerType.Observer || e.Player.Type == (int)PlayerType.Observer)
            {
                Console.WriteLine("::::spectator|" + Game.Observers.Count);
            }
        }

        private void Game_OnPlayerReady(object sender, PlayerEventArgs e)
        {
            Console.WriteLine("::::lock-slot|" + e.Player.Type + "|" + Game.IsReady[e.Player.Type]);
        }

        private void Game_OnGameStart(object sender, EventArgs e)
        {
            Console.WriteLine("::::start-game");
        }

        private void Game_OnGameEnd(object sender, EventArgs e)
        {
            Console.WriteLine("::::end-game|" + Game.Winner);
        }

        private void Game_OnDuelEnd(object sender, EventArgs e)
        {
            Console.WriteLine("::::end-duel|" + Game.MatchResults[Game.DuelCount] + "|" + Game.MatchReasons[Game.DuelCount]);
        }
    }
}
