using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using ThrowBall.Models;

namespace ThrowBall.TCP
{
    public class Server : TcpBase
    {

        private TcpListener _tcpL; //TcpListener
        private ConcurrentBag<Connection> _clients;
        private int _maxNumberOfClient = 1000;
        private int _currentNumberOfClient = 0;


        public Action<Guid> OnClientConnected;
        public Action<Guid, byte[]> OnMessageReceived;
        public Action<Guid> OnClientDisconnected;


        public int GetCurrentNumberOfClient()
        {
            return _currentNumberOfClient;
        }

        public Server( int numberOfClients)
        {
            _maxNumberOfClient = numberOfClients;
            _clients = new ConcurrentBag<Connection>();
        }


        public bool Start(int port) {
            try
            {
                _tcpL = new TcpListener(IPAddress.Any, port);
                _tcpL.Start();
                incomingQueue = new ConcurrentQueue<Packet>();
                receiveThread = new Thread(() => StartListening())
                {
                    IsBackground = true,
                    Priority = ThreadPriority.AboveNormal
                };
                receiveThread.Start();
                
                return true;
            } catch (SocketException e) {
                return false;
            }
        }

        private void StartListening() {
            if (_tcpL != null)
            {
                try
                {
                    while (true)
                    {
                        TcpClient client = _tcpL.AcceptTcpClient();
                        client.NoDelay = true;
                        client.SendTimeout = 5000;
                        client.ReceiveTimeout = 0;
                        Interlocked.Increment(ref _currentNumberOfClient);
                        var connection = new Connection()
                        {
                            Id = Guid.NewGuid(),
                            IsOpen = true,
                            PendingQueue = new ConcurrentQueue<Packet>(),
                            Client = client
                        };
                        _clients.Add(connection);
                        Thread listenThread = new Thread(() => TcpHelpers.ListenTo(connection, maxMessageSize, incomingQueue))
                        {
                            IsBackground = true,
                            Priority = ThreadPriority.BelowNormal
                        };
                        listenThread.Start();

                        Thread sendThread = new Thread(() => TcpHelpers.SendTo(connection))
                        {
                            IsBackground = true,
                            Priority = ThreadPriority.BelowNormal
                        };
                        sendThread.Start();
                    }
                }
                catch (ThreadAbortException threadAbortException)
                {

                }
                catch (SocketException socketException)
                {

                }
                catch (Exception exception)
                {
                    // ignored
                }
            } 
        }
        

        public bool SendMessage(Guid id, byte[] load)
        {
            if (_clients.TryPeek(out Connection connection))
            {
                if (connection.PendingQueue.Count > maxQueueSize || load.Length > maxMessageSize)
                {
                    return false;
                }
                
                connection.PendingQueue.Enqueue(new Packet(id, Meta.Message, load));

                return true;
            }

            return false;
        }
        
        
        public bool DisconnectClient(Guid id)
        {
            _clients.TryPeek(out Connection connection);
            if (connection != null)
            {
                connection.IsOpen = false;
                return true;
            }
            return false;
        }

        private void SendUpdates()
        {
            //TODO: write logic to send pending messages from the queue
        }
        
        public override bool ProcessNextMessage()
        {
            var result = incomingQueue.TryDequeue(out Packet packet);
            if (!result) return false;

            switch (packet.MetaData)
            {
                case Meta.Connect: 
                    OnClientConnected?.Invoke(packet.Id);
                    break;
                case Meta.Message:
                    OnMessageReceived?.Invoke(packet.Id, packet.Message);
                    break;
                case Meta.Disconnect:
                    OnClientDisconnected?.Invoke(packet.Id);
                    break;
            }
            return true;
        }

    }
}