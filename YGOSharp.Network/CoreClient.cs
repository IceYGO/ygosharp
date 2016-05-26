using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using YGOSharp.Network.Enums;

namespace YGOSharp.Network
{
    public class CoreClient
    {
        public bool IsConnected { get; private set; }
        
        public event Action<object, EventArgs> Opened;
        public event Action<object, EventArgs> Closed;
        public event Action<object, ExceptionEventArgs> ErrorCatched;
        public event Action<object, MessageEventArgs> MessageReceived;

        private TcpClient _client;
        private BinaryReader _reader;
        private Queue<GamePacketReader> _recvQueue;
        private Queue<byte[]> _sendQueue;
        
        private bool _closePending;
        private int _receivedLen;

        private Thread _networkThread;

        public CoreClient(string hostname, int port)
        {
            try
            {
                InitializeClient(new TcpClient(hostname, port));
            }
            catch (Exception)
            {
                IsConnected = false;
            }
        }

        public CoreClient(TcpClient client)
        {
            InitializeClient(client);
        }

        public void StartNetworkThread()
        {
            _networkThread = new Thread(NetworkThreadMain);
            _networkThread.IsBackground = true;
            _networkThread.Start();
        }

        public void Close()
        {
            _closePending = true;
        }

        public void Send(GamePacketWriter writer)
        {
            Send(writer.GetContent());
        }

        public void Send(CtosMessage message)
        {
            Send(new GamePacketWriter(message));
        }

        public void Send(CtosMessage message, byte value)
        {
            GamePacketWriter packet = new GamePacketWriter(message);
            packet.Write(value);
            Send(packet);
        }

        public void Send(CtosMessage message, int value)
        {
            GamePacketWriter packet = new GamePacketWriter(message);
            packet.Write(value);
            Send(packet);
        }

        public void Send(byte[] raw)
        {
            lock (_sendQueue)
                _sendQueue.Enqueue(raw);
        }

        public virtual void Update()
        {
            NetworkParse();
            if (!IsConnected && _closePending)
            {
                _closePending = false;
                OnClose();
            }
        }

        public virtual void UpdateNetwork()
        {
            if (IsConnected)
            {
                try
                {
                    CheckDisconnected();
                    if (IsConnected)
                    {
                        NetworkSend();
                        NetworkReceive();
                    }
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            }
            if (IsConnected && _closePending)
            {
                IsConnected = false;
                try
                {
                    _client.Close();
                }
                catch (Exception)
                {
                    // ignore
                }
            }
        }

        private void OnOpen()
        {
            if (Opened != null)
            {
                Opened(this, EventArgs.Empty);
            }
        }

        private void OnClose()
        {
            if (Closed != null)
            {
                Closed(this, EventArgs.Empty);
            }
        }

        private void OnError(Exception ex)
        {
            Close();
            if (ErrorCatched != null)
            {
                ErrorCatched(this, new ExceptionEventArgs(ex));
            }
        }

        private void OnMessage(GamePacketReader message)
        {
            if (MessageReceived != null)
            {
                MessageReceived(this, new MessageEventArgs(message));
            }
        }

        private void CheckDisconnected()
        {
            IsConnected = !(_client.Client.Poll(1, SelectMode.SelectRead) && _client.Available == 0);
            if (!IsConnected)
                Close();
        }

        private void NetworkReceive()
        {
            if (_client.Available >= 2 && _receivedLen == -1)
                _receivedLen = _reader.ReadUInt16();

            if (_receivedLen != -1 && _client.Available >= _receivedLen)
            {
                GamePacketReader packet = new GamePacketReader(_reader.ReadBytes(_receivedLen));
                _receivedLen = -1;
                lock (_recvQueue)
                    _recvQueue.Enqueue(packet);
            }
        }

        private void NetworkSend()
        {
            int count;
            lock (_sendQueue)
                count = _sendQueue.Count;
            while (count > 0)
            {
                byte[] raw = null;
                lock (_sendQueue)
                {
                    if (_sendQueue.Count > 0)
                        raw = _sendQueue.Dequeue();
                    count = _sendQueue.Count;
                }
                if (raw != null)
                {
                    MemoryStream stream = new MemoryStream(raw.Length + 2);
                    BinaryWriter writer = new BinaryWriter(stream);
                    writer.Write((ushort)raw.Length);
                    writer.Write(raw);
                    _client.Client.Send(stream.ToArray());
                }
            }
        }

        private void NetworkParse()
        {
            int count;
            lock (_recvQueue)
                count = _recvQueue.Count;
            while (count > 0)
            {
                GamePacketReader packet = null;
                lock (_recvQueue)
                {
                    if (_recvQueue.Count > 0)
                        packet = _recvQueue.Dequeue();
                    count = _recvQueue.Count;
                }
                if (packet != null)
                    OnMessage(packet);
            }
        }

        private void InitializeClient(TcpClient client)
        {
            IsConnected = true;

            _client = client;
            _reader = new BinaryReader(_client.GetStream());
            _recvQueue = new Queue<GamePacketReader>();
            _sendQueue = new Queue<byte[]>();
            _receivedLen = -1;

            OnOpen();
        }

        private void NetworkThreadMain()
        {
            while (IsConnected)
            {
                UpdateNetwork();
                Thread.Sleep(1);
            }
        }
    }
}
