using System;
using System.Text;
using System.IO;
using System.Net.Sockets;
using Packets;
using System.Runtime.Serialization.Formatters.Binary;

namespace chat_server
{

    public class Client
    {
       private readonly Socket _socket;
       private readonly NetworkStream _stream;
       public Guid Id { get; }
       public string Username { get; set; }
       public string Status { get; set; }
       public BinaryReader Reader { private set; get; }
       public BinaryWriter Writer { private set; get; }
       

        
        public Client(Socket socket)
        {
            Id = Guid.NewGuid();
            _socket = socket;
            _stream = new NetworkStream(_socket);
            Reader = new BinaryReader(_stream, Encoding.UTF8);
            Writer = new BinaryWriter(_stream, Encoding.UTF8);

        }

        public void SendPacket(Packet packet)
        {
            var memStream = new MemoryStream();
            var formatter = new BinaryFormatter();
            
            formatter.Serialize(memStream, packet);
            byte[] buffer = memStream.GetBuffer();
            Writer.Write(buffer.Length);
            Writer.Write(buffer);
            Writer.Flush();

        }

        public void Close()
        {
            Writer.Write(0);
            _stream.Close();
            _socket.Close();
        }
    }
}
