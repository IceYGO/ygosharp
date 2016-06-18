using System.IO;
using System.Net.Sockets;

namespace YGOSharp.Network
{
    public class YGOClient : BinaryClient
    {
        public YGOClient()
            : base(new NetworkClient())
        {
        }

        public YGOClient(Socket socket)
            : base(new NetworkClient(socket))
        {
        }

        public void Send(BinaryWriter writer)
        {
            Send(((MemoryStream)writer.BaseStream).ToArray());
        }
    }
}
