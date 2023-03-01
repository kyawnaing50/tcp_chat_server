using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace chat_server.Services
{
    public class ChatService
    {
        private readonly TcpListener _tcpListener;
        private static List<Client> _clients;
        private readonly PacketHandler.PacketHandler _packetHandler;

        public ChatService(string ipAddress, int port)
        {
            _tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
            _clients = new List<Client>();
            _packetHandler = new PacketHandler.PacketHandler(_clients);
        }

        public void Start()
        {
            _tcpListener.Start();
            Console.WriteLine("Listener Started.");
            
            var connectionHandlingThread = new Thread(HandleConnections);
            connectionHandlingThread.Start();

            while (true)
            {
                Console.WriteLine("Please enter a command...");
                switch (Console.ReadLine()?.ToLower())
                {
                    case "connected":
                        ChatServiceCommands.ListConnectedClients(_clients);
                        break;
                    case "exit":
                        connectionHandlingThread.Abort();
                        Stop();
                        return;
                    default:
                        Console.WriteLine("Command not recognised.");
                        break;
                }
            }
        }

        private void HandleConnections()
        {
            while (true)
            {
                if (_tcpListener.Pending())
                {
                    Socket socket = _tcpListener.AcceptSocket();
                    Client client = new Client(socket);
                    _clients.Add(client);
                
                    Console.WriteLine($"Added client {client.Id} successfully.");
                    _packetHandler.UpdateConnectedClients(_clients);
                
                    Thread thread = new Thread(new ParameterizedThreadStart(HandlePacketFromClient));
                    thread.Start(client);
                }
            } 
        }

        private void Stop()
        {
            _tcpListener.Stop();
            Console.WriteLine("Listener Stopped.");
        }

        private void HandlePacketFromClient(object clientObj)
        {
            Client client = (Client)clientObj;
            _packetHandler.HandleForClient(client);
        }
    }
}
