using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace YGOSharp
{
    public class CoreClient
    {
        public bool IsConnected { get; private set; }
        public Game Game { get; private set; }
        public Player Player { get; private set; }

        private readonly CoreServer _server;
        private readonly TcpClient _client;
        private readonly BinaryReader _reader;
        private readonly Queue<GameClientPacket> _recvQueue;
        private readonly Queue<byte[]> _sendQueue;

        private bool _disconnected;
        private bool _closePending;
        private int _receivedLen;

        public CoreClient(CoreServer server, TcpClient client)
        {
            IsConnected = true;
            Game = server.Game;
            Player = new Player(this);
            _server = server;
            _client = client;
            _reader = new BinaryReader(_client.GetStream());
            _recvQueue = new Queue<GameClientPacket>();
            _sendQueue = new Queue<byte[]>();
            _receivedLen = -1;
        }

        public void Close()
        {
            if (!IsConnected)
                return;
            IsConnected = false;
            _client.Close();
            _server.RemoveClient(this);
        }

        public void CloseDelayed()
        {
            _closePending = true;
        }

        public void Send(byte[] raw)
        {
            _sendQueue.Enqueue(raw);
        }

        public void Tick()
        {
            if (IsConnected)
            {
                try
                {
                    CheckDisconnected();
                    NetworkSend();
                    NetworkReceive();
                }
                catch (Exception)
                {
                    _disconnected = true;
                }
            }
            if (_closePending)
            {
                _disconnected = true;
                Close();
                return;
            }
            if (!_disconnected)
            {
                try
                {
                    NetworkParse();
                }
                catch (Exception ex)
                {
                    File.WriteAllText("error_" + DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt", ex.ToString());
                    _disconnected = true;
                }
            }
            if (_disconnected)
            {
                Close();
                Player.OnDisconnected();
            }
        }

        private void CheckDisconnected()
        {
            _disconnected = (_client.Client.Poll(1, SelectMode.SelectRead) && _client.Available == 0);
        }

        private void NetworkReceive()
        {
            if (_client.Available >= 2 && _receivedLen == -1)
                _receivedLen = _reader.ReadUInt16();

            if (_receivedLen != -1 && _client.Available >= _receivedLen)
            {
                GameClientPacket packet = new GameClientPacket(_reader.ReadBytes(_receivedLen));
                _receivedLen = -1;
                lock (_recvQueue)
                    _recvQueue.Enqueue(packet);
            }
        }

        private void NetworkSend()
        {
            while (_sendQueue.Count > 0)
            {
                byte[] raw = _sendQueue.Dequeue();
                MemoryStream stream = new MemoryStream(raw.Length + 2);
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write((ushort)raw.Length);
                writer.Write(raw);
                _client.Client.Send(stream.ToArray());
            }
        }

        private void NetworkParse()
        {
            int count;
            lock (_recvQueue)
                count = _recvQueue.Count;
            while (count > 0)
            {
                GameClientPacket packet = null;
                lock (_recvQueue)
                {
                    if (_recvQueue.Count > 0)
                        packet = _recvQueue.Dequeue();
                    count = _recvQueue.Count;
                }
                if (packet != null)
                    Player.Parse(packet);
            }
        }
    }
}