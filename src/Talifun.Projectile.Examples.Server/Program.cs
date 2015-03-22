using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Talifun.Projectile.Protocol;
using SocketError = Udt.SocketError;

namespace Talifun.Projectile.Examples.Server
{
    class Program
    {
        static int Main(string[] args)
        {
            var port = 9000;
            var maxQueuedConnections = 10;
            var exitEventLoop = false;
            try
            {
                using (var poller = new Udt.SocketPoller())
                {
                    using (var server = new Udt.Socket(AddressFamily.InterNetwork, SocketType.Stream))
                    {
                        var blockingBufferManager = new BlockingBufferManager(server.UdpReceiveBufferSize, maxQueuedConnections * 2);

                        server.Bind(IPAddress.Any, port);
                        Console.WriteLine("Server is ready at port: {0}", port);
                        server.Listen(maxQueuedConnections);
                        Console.WriteLine("Server queue size: {0}", maxQueuedConnections);
                        Console.WriteLine("Press any key to exit");
                        poller.AddSocket(server);
                        
                        while (!exitEventLoop)
                        {
                            var newClient = server.Accept();
                            
                            var client = newClient;
                            Task.Run(() =>
                            {
                                try
                                {
                                    ProcessRequest(client, blockingBufferManager);
                                }
                                catch (Udt.SocketException socketException)
                                {
                                    if (socketException.SocketErrorCode == SocketError.ConnectionLost)
                                    {
                                        Console.Error.WriteLine("Client closed connection: ({0})", client.GetSocketId());
                                    }
                                    else
                                    {
                                        Console.Error.WriteLine("Error processing request: ({0}) {1}", client.GetSocketId(), socketException.Message);
  
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.Error.WriteLine("Error processing request: ({0}) {1}", client.GetSocketId(), ex.Message);
                                }  
                            });

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

        private static void ProcessRequest(Udt.Socket client, BlockingBufferManager blockingBufferManager)
        {
            IPEndPoint ipAddress = null;
            int socketId = 0;
            TimeSpan timeOut = TimeSpan.FromSeconds(30);
            bool exitListener = false;
            using (var poller = new Udt.SocketPoller())
            {
                poller.AddSocket(client);
                do
                {
                    if (ipAddress == null)
                    {
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
                        socketId = client.GetSocketId();
                    }

                    Console.WriteLine("Start processing connection from ({0}) {1}:{2}", socketId,
                        ipAddress == null ? "Unknown" : ipAddress.Address.ToString(),
                        ipAddress == null ? "unknown" : ipAddress.Port.ToString());

                    client.Read(blockingBufferManager);

                    Console.WriteLine("Finished processing connection from ({0}) {1}:{2}", socketId,
                        ipAddress == null ? "Unknown" : ipAddress.Address.ToString(),
                        ipAddress == null ? "unknown" : ipAddress.Port.ToString());

                    if (poller.Wait(timeOut))
                    {
                        exitListener = !poller.ReadSockets.Any() && !poller.WriteSockets.Any();
                    }
                    else
                    {
                        exitListener = true;
                    }

                } while (!exitListener);
            }
        }
    }
}
