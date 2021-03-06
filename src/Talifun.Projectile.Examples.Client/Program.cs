﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Talifun.Projectile.Command;
using Talifun.Projectile.Protocol;

namespace Talifun.Projectile.Examples.Client
{
    class Program
    {
        static int Main(string[] args)
        {
            var port = 9000;
            var ipAddress = IPAddress.Parse("127.0.0.1");
            var bufferSize = 65000;
            var bufferCount = 32;

            try
            {
                var blockingBufferManager = new BlockingBufferManager(bufferSize, bufferCount);
                Thread.Sleep(300);
                using (var client = new Udt.Socket(AddressFamily.InterNetwork, SocketType.Stream))
                {
                    client.Connect(ipAddress, port);

                    for (var i = 1; i < 5; i++)
                    {
                        var sendFileRequest = new SendFileRequest
                        {
                            RemoteFilePath = string.Format("c:\\Temp\\test.pdf", i),
                            LocalFilePath = string.Format("c:\\Temp\\reply1-temp{0}.pdf", i)
                        };

                        Console.WriteLine("Requesting file {0}", sendFileRequest.RemoteFilePath);
                        client.WriteRead(blockingBufferManager, sendFileRequest); 
                    }

                    client.Close();
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
