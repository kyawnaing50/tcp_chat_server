using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using chat_server.Services;
using System.Net;
using System.Net.Sockets;

namespace chat_server
{
    internal class Program
    {
       
        static string[] addressList;
        public static void Main(string[] args)
        {
            IPAddressList();
            Console.Write("\nChoose Index of IP,example 0,1,2,..etc: ");
            string addressIndex = Console.ReadLine();
            Console.WriteLine("\t\t\tServer is  Starting at : " + addressList[Convert.ToInt32(addressIndex)]);
            var server = new ChatService(addressList[Convert.ToInt32(addressIndex)], 4444);
            server.Start();
        }

        private static void IPAddressList()
        {
            String strHostName = string.Empty;
            // Getting Ip address of local machine...
            // First get the host name of local machine.
            strHostName = Dns.GetHostName();
            Console.WriteLine("\t\t\tServer Name: " + strHostName);
            // Then using host name, get the IP address list..
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            IPAddress[] addr = ipEntry.AddressList;
            addressList = new string[10];
            for (int i = 0; i < addr.Length; i++)
            {
                Console.WriteLine("\t\t\tAddress {0}: {1} ", i, addr[i].ToString());
                addressList[i] = addr[i].ToString();
            }

        }
    }   
}


