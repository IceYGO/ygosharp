using System;

namespace YGOSharp.Network
{
    public class MessageEventArgs : EventArgs
    {
        public GamePacketReader Message { get; private set; }

        public MessageEventArgs(GamePacketReader message)
        {
            Message = message;
        }
    }
}
