using OCGWrapper.Enums;
using YGOSharp.Network;
using YGOSharp.Network.Enums;

namespace YGOSharp
{
    public static class GamePacketFactory
    {
        public static GamePacketWriter Create(GameMessage message)
        {
            GamePacketWriter writer = new GamePacketWriter(StocMessage.GameMsg);
            writer.Write((byte)message);
            return writer;
        } 
    }
}
