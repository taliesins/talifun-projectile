using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Talifun.Projectile.Command;
using Talifun.Projectile.Rubbish.Structures;

namespace Talifun.Projectile.Examples.Client2
{
    class Program
    {
        static int Main(string[] args)
        {
            var port = 9000;
            var ipAddress = IPAddress.Parse("127.0.0.1");

            try
            {
                var blockingBufferManager = new BlockingBufferManager(65000, 32);
                Thread.Sleep(100);
                for (int i = 1; i < 5; i++)
                { 
                    using (Udt.Socket client = new Udt.Socket(AddressFamily.InterNetwork, SocketType.Stream))
                    {
                        client.Connect(ipAddress, port);

                        var sendFileRequest = new SendFileRequest
                        {
                            FilePath = string.Format("c:\\Temp\\temp{0}.pdf", i)
                        };

                        Console.WriteLine("Requesting file {0}", sendFileRequest.FilePath);
                        client.Write(blockingBufferManager, sendFileRequest);
                    }
                }

                Console.ReadKey(true);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error receiving file: {0}", ex.Message);
                Console.ReadKey(true);
                return 2;
            }
        }
    }
}
