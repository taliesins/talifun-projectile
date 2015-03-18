using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Talifun.Projectile.Command;
using Talifun.Projectile.Rubbish.Structures;

namespace Talifun.Projectile.Examples.Client
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
                using (Udt.Socket client = new Udt.Socket(AddressFamily.InterNetwork, SocketType.Stream))
                {
                    client.Connect(ipAddress, port);

                    var sendFileRequest = new SendFileRequest
                    {
                        FilePath = "c:\\Temp\\temp.pdf"
                    };

                    Console.WriteLine("Requesting file {0}", sendFileRequest.FilePath);
                    client.Write(blockingBufferManager, sendFileRequest);

                    Console.WriteLine("Requesting file {0}", sendFileRequest.FilePath);
                    client.Write(blockingBufferManager, sendFileRequest);

                    Console.WriteLine("Requesting file {0}", sendFileRequest.FilePath);
                    client.Write(blockingBufferManager, sendFileRequest);

                    Console.WriteLine("Requesting file {0}", sendFileRequest.FilePath);
                    client.Write(blockingBufferManager, sendFileRequest);
                    
                    //client.WriteRead(blockingBufferManager, sendFileRequest);

                    //// Send name information of the requested file
                    //string name = args[2];
                    //byte[] nameBytes = Encoding.UTF8.GetBytes(name);

                    //client.Send(BitConverter.GetBytes(nameBytes.Length), 0, sizeof(int));
                    //client.Send(nameBytes);

                    //// Get size information
                    //long size;
                    //byte[] file = new byte[1024];

                    //client.Receive(file, 0, sizeof(long));
                    //size = BitConverter.ToInt64(file, 0);

                    //// Receive the file
                    //string localName = args[3];
                    //client.ReceiveFile(localName, size);
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
