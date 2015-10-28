namespace YGOSharp
{
    public class PlayerMoveEventArgs : PlayerEventArgs
    {
        public int FromType { get; private set; }

        public PlayerMoveEventArgs(Player player, int fromType)
            : base(player)
        {
            FromType = fromType;
        }
    }
}
