using System;

namespace YGOSharp
{
    public interface IGame
    {
        /// <summary>
        /// Called when the server is listening and ready to accept incoming connections.
        /// </summary>
        event Action<object, EventArgs> OnNetworkReady;

        /// <summary>
        /// Called right before the server closes, after closing all connections.
        /// </summary>
        event Action<object, EventArgs> OnNetworkEnd;

        /// <summary>
        /// Called when RPS has started.
        /// </summary>
        event Action<object, EventArgs> OnGameStart;

        /// <summary>
        /// Called when a player as won and the game has ended.
        /// </summary>
        event Action<object, EventArgs> OnGameEnd;

        /// <summary>
        /// Called after one duel has been completed (can be called multiple times during a match).
        /// </summary>
        event Action<object, EventArgs> OnDuelEnd;

        /// <summary>
        /// Called when a player joined a duel.
        /// </summary>
        event Action<object, PlayerEventArgs> OnPlayerJoin;

        /// <summary>
        /// Called when a player left a duel.
        /// </summary>
        event Action<object, PlayerEventArgs> OnPlayerLeave;

        /// <summary>
        /// Called when a player move to another lobby spot or to the spectators.
        /// </summary>
        event Action<object, PlayerMoveEventArgs> OnPlayerMove;

        /// <summary>
        /// Called when a player changed his ready state.
        /// </summary>
        event Action<object, PlayerEventArgs> OnPlayerReady;

        /// <summary>
        /// Called when a player sent a chat message.
        /// </summary>
        event Action<object, PlayerChatEventArgs> OnPlayerChat;
    }
}
