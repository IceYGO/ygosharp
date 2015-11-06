using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using YGOSharp.Network;

namespace YGOSharp
{
    public class CoreServer
    {
        public static int DEFAULT_PORT = 7911;

        public bool IsRunning { get; private set; }
        public bool IsListening { get; private set; }
        public AddonsManager Addons { get; private set; }
        public Game Game { get; private set; }

        private TcpListener _listener;
        private readonly List<CoreClient> _clients;

        private bool _closePending;

        public CoreServer()
        {
            _clients = new List<CoreClient>();
        }

        public void Start()
        {
            if (IsRunning)
                return;
            Addons = new AddonsManager();
            Game = new Game(this);
            Addons.Init(Game);
            try
            {
                _listener = new TcpListener(IPAddress.Any, Config.GetInt("Port", DEFAULT_PORT));
                _listener.Start();
                IsRunning = true;
                IsListening = true;
                Game.Start();
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
            Game.Stop();
            IsRunning = false;
        }

        public void StopDelayed()
        {
            _closePending = true;
            foreach (CoreClient client in _clients)
                client.Close();
        }

        public void AddClient(CoreClient client)
        {
            _clients.Add(client);
            Player player = new Player(Game, client);
            client.MessageReceived += (sender, e) => player.Parse(e.Message);
            client.Closed += (sender, e) => player.OnDisconnected();
        }
        
        public void Tick()
        {
            while (IsListening && _listener.Pending())
                AddClient(new CoreClient(_listener.AcceptTcpClient()));

            List<CoreClient> disconnectedClients = new List<CoreClient>();

            foreach (CoreClient client in _clients)
            {
                client.UpdateNetwork();
                client.Update();
                if (!client.IsConnected)
                {
                    disconnectedClients.Add(client);
                }
            }

            Game.TimeTick();

            while (disconnectedClients.Count > 0)
            {
                _clients.Remove(disconnectedClients[0]);
                disconnectedClients.RemoveAt(0);
            }

            if (_closePending && _clients.Count == 0)
                Stop();
        }
    }
}
