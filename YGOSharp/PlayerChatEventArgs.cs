namespace YGOSharp
{
    public class PlayerChatEventArgs : PlayerEventArgs
    {
        public string Message { get; private set; }

        public PlayerChatEventArgs(Player player, string message)
            : base(player)
        {
            Message = message;
        }
    }
}
