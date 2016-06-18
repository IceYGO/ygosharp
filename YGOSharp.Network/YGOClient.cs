using System.IO;

namespace YGOSharp.Network
{
    public class YGOClient : BinaryClient
    {
        public YGOClient()
            : base(new NetworkClient())
        {
        }

        public YGOClient(NetworkClient client)
            : base(client)
        {
        }

        public void Send(BinaryWriter writer)
        {
            Send(((MemoryStream)writer.BaseStream).ToArray());
        }
    }
}
