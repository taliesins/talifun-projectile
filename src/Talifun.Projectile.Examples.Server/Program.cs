using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Talifun.Projectile.Rubbish.Structures;

namespace Talifun.Projectile.Examples.Server
{
    class Program
    {
        static int Main(string[] args)
        {
            var port = 9000;
            var maxQueuedConnections = 10;
            var exitEventLoop = false;
            var checkFrequency = TimeSpan.FromSeconds(1);
            try
            {
                var blockingBufferManager = new BlockingBufferManager(65000, 32);
                using (var poller = new Udt.SocketPoller())
                {
                    using (var server = new Udt.Socket(AddressFamily.InterNetwork, SocketType.Stream))
                    {
                        server.Bind(IPAddress.Any, port);
                        Console.WriteLine("Server is ready at port: {0}", port);
                        server.Listen(maxQueuedConnections);
                        Console.WriteLine("Server queue size: {0}", maxQueuedConnections);
                        Console.WriteLine("Press any key to exit");
                        poller.AddSocket(server);
                        
                        while (!exitEventLoop)
                        {
                            if (poller.Wait(checkFrequency))
                            {
                                foreach (var readSocket in poller.ReadSockets)
                                {
                                    var socket = readSocket;
                                    Task.Run(() =>
                                    {
                                        using (var listenerPoller = new Udt.SocketPoller())
                                        {
                                            using (var client = socket.Accept())
                                            {
                                                try
                                                {
                                                    IPEndPoint ipAddress = null;
                                                    var maxRetries = 0;
                                                    while (ipAddress == null)
                                                    {
                                                        try
                                                        {
                                                            ipAddress = client.RemoteEndPoint;
                                                        }
                                                        catch (Udt.SocketException exception)
                                                        {
                                                            maxRetries++;
                                                            if (maxRetries > 5)
                                                            {
                                                                throw;
                                                            }
                                                            Thread.Yield();
                                                        }
                                                    }
                                                    listenerPoller.AddSocket(client);
                                                    ProcessRequest(ipAddress, listenerPoller, blockingBufferManager);
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.Error.WriteLine("Error processing request: ({0}) {1}", client.GetSocketId(), ex.Message);
                                                    throw;
                                                }

                                            }
                                        }
                                    });
                                }
                            }

                            exitEventLoop = Console.KeyAvailable;
                        }
                    }
                }
                Console.ReadKey(true);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error sending file: {0}", ex.Message);
                Console.ReadKey(true);
                return 2;
            }
        }

        private static void ProcessRequest(IPEndPoint ipAddress, Udt.SocketPoller poller, BlockingBufferManager blockingBufferManager)
        {
            var requestHandlerTimeoutInactivity = TimeSpan.FromSeconds(1);

            do
            {
                foreach (var client in poller.ReadSockets)
                {
                    var socketId = client.GetSocketId();

                    Console.WriteLine("Start processing connection from ({0}) {1}:{2}", socketId, ipAddress == null ? "Unknown" : ipAddress.Address.ToString(), ipAddress == null ? "unknown" : ipAddress.Port.ToString());

                    client.Read(blockingBufferManager);

                    //// Receive file name from client
                    //byte[] file = new byte[1024];
                    //int length;
                    //string name;

                    //client.Receive(file, 0, sizeof(int));
                    //length = BitConverter.ToInt32(file, 0);

                    //client.Receive(file, 0, length);
                    //name = Encoding.UTF8.GetString(file, 0, length);

                    //// Send file size information
                    //client.Send(BitConverter.GetBytes(new FileInfo(name).Length), 0, sizeof(long));

                    //Udt.TraceInfo trace = client.GetPerformanceInfo();

                    //// Send the file
                    //client.SendFileRequest(name);

                    //trace = client.GetPerformanceInfo();

                    //PrintProps("Total", trace.Total);
                    //PrintProps("Local", trace.Local);
                    //PrintProps("Probe", trace.Probe);

                    Console.WriteLine("Finished processing connection from ({0}) {1}:{2}", socketId, ipAddress == null ? "Unknown" : ipAddress.Address.ToString(), ipAddress == null ? "unknown" : ipAddress.Port.ToString());

                    //Console.WriteLine("Finished processing connection from {0}", client.RemoteEndPoint);
                }
            } while (poller.Wait(requestHandlerTimeoutInactivity));
        }

        static void PrintProps(string name, object obj)
        {
            Console.WriteLine("---- {0} ----", name);
            foreach (PropertyInfo prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                Console.WriteLine("{0} = {1}", prop.Name, prop.GetValue(obj, null));
            }
        }
    }
}
