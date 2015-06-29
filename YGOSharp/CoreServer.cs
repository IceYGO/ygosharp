using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace YGOSharp
{
    public class CoreServer
    {
        public bool IsRunning { get; private set; }
        public bool IsListening { get; private set; }
        public Game Game { get; private set; }
        public CoreConfig Config { get; private set; }

        private TcpListener _listener;
        private readonly List<CoreClient> _clients;
        private readonly List<CoreClient> _removedClients;

        private bool _closePending;

        public CoreServer(CoreConfig config)
        {
            _clients = new List<CoreClient>();
            _removedClients = new List<CoreClient>();
            Config = config;
        }

        public void Start()
        {
            if (IsRunning)
                return;
            Game = new Game(this, Config);
            try
            {
                _listener = new TcpListener(IPAddress.Any, Config.Port);
                _listener.Start();
                IsRunning = true;
                IsListening = true;
            }
            catch (Exception)
            {
                //ignore
            }
        }

        public void StopListening()
        {
            if (!IsListening)
                return;
            IsListening = false;
            _listener.Stop();
        }

        public void Stop()
        {
            if (IsListening)
                StopListening();
            foreach (CoreClient client in _clients)
                client.Close();
            IsRunning = false;
        }

        public void StopDelayed()
        {
            _closePending = true;
            foreach (CoreClient client in _clients)
                client.CloseDelayed();
        }

        public void AddClient(CoreClient client)
        {
            _clients.Add(client);
        }

        public void RemoveClient(CoreClient client)
        {
            _removedClients.Add(client);
        }

        public void Tick()
        {
            while (IsListening && _listener.Pending())
                AddClient(new CoreClient(this, _listener.AcceptTcpClient()));

            foreach (CoreClient client in _clients)
                client.Tick();

            Game.TimeTick();

            while (_removedClients.Count > 0)
            {
                _clients.Remove(_removedClients[0]);
                _removedClients.RemoveAt(0);
            }

            if (_closePending && _clients.Count == 0)
                Stop();
        }
    }
}
