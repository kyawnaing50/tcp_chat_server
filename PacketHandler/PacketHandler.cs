using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Packets;

namespace chat_server.PacketHandler
{
    public class PacketHandler
    {
        private List<Client> _connectedClients;
        
        private const string ServerPrefix = "[Server]: ";


        public PacketHandler(List<Client> connectedClients)
        {
            _connectedClients = connectedClients;
        }

        public void UpdateConnectedClients(List<Client> connectedClients)
        {
            _connectedClients = connectedClients;
        }

        public void HandleForClient(Client client)
        {
            int noOfIncomingBytes;
            while ((noOfIncomingBytes = client.Reader.ReadInt32()) != 0)
            {
                Console.WriteLine(noOfIncomingBytes);

                byte[] bytes = client.Reader.ReadBytes(noOfIncomingBytes);
                MemoryStream memStream = new MemoryStream(bytes);
                var formatter = new BinaryFormatter();
                Packet packet = formatter.Deserialize(memStream) as Packet;

                if (packet.packetType == PacketType.DISCONNECT)
                {
                    _connectedClients.Remove(client);
                    HandlePacket(client, packet);
                    break;
                }
                HandlePacket(client, packet);
            }
            
            _connectedClients.Remove(client);
            SendUpdatedUserList();
            client.Close();
        }

        private void HandlePacket(Client client, Packet packet)
        {
            switch (packet.packetType)
            {
                case PacketType.USER:
                    client.Username = ((UserPacket) packet).nickname;
                    client.Status = ((UserPacket) packet).status;
                    SendUpdatedUserList();
                    break;
                case PacketType.CHATMESSAGE:
                    string message = ((ChatMessagePacket) packet).message;
                    ChatMessagePacket chatMessage =
                        new ChatMessagePacket("[" + client.Username + "]: " + message+ ": from Server");
                    foreach (Client connectedClient in _connectedClients)
                    {
                        connectedClient.SendPacket(chatMessage);
                        Console.WriteLine(message);
                    }

                    break;
                case PacketType.SERVER_MESSAGE:
                    string servermessage = ((ServerMessagePacket) packet).message;
                    ServerMessagePacket serverMessage = new ServerMessagePacket(ServerPrefix + servermessage);
                    foreach (Client connectedClient in _connectedClients)
                    {
                        connectedClient.SendPacket(serverMessage);
                    }

                    break;
                case PacketType.POKE:
                    Console.WriteLine("got poker");
                    string pokeSender = ((PokePacket) packet).sender;
                    string pokeRecipient = ((PokePacket) packet).recipient;
                    foreach (Client connectedClient in _connectedClients)
                    {
                        if (connectedClient.Username == pokeRecipient)
                        {
                            connectedClient.SendPacket(new PokePacket(pokeSender, pokeRecipient));
                            Console.WriteLine(connectedClient.Username);
                        }
                    }

                    break;
                case PacketType.COMMAND:
                    switch (((CommandPacket) packet).command)
                    {
                        case "!roll":
                            Random rnd = new Random();
                            int roll = rnd.Next(1, 99);
                            ServerMessagePacket rollres =
                                new ServerMessagePacket(ServerPrefix + client.Username + " rolled: " + roll);
                        {
                            foreach (Client connectedClient in _connectedClients)
                            {
                                connectedClient.SendPacket(rollres);
                            }
                        }
                            break;

                        default:
                            break;
                    }

                    break;
                case PacketType.DISCONNECT:
                    foreach (Client c in _connectedClients)
                    {
                        Console.WriteLine("Sending");
                        c.SendPacket(new ServerMessagePacket(ServerPrefix + client.Username + " has left the server."));
                    }

                    break;
            }
        }
        
        private void SendUpdatedUserList()
        {
            List<string> userlist = new List<string>();

            foreach (Client connectedClient in _connectedClients)
            {
                if(connectedClient.Username != null)
                {
                    Console.WriteLine(connectedClient.Username);
                    userlist.Add(connectedClient.Username + "{" + connectedClient.Status + "}");

                }
            }

            foreach(Client connectedClient in _connectedClients)
            {
                Console.WriteLine("Sending userlist");
                connectedClient.SendPacket(new UserListPacket(userlist));
            }
        }
    }
}