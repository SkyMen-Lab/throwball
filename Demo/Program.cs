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
            Console.WriteLine("Enter your choice:");
            Console.WriteLine("1. Server\n2. Client");
            choice = int.Parse(Console.ReadLine());
            switch (choice)
            {
                case 1: CreateServer();
                    break;
                case 2: CreateClient();
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
                        Console.ReadLine();
                        break;
                    case 2:
                        ProcessUpdates(_client, 100);
                        Console.WriteLine("Enter the message");
                        var message = Console.ReadLine();
                        _client.SendMessage(Encoding.ASCII.GetBytes(message));
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
            _client.Connect("127.0.0.1", 5000);
            _client.OnConnected = () => Console.WriteLine("Connected to server from client");
            _client.OnMessageReceived = (bytes =>
            {
                var message= Encoding.ASCII.GetString(bytes);
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