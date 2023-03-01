using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using chat_server.Services;

namespace chat_server
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var server = new ChatService("127.0.0.1", 4444);
            server.Start();
        }
    }
}
