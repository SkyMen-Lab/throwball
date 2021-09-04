using System;
using System.Text;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using ThrowBall.Models;
using ThrowBall.Logger;

namespace ThrowBall.TCP
{
    public class Client : TcpBase
    {

        public Action OnConnected;
        public Action<byte[]> OnMessageReceived;
        public Action OnDisconnected;
        
        private Connection _connection;

        private Thread _sendThread;

        public bool IsConnected { get  {
            if (_connection == null) {
                return false;
            }
            return _connection.IsOpen && _connection.Client.Connected;
        } } 
        
        public bool IsPinging { get; set; } = true;

        public Client()
        {
            _connection = new Connection()
            {
                Id = Guid.Empty,
                IsOpen = true,
                Client = new TcpClient(),
                PendingQueue = new ConcurrentQueue<Packet>()
            };
        }


        public bool Connect(string ip, int port)
        {
            _connection.Client.Connect(ip, port);

            if (_connection.Client.Connected)
            {
                incomingQueue = new ConcurrentQueue<Packet>();
                _sendThread = new Thread(() =>
                {
                    TcpHelpers.SendTo(_connection);
                })
                {
                    IsBackground = true,
                };
                _sendThread.Start();
                receiveThread = new Thread(() =>
                {
                    TcpHelpers.ListenTo(_connection, maxMessageSize, incomingQueue);
                })
                {
                    IsBackground = true
                };
                receiveThread.Start();

                Thread pingThread = new Thread(Ping);
                pingThread.Priority = ThreadPriority.Lowest;
                pingThread.IsBackground = true;
                pingThread.Start();

                Log.Info("New client has been connected");
                return true;
            }
            Log.Warning("Error connecting new client");
            return false;
        }

        private void Ping() {
            if (IsConnected && IsPinging) {
                while(true) {
                    var load = ASCIIEncoding.ASCII.GetBytes("ping");
                    if(!SendMessage(load)) {
                        Log.Warning("Connection is broken!");
                        _connection.SetConnectionStatus(false);
                        break;
                    }
                    Thread.Sleep(5000);
                }
            }
        }

        public bool SendMessage(byte[] load)
        {
            //TODO: check whether stream is writable
            if (!_connection.IsOpen) {
                return false;
            }
            int size = load.Length;
            if (size > maxMessageSize)
            {
                Log.Warning("Attempt to send message of greater than allowed size");
                return false;
            }

            Packet packet = new Packet(Guid.Empty, Meta.Message, load);
            
            _connection.PendingQueue.Enqueue(packet);
            _connection.SendManualReset.Set();
            Log.Info("Message has successfully been queued");

            return true;
        }


        //use this method to send a disconnect packet to the server
        //receive pending messages from it
        //and close connection
        public void Disconnect()
        {
            _connection.SetConnectionStatus(false);
            Packet packet = new Packet(Guid.Empty, Meta.Disconnect, default);
            _connection.PendingQueue.Enqueue(packet);
        }

        public void ForceDisconnect()
        {
            _connection.Client.GetStream().Close();
            _connection.Client.Close();
            _connection.PendingQueue.Clear();
            incomingQueue.Clear();
        }

        public override bool ProcessNextMessage()
        {
            if (_connection.IsOpen)
            {
                return false;
            }

            var result = incomingQueue.TryDequeue(out Packet packet);
            if (!result) return false;

            switch (packet.MetaData)
            {
                case Meta.Connect: 
                    OnConnected?.Invoke();
                    break;
                case Meta.Message:
                    OnMessageReceived?.Invoke(packet.Message);
                    break;
                case Meta.Disconnect:
                    OnDisconnected?.Invoke();
                    break;
            }
            return true;
        }
        
    }
}