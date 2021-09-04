using System;
using System.Text;
using ThrowBall;
using ThrowBall.TCP;

namespace Demo
{
    class Program
    {
        private static Client _client;
        private static Server _server;
        private static bool open = true;
        private static int choice;

        static void Main(string[] args)
        {
            ThrowBall.Logger.Log.SetupLogger(
                infoAction: (info) => Console.WriteLine(info),
                warningAction: (info) => Console.WriteLine(info),
                errorAction: (info, e) => Console.WriteLine(info)
            );
            Console.WriteLine("Enter your choice:");
            Console.WriteLine("1. Server\n2. Client");
            choice = int.Parse(Console.ReadLine());
            switch (choice)
            {
                case 1:
                    CreateServer();
                    break;
                case 2:
                    CreateClient();
                    break;
                default:
                    Console.WriteLine("Incorrect input");
                    break;
            }

            while (open)
            {

                switch (choice)
                {
                    case 1:
                        ProcessUpdates(_server, 100);
                        // _server.SendMessage(Guid.Empty, Encoding.ASCII.GetBytes(message));
                        //Console.ReadLine();
                        break;
                    case 2:
                        string message;
                        if (!_client.IsConnected)
                        {
                            Console.WriteLine("connect or quit?");
                            message = Console.ReadLine();
                            if (message == "connect")
                            {
                                _client = ThrowServer.CreateClient();
                                _client.Connect("127.0.0.1", 5000);
                            }
                            else
                            {
                                Console.WriteLine("Ending session");
                                return;
                            }
                        }
                        else
                        {
                            ProcessUpdates(_client, 100);
                            Console.WriteLine("Enter the message");
                            message = Console.ReadLine();
                            var s = _client.SendMessage(Encoding.ASCII.GetBytes(message));
                            if (!s)
                            {
                                Console.WriteLine("You are disconnected");
                            }
                            if (message == "disconnect") {
                            _client.Disconnect();
                            }
                        }
                        break;
                    default:
                        Console.WriteLine("Incorrect input");
                        break;
                }
            }

        }

        public static void CreateClient()
        {
            _client = ThrowServer.CreateClient();
            _client.IsPinging = false;
            _client.Connect("127.0.0.1", 5000);
            _client.OnConnected = () => Console.WriteLine("Connected to server from client");
            _client.OnMessageReceived = (bytes =>
            {
                var message = Encoding.ASCII.GetString(bytes);
                Console.WriteLine("Message from server: " + message);
            });
            _client.OnDisconnected = () => Console.WriteLine("Disconnected from the server");
        }



        public static void CreateServer()
        {
            _server = ThrowServer.CreateServer();
            _server.Start(5000);
            _server.OnClientConnected = guid => Console.WriteLine($"Client connected - {guid}");
            _server.OnMessageReceived = (guid, bytes) =>
            {
                var message = Encoding.ASCII.GetString(bytes);
                if (message.ToLower().Contains("disconnect"))
                {
                    if (_server.DisconnectClient(guid))
                        Console.WriteLine($"Message from {guid} - {message} - trying to disconnect client");
                    else Console.WriteLine($"Error disconneting a client {guid}");
                }
                else
                    Console.WriteLine($"Message from {guid} - {message}");
            };
            _server.OnClientDisconnected = guid => Console.WriteLine($"Client disconnected - {guid}");
        }

        public static void ProcessUpdates(TcpBase tcpBase, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var result = tcpBase.ProcessNextMessage();
                if (result == false) break;
            }
        }
    }
}