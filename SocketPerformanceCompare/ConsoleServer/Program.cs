using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ConsoleServer
{
    class Program
    {
        public static void StartListening()
        {
            // Data buffer for incoming data.  
            byte[] buffer = new Byte[1024];

            IPAddress LocalIp = IPAddress.Any; //System.Net.IPAddress.Parse("127.0.0.1");
            IPEndPoint localEndPoint = new IPEndPoint(LocalIp, 50000);

            // Create a UDP/IP socket.  
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );

            // Bind the socket to the local endpoint and   
            // listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                Console.WriteLine("Bind Complete");

                int messagesReceived = 0;

                while(true)
                {

                    EndPoint receiveEP = new IPEndPoint(IPAddress.Any, 0);
                    int receivedBytes = listener.ReceiveFrom(buffer, ref receiveEP);

                    messagesReceived++;

                    IPEndPoint receiveIPEP = receiveEP as IPEndPoint;
                    Console.WriteLine("{0}::{1}::{2} Client Request ({3} bytes):: {4}.",
                        messagesReceived,
                        receiveIPEP.Address.ToString(),
                        receiveIPEP.Port.ToString(),
                        receivedBytes,
                        System.Text.Encoding.UTF8.GetString(buffer, 0, receivedBytes));

                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            //Console.WriteLine("\nPress ENTER to continue...");
            //Console.Read();

        }

        static void Main(string[] args)
        {
            StartListening();
        }
    }
}
