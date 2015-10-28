namespace YGOSharp
{
    public abstract class AddonBase
    {
        public Game Game { get; private set; }

        protected AddonBase(Game game)
        {
            Game = game;
        }
    }
}
