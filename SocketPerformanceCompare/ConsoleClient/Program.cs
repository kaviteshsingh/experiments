using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                byte[] buffer = new byte[1024];

                IPEndPoint localEP = new IPEndPoint(IPAddress.Any, 50001);
                IPAddress remoteIp = System.Net.IPAddress.Parse("127.0.0.1");
                IPEndPoint remoteEP = new IPEndPoint(remoteIp, 50000);

                Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                try
                {
                    sender.Bind(localEP);

                    for(int i = 0; i < 10000; i++)
                    {
                        //Thread.Sleep(1);
                        // Encode the data string into a byte array. 
                        string ClientReq =  "Client Request:: " + i.ToString() + " "+ Guid.NewGuid().ToString();
                        byte[] msg = Encoding.ASCII.GetBytes(ClientReq);

                        // Send the data through the socket.  
                        int sentBytes = sender.SendTo(msg, remoteEP);
                        Console.WriteLine("{0}::{1} Client Request ({2} bytes):: {3}.",
                        remoteEP.Address.ToString(),
                        remoteEP.Port.ToString(),
                        sentBytes,
                        ClientReq
                        );

                    }

                    // Release the socket.  
                    sender.Close();
                    //Console.ReadKey();

                }
                catch(ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch(SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch(Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
